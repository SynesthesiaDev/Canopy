// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Extensions;
using static SDL3.SDL;
namespace Canopy.Rendering;

public sealed class OpenGLSurface : IDisposable
{
    /// <summary>
    /// Handle to the os window
    /// </summary>
    public required IntPtr WindowHandle { get; init; }

    /// <summary>
    /// Handle to the GL Context
    /// </summary>
    public required  IntPtr ContextHandle { get; init; }

    /// <summary>
    /// Window Width
    /// </summary>
    public int BackBufferWidth
    {
        get
        {
            GetWindowSize(WindowHandle, out var width, out _);
            return width;
        }
    }

    /// <summary>
    /// Window Height
    /// </summary>
    public int BackBufferHeight
    {
        get
        {
            GetWindowSize(WindowHandle, out _, out var height);
            return height;
        }
    }

    /// <summary>
    /// Get an OpenGL function by name.
    ///If the GL library is loaded at runtime with GLLoadLibrary(string?), then all GL functions must be retrieved this way. Usually this is used to retrieve function pointers to OpenGL extensions.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static nint GetProcAddress(string name) => GLGetProcAddress(name);

    /// <summary>
    /// Moves the ownership of <see cref="ContextHandle"/> to current thread
    /// </summary>
    /// <exception cref="SDLPlatformException">Ownership is not released/free</exception>
    public void ClaimOwnership() => GLMakeCurrent(WindowHandle, ContextHandle).ThrowIfFailed();

    /// <summary>
    /// Releases ownership of <see cref="ContextHandle"/> from thread
    /// </summary>
    /// <threadsafety>Function should be called on the same thread SDL has been initialized
    /// or on the thread which currently has ownership
    /// </threadsafety>
    /// <exception cref="SDLPlatformException">the ownership is already free or context is invalid</exception>
    public void ReleaseOwnership() => GLMakeCurrent(WindowHandle, IntPtr.Zero).ThrowIfFailed();

    /// <summary>
    /// Swaps window buffers
    /// </summary>
    /// <exception cref="SDLPlatformException">Ownership is free</exception>
    public void SwapBuffers() => GLSwapWindow(WindowHandle).ThrowIfFailed();

    /// <summary>
    /// Destroys the GL Context and clears ownership
    /// </summary>
    public void Dispose()
    {
        GLDestroyContext(ContextHandle);
    }
}

