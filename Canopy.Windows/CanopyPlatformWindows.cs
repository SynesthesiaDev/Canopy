// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Graphics;
using Canopy.Rendering;
using Canopy.Utils;
using Serilog;
using Synesthesia.Utils;
using SynesthesiaDev.Chibi.Core;
using SynesthesiaDev.Chibi.Core.Enums;
using Vanara.PInvoke;
using WinEventHook;

namespace Canopy.Windows;

public class CanopyPlatformWindows : ICanopyPlatform
{
    public RuntimeInfo.Platform Platform => RuntimeInfo.Platform.Windows;

    private IntPtr workerW, progman, shellDllDefView, originalWorkerW;
    private bool layeredShellView;
    private WindowEventHook workerWHook = null!;

    public OpenGLRenderer? OpenGL;
    public ChibiWindow Window = null!;

    public readonly WallpaperManager WallpaperManager;

    public CanopyPlatformWindows()
    {
        WallpaperManager = new WallpaperManager(this);
    }

    public void Initialize()
    {
        Window = new ChibiWindow();

        Window.OnWindowCreated.Subscribe(_ =>
        {
            var surface = new Gdi32Surface
            {
                Window = Window,
                Handle = Window.WindowHandle,
                NativeContext = new WindowsGLContext()
            };

            surface.InitializeGraphicsContext();

            OpenGL = new OpenGLRenderer
            {
                Surface = surface
            };

            OpenGL.Initialize();

            UserLand.EntryPoint(this);
        });

        Window.OnWindowResized.Subscribe(e =>
        {
            OpenGL?.Resize((int)e.Now.X, (int)e.Now.Y);
        });

        Window.OnFrame += () =>
        {
            if (OpenGL == null) return;

            OpenGL.BeginDrawing();
            WallpaperManager.Draw(OpenGL);
            OpenGL.EndDrawing();
        };

        // Start window hidden and unhide it after the first swap to prevent white flash
        Window.Run(952, 520, WindowFlags.Resizable | WindowFlags.HighPixelDensity | WindowFlags.Hidden);
    }

    public void HideWindow() => Window.WindowVisible = false;
    public void ShowWindow() => Window.WindowVisible = true;

    public void PushWallpaper(Wallpaper wallpaper)
    {
        WallpaperManager.PushWallpaper(wallpaper);
        // WallpaperManager.Wallpapers.Add(wallpaper);
    }

    // !! This is not AI-made, I am just leaving comments here
    // for whoever wants to do this in the future and wants to learn from this source code
    // <3

