// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using Canopy.Rendering;

namespace Canopy;

public interface IWindowSurface : IDisposable
{
    IntPtr Handle { get; }

    INativeContext NativeContext { get; }

    Vector2 GetScreenSize();
    void SwapBuffers();

    void InitializeGraphicsContext();
}
