// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Numerics;
using Canopy.Extensions;
using Canopy.Rendering;
using Synesthesia.Utils;
using Synesthesia.Utils.Events;
using static SDL3.SDL;
using Log = Serilog.Log;

namespace Canopy;

public class SDL3WindowHost(ICanopyPlatform platform) : IDisposable
{
    private const int default_width = 1920;
    private const int default_height = 1080;
    private const int events_per_peep = 64;
    private const string process_name = "canopy_wallpaper_window";

    private const WindowFlags window_creation_flags = WindowFlags.Resizable |
                                                      WindowFlags.HighPixelDensity |
                                                      WindowFlags.OpenGL |
                                                      WindowFlags.Borderless |
                                                      WindowFlags.Hidden; // prevent white flash. Unhide after the first swap

    private readonly ConcurrentQueue<Action> commandQueue = new();

    public EventDispatcher<bool> ExitRequested { get; } = new();
    public EventDispatcher<Vector2> OnWindowResized { get; } = new EventDispatcher<Vector2>();
    public EventDispatcher<bool> OnWindowFocusChange { get; } = new EventDispatcher<bool>();
    public EventDispatcher<Nothing> OnDeviceLowMemory { get; } = new EventDispatcher<Nothing>();
    public EventDispatcher<SystemTheme> OnSystemThemeChanged { get; } = new EventDispatcher<SystemTheme>();

    public Vector2 Size { get; private set; } = Vector2.Zero;

    public bool Resizable { get; set; } = true;

    public readonly ICanopyPlatform Platform = platform;

    public bool WindowFocused { get; private set; }

    /// <summary>
    /// OpenGL Surface containing <see cref="OpenGLSurface.WindowHandle"/>, <see cref="OpenGLSurface.ContextHandle"/> and responsible
    /// for swapping buffers and managing ownership of OpenGL Context
    /// </summary>
    public OpenGLSurface Surface { get; private set; } = null!;

    /// <summary>
    /// Manages everything related to rendering/drawing
    /// </summary>
    public OpenGLRenderer Renderer { get; private set; } = null!;

    public bool WindowExists { get; private set; }

    public bool IsWayland => string.Equals(GetCurrentVideoDriver(), "wayland", StringComparison.Ordinal);

    private readonly Event[] events = new Event[events_per_peep];

    public Vector2 WindowSize
    {
        get
        {
            GetWindowSizeInPixels(Surface.WindowHandle, out var x, out var y).ThrowIfFailed();
            return new Vector2(x, y);
        }
    }

    public void Schedule(Action action) => commandQueue.Enqueue(action);

    /// <summary>
    /// Initializes SDL3 and creates <see cref="OpenGLSurface"/> and <see cref="OpenGlRenderer"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">Failed to create SDL window</exception>
    /// <exception cref="InvalidOperationException">Failed to create GL Context</exception>
    public void Initialize()
    {
        try
        {
            SetHint(Hints.AppName, process_name).LogErrorIfFailed();

            if (!Init(InitFlags.Video))
                throw new InvalidOperationException($"Failed to initialise SDL: {GetError()}");

            var version = GetVersion();
            var major = VersionNumMajor(version);
            var minor = VersionNumMinor(version);
            var micro = VersionNumMicro(version);
            var revision = GetRevision();
            var videoDriver = GetCurrentVideoDriver();

            Log.Debug("SDL 3 Initialized");
            Log.Debug("- Version:         {version}", $"{major}.{minor}.{micro}");
            Log.Debug("- Revision:        {revision}", revision);
            Log.Debug("- Video Driver:    {videoDriver}", videoDriver);

            SetLogOutputFunction(handleSdlLog, IntPtr.Zero);

            SetHint(Hints.WindowsCloseOnAltF4, "0").LogErrorIfFailed();
            SetHint(Hints.IMEImplementedUI, "composition").LogErrorIfFailed();

            IntPtr? windowHandle = CreateWindow(process_name, default_width, default_height, window_creation_flags);
            if (windowHandle == null) throw new InvalidOperationException($"Failed to create SDL window. SDL Error: {GetError()}");

            StopTextInput(windowHandle.Value).LogErrorIfFailed();

            GLSetAttribute(GLAttr.ContextMajorVersion, 3).LogErrorIfFailed();
            GLSetAttribute(GLAttr.ContextMinorVersion, 3).LogErrorIfFailed();
            GLSetAttribute(GLAttr.ContextProfileMask, (int)GLProfile.Core).LogErrorIfFailed();
            GLSetAttribute(GLAttr.StencilSize, 8).LogErrorIfFailed();

            IntPtr? glContext = GLCreateContext(windowHandle.Value);
            if (glContext == null) throw new InvalidOperationException($"Failed to create GL Context. SDL Error: {GetError()}");

            GLSetSwapInterval(0).LogErrorIfFailed();

            Surface = new OpenGLSurface
            {
                WindowHandle = windowHandle.Value,
                ContextHandle = glContext.Value
            };

            Surface.ClaimOwnership();

            Renderer = new OpenGLRenderer()
            {
                Surface = Surface,
            };

            Renderer.Initialize();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to initialize sdl3 window");
#if DEBUG
            Environment.Exit(exception.HResult);
#endif
        }
    }

