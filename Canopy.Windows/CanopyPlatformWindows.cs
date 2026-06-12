// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Serilog;
using Synesthesia.Utils;
using WinEventHook;

namespace Canopy.Windows;

public class CanopyPlatformWindows : ICanopyPlatform
{
    public RuntimeInfo.Platform Platform => RuntimeInfo.Platform.Windows;

    private IntPtr workerW, progman, shellDllDefView, originalWorkerW;
    private bool layeredShellView;
    private WindowEventHook workerWHook = null!;
    private IntPtr windowHandle = IntPtr.Zero;

    public void Initialize()
    {
        var sdl = new SDL3WindowHost(this);
        sdl.Initialize();
        sdl.RunWindow();
    }

    // !! This is not AI-made, I am just leaving comments here
    // for whoever wants to do this in the future and wants to learn from this source code
    // <3

    public void InjectIntoDesktop(IntPtr sdlWindowHandle)
    {
        windowHandle = sdlWindowHandle;
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

        if (layeredShellView)
            workerW = Native.FindWindowEx(progman, IntPtr.Zero, "WorkerW", IntPtr.Zero);

        originalWorkerW = NativeUtils.GetDesktopWorkerW();

        Log.Information("WorkerW created ({workerW})", workerW);

        attachWindowToWorkerW(sdlWindowHandle);

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
            InjectIntoDesktop(windowHandle);

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
        Native.SetParent(sdlWindowHandle, workerW);

        var windowFlags = (int)(
            Native.SetWindowPosFlags.SWP_NOACTIVATE |
            Native.SetWindowPosFlags.SWP_SHOWWINDOW
        );

        Native.SetWindowPos(
            sdlWindowHandle,
            (int)Native.HWNDInsertAfter.HWND_TOP,
            0,
            0,
            Native.GetSystemMetrics((int)Native.SystemMetric.SM_CXVIRTUALSCREEN),
            Native.GetSystemMetrics((int)Native.SystemMetric.SM_CYVIRTUALSCREEN),
            windowFlags
        );

        ensureWorkerWzOrder();

        Log.Information("Wallpaper attached to WorkerW.");
    }

    private void ensureWorkerWzOrder()
    {
        if (!layeredShellView) return;

        var lastchild = NativeUtils.GetLastChildWindow(progman);
        Log.Information("Last child of Progman: {lastChild}", NativeUtils.GetLastChildWindow(progman));
        Log.Information("WorkerW: {workerW}", workerW);

        if (lastchild != workerW)
        {
            var windowFlags = (int)(Native.SetWindowPosFlags.SWP_NOMOVE
                                    | Native.SetWindowPosFlags.SWP_NOSIZE
                                    | Native.SetWindowPosFlags.SWP_NOACTIVATE);

            Native.SetWindowPos(workerW, (int)Native.HWNDInsertAfter.HWND_BOTTOM, 0, 0, 0, 0, windowFlags);
        }
    }
}
