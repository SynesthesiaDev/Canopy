// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using Canopy.Rendering;

namespace Canopy.Windows;

public class WindowsGLContext : INativeContext
{
    public IntPtr GetProcAddress(string procName)
    {
        IntPtr addr = wglGetProcAddress(procName);

        // wglGetProcAddress returns 0, 1, 2, 3, or -1 if it fails or if the function is a core 1.1 function.
        if (addr == IntPtr.Zero || addr == 1 || addr == 2 || addr == 3 || addr == -1)
        {
            // fallback look inside opengl32.dll directly
            IntPtr module = GetModuleHandle("opengl32.dll");
            addr = GetProcAddress(module, procName);
        }

        return addr;
    }

    [DllImport("opengl32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr wglGetProcAddress(string lpszProc);

    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    public bool TryGetProcAddress(string procName, out IntPtr addr)
    {
        addr = GetProcAddress(procName);
        return addr != IntPtr.Zero;
    }
}
