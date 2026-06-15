// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Canopy.Rendering;
using SynesthesiaDev.Chibi.Core;
using Vanara.PInvoke;
using static Vanara.PInvoke.Gdi32;
using static Vanara.PInvoke.User32;

namespace Canopy.Windows;

public class Gdi32Surface : IWindowSurface
{
    public required ChibiWindow Window { get; init; }
    public required IntPtr Handle { get; init; }
    public required INativeContext NativeContext { get; init; }

    private IntPtr hglrc = IntPtr.Zero;
    private SafeReleaseHDC hdc = SafeReleaseHDC.Null;

    public Vector2 GetScreenSize() => Window.ScreenResolution;

    public void SwapBuffers()
    {
        if (!hdc.IsInvalid)
        {
            Gdi32.SwapBuffers(hdc);
        }
    }

    public void InitializeGraphicsContext()
    {
        hdc = GetDC(Handle);
        if (hdc == IntPtr.Zero) throw new InvalidOperationException("Failed to get Device Context (HDC)");

        var pfd = new PIXELFORMATDESCRIPTOR();
        pfd.nSize = (ushort)Marshal.SizeOf(pfd);
        pfd.nVersion = 1;
        pfd.dwFlags = PFD_FLAGS.PFD_DRAW_TO_WINDOW | PFD_FLAGS.PFD_SUPPORT_OPENGL | PFD_FLAGS.PFD_DOUBLEBUFFER;
        pfd.iPixelType = PFD_TYPE.PFD_TYPE_RGBA;
        pfd.cColorBits = 32;
        pfd.cDepthBits = 24;
        pfd.cStencilBits = 8;

        var pixelFormat = ChoosePixelFormat(hdc, in pfd);
        if (pixelFormat == 0) throw new InvalidOperationException("Failed to choose suitable pixel format");

        if (!SetPixelFormat(hdc, pixelFormat, in pfd))
            throw new InvalidOperationException("Failed to set pixel format on HDC");

        hglrc = wglCreateContext(hdc.DangerousGetHandle());
        if (hglrc == IntPtr.Zero) throw new InvalidOperationException("Failed to create wgl context");

        if (!wglMakeCurrent(hdc.DangerousGetHandle(), hglrc))
            throw new InvalidOperationException("Failed to make WGL context current");

    }

    [DllImport("opengl32.dll")]
    private static extern IntPtr wglCreateContext(IntPtr hdc);

    [DllImport("opengl32.dll")]
    private static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

    [DllImport("opengl32.dll")]
    private static extern bool wglDeleteContext(IntPtr hglrc);

    public void Dispose()
    {
        wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);

        if (hglrc != IntPtr.Zero)
        {
            wglDeleteContext(hglrc);
            hglrc = IntPtr.Zero;
        }

        if (!hdc.IsInvalid)
        {
            ReleaseDC(Handle, hdc);
            hdc.Dispose();
        }
    }
}
