// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Rendering;


[Flags]
public enum ClearFlags
{
    None = 0,
    ColorBuffer = 1,
    DepthBuffer = 2,
    StencilBuffer = 4,
    CoverageBuffer = 8,
    All = ColorBuffer | DepthBuffer | StencilBuffer | CoverageBuffer
}
