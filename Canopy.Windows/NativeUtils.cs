// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Windows;

public static class NativeUtils
{
    public static bool HasExtendedStyle(IntPtr hwnd, uint style)
    {
        if (hwnd == IntPtr.Zero)
            return false;

        IntPtr exStylePtr = Native.GetWindowLongPtr(hwnd, (int)Native.GWL.GWL_EXSTYLE);
        if (exStylePtr == IntPtr.Zero)
            return false;

        return (exStylePtr.ToInt64() & style) != 0;
    }

    public static IntPtr GetProgman() => Native.FindWindow("Progman", null!);

    public static IntPtr GetDesktopWorkerW()
    {
        var progman = GetProgman();
        var workerWOrig = IntPtr.Zero;
        var folderView = Native.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null!);
        if (folderView != IntPtr.Zero)
            return workerWOrig != IntPtr.Zero ? workerWOrig : progman;

        //If the desktop isn't under Progman, cycle through the WorkerW handles and find the correct one
        do
        {
            workerWOrig = Native.FindWindowEx(Native.GetDesktopWindow(), workerWOrig, "WorkerW", null!);
            folderView = Native.FindWindowEx(workerWOrig, IntPtr.Zero, "SHELLDLL_DefView", null!);
        } while (folderView == IntPtr.Zero && workerWOrig != IntPtr.Zero);

        // Win 11
        return workerWOrig != IntPtr.Zero ? workerWOrig : progman;
    }

    public static IntPtr GetLastChildWindow(IntPtr parent)
    {
        IntPtr lastChild = IntPtr.Zero;

        Native.EnumChildWindows(parent, (hWnd, lParam) =>
        {
            lastChild = hWnd;
            return true;
        }, IntPtr.Zero);

        return lastChild;
    }
}