    /// <summary>
    /// Marks the window as ready and starts pumping window events
    /// Do mind that the window starts as hidden until the first swap to prevent flashing the user with a white empty window
    /// </summary>
    public void RunWindow()
    {
        WindowExists = true;

        var nativeOsHandle = IntPtr.Zero;
        var windowProperties = GetWindowProperties(Surface.WindowHandle);

        switch (Platform.Platform)
        {
            case RuntimeInfo.Platform.Windows:
                nativeOsHandle = GetPointerProperty(windowProperties, Props.WindowWin32HWNDPointer, 0);
                break;
            case RuntimeInfo.Platform.macOS:
                nativeOsHandle = GetPointerProperty(windowProperties, Props.WindowCocoaWindowPointer, 0);
                break;
            default:
            {
                if (RuntimeInfo.IsMobile) throw new InvalidOperationException("how even did you get this running on mobile");
                if (IsWayland) throw new InvalidOperationException("wayland not supported");

                nativeOsHandle = GetPointerProperty(windowProperties, Props.WindowX11WindowNumber, 0);
                break;
            }
        }

        if (nativeOsHandle != IntPtr.Zero)
        {
            Log.Verbose("Extracted native OS handle pointer. Triggering desktop injection hook: {handle}", nativeOsHandle);
            Platform.InjectIntoDesktop(nativeOsHandle);
        }
        else
        {
            throw new InvalidOperationException("Failed to retrieve native window handle from SDL3 properties");
        }

        // ShowWindow(Surface.WindowHandle).LogErrorIfFailed();

        Loop();
    }

    protected void Loop()
    {
        try
        {
            while (WindowExists) ProcessFrame();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to loop");
        }

        Dispose();
    }

    protected void ProcessFrame()
    {
        if (!WindowExists) return;
        while (!commandQueue.IsEmpty)
        {
            if (commandQueue.TryDequeue(out var action))
            {
                action.Invoke();
            }
        }

        Renderer.Surface.ClaimOwnership();
        Renderer.OpenGL.ClearColor(1.0f, 0.0f, 1.0f, 1.0f);
        Renderer.BeginDrawing();
        Renderer.EndDrawing();

        pollEvents();
    }

    private void pollEvents()
    {
        PumpEvents();
        int eventsRead;

        do
        {
            eventsRead = PeepEvents(events, events_per_peep, EventAction.GetEvent, (uint)EventType.First, (uint)EventType.Last).LogErrorIfFailed();
            for (int i = 0; i < eventsRead; i++) HandleEvent(events[i]);
        } while (eventsRead == events_per_peep);
    }

    protected void HandleEvent(Event sdlEvent)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        switch ((EventType)sdlEvent.Type)
        {
            case EventType.Quit:
            {
                ExitRequested.Dispatch(true);
                WindowExists = false;
                break;
            }

            case EventType.LowMemory:
                OnDeviceLowMemory.Dispatch(Nothing.INSTANCE);
                break;

            case EventType.WindowResized:
                OnWindowResized.Dispatch(WindowSize);
                Renderer.Resize(sdlEvent.Window.Data1, sdlEvent.Window.Data2);
                break;

            case EventType.SystemThemeChanged:
                OnSystemThemeChanged.Dispatch(GetSystemTheme());
                break;

            case EventType.WindowFocusLost:
                WindowFocused = false;
                OnWindowFocusChange.Dispatch(WindowFocused);
                break;
            case EventType.WindowFocusGained:
                WindowFocused = true;
                OnWindowFocusChange.Dispatch(WindowFocused);
                break;
        }
    }

    private static void handleSdlLog(IntPtr userData, LogCategory logCategory, LogPriority priority, string message) => Log.Verbose(message);


    public void Dispose()
    {
        ExitRequested.Dispose();
        OnWindowFocusChange.Dispose();
        OnWindowResized.Dispose();
        OnDeviceLowMemory.Dispose();
        OnSystemThemeChanged.Dispose();
        Surface.Dispose();
        Renderer.Dispose();
        DestroyWindow(Surface.WindowHandle);
    }
}