    public void InjectIntoDesktop(IntPtr chibiWindowHandle)
    {
        WindowDiagnostics.DumpTree(progman, "BEFORE injection");

        Log.Verbose("Creating WorkerW..");

        // Find progman
        progman = NativeUtils.GetProgman();

        // win 11 has changed how desktop rendering works to allow HDR and stuff like that. Read more below
        // https://github.com/rocksdanister/lively/issues/2074#issuecomment-2030662089
        layeredShellView = NativeUtils.HasExtendedStyle(progman, Native.WindowStyles.WS_EX_NOREDIRECTIONBITMAP);
        if (layeredShellView)
            Log.Information("Detected raised desktop with layered shell view");

        // Send timeout with 0x052C to progman. This tells progman to create a new
        // WorkerW window behind the desktop icons
        Native.SendMessageTimeout(
            progman,
            0x052C,
            new IntPtr(0xD),
            new IntPtr(0x1),
            Native.SendMessageTimeoutFlags.SMTO_NORMAL,
            1000,
            out _
        );

        Native.EnumWindows((handle, _) =>
        {
            var pointer = Native.FindWindowEx(handle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);

            if (pointer != IntPtr.Zero)
            {
                workerW = Native.FindWindowEx(IntPtr.Zero, handle, "WorkerW", IntPtr.Zero);
                shellDllDefView = pointer;
            }

            return true;
        }, IntPtr.Zero);

        if (workerW == IntPtr.Zero)
        {
            Log.Error("WorkerW not found after EnumWindows — SHELLDLL_DefView may still be in Progman");
            // Fallback: check directly inside Progman
            shellDllDefView = Native.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            workerW = progman; // parent to Progman itself as fallback
        }

        if (layeredShellView)
        {
            shellDllDefView = Native.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
            workerW = Native.FindWindowEx(progman, IntPtr.Zero, "WorkerW", IntPtr.Zero);
            Log.Information("LayeredShell: shellDllDefView={defView}, workerW={workerW}", shellDllDefView, workerW);
        }

        originalWorkerW = NativeUtils.GetDesktopWorkerW();

        Log.Information("WorkerW created ({workerW})", workerW);

        attachWindowToWorkerW(chibiWindowHandle);

        WindowDiagnostics.DumpTree(progman, "AFTER injection");

        WindowDiagnostics.DumpWindow(chibiWindowHandle, "Chibi Window");

        try
        {
            if (workerW != IntPtr.Zero)
            {
                Log.Information("Listening to WorkerW events..");
                var dwThreadId = Native.GetWindowThreadProcessId(workerW, out int dwProcess);
                workerWHook = new WindowEventHook(WindowEvent.EVENT_OBJECT_DESTROY);
                workerWHook.HookToThread(dwThreadId);
                workerWHook.EventReceived += WorkerWHook_EventReceived;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    private async void WorkerWHook_EventReceived(object? _, WinEventHookEventArgs e)
    {
        if (e.WindowHandle != workerW || e.EventType != WindowEvent.EVENT_OBJECT_DESTROY)
            return;

        Log.Error("WorkerW died");

        if (layeredShellView)
        {
            InjectIntoDesktop(Window.WindowHandle);

            // var windowFlags = (int)(Native.SetWindowPosFlags.SWP_NOMOVE |
            //                         Native.SetWindowPosFlags.SWP_NOSIZE |
            //                         Native.SetWindowPosFlags.SWP_NOACTIVATE);

            // foreach (var item in wallpapers) //only have one, later
            // {
            //     NativeMethods.SetWindowPos(item.Handle,
            //         (int)shellDLL_DefView,
            //         0,
            //         0,
            //         0,
            //         0,
            //         windowFlags);
            // }

            ensureWorkerWzOrder();
        }
        else
        {
            //reset wallpaper?
            // what does  that mean in lively? creating the window again? Is each wallpaper different window? thats janky
        }
    }

    private void attachWindowToWorkerW(IntPtr sdlWindowHandle)
{
    var styleFlags = User32.GetWindowLong(sdlWindowHandle, User32.WindowLongFlags.GWL_STYLE);

    styleFlags &= (int)~(User32.WindowStyles.WS_POPUP | User32.WindowStyles.WS_CAPTION | User32.WindowStyles.WS_THICKFRAME | User32.WindowStyles.WS_MINIMIZEBOX | User32.WindowStyles.WS_MAXIMIZEBOX);
    styleFlags |= (int)(User32.WindowStyles.WS_CHILD |
                        User32.WindowStyles.WS_CLIPSIBLINGS |
                        User32.WindowStyles.WS_CLIPCHILDREN);

    User32.SetWindowLong(sdlWindowHandle, User32.WindowLongFlags.GWL_STYLE, styleFlags);

    User32.SetWindowPos(sdlWindowHandle, 0, 0, 0, 0, 0,
        User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE |
        User32.SetWindowPosFlags.SWP_NOZORDER | User32.SetWindowPosFlags.SWP_FRAMECHANGED);

    var parent = layeredShellView ? progman : workerW;
    Native.SetParent(sdlWindowHandle, parent);

    Native.GetWindowRect(parent, out Native.RECT prct);

    var windowFlags = (int)(
        Native.SetWindowPosFlags.SWP_NOACTIVATE |
        Native.SetWindowPosFlags.SWP_SHOWWINDOW
    );

    if (layeredShellView && shellDllDefView != IntPtr.Zero)
    {
        // put window below the icon layer in Z-order (siblings under Progman)
        Native.SetWindowPos(
            sdlWindowHandle,
            (int)shellDllDefView,   // insert AFTER
            0, 0,
            prct.Right - prct.Left,
            prct.Bottom - prct.Top,
            windowFlags
        );
    }
    else
    {
        Native.SetWindowPos(
            sdlWindowHandle,
            (int)Native.HWNDInsertAfter.HWND_TOP,
            0, 0,
            prct.Right - prct.Left,
            prct.Bottom - prct.Top,
            windowFlags
        );
    }

    ensureWorkerWzOrder();
    Log.Information("Wallpaper attached (parent={parent}, layered={layered})", parent, layeredShellView);
}

    private void ensureWorkerWzOrder()
    {
        if (!layeredShellView) return;
        if (shellDllDefView == IntPtr.Zero || Window.WindowHandle == IntPtr.Zero) return;

        var windowFlags = (int)(Native.SetWindowPosFlags.SWP_NOMOVE
                                | Native.SetWindowPosFlags.SWP_NOSIZE
                                | Native.SetWindowPosFlags.SWP_NOACTIVATE);

        //window below icons, as a sibling under Progman
        Native.SetWindowPos(Window.WindowHandle, (int)shellDllDefView, 0, 0, 0, 0, windowFlags);
        Log.Information("Z-order re-asserted: SDL window below shellDllDefView");
    }

    // FUCKKK
}
