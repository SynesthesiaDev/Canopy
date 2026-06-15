// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Graphics;

public enum BlendMode : uint
{
    /// <summary>no blending: dstRGBA = srcRGBA</summary>
    None = 0,
    /// <summary>
    /// alpha blending: dstRGB = (srcRGB * srcA) + (dstRGB * (1-srcA)), dstA = srcA + (dstA * (1-srcA))
    /// </summary>
    Alpha = 1,
    /// <summary>
    /// additive blending: dstRGB = (srcRGB * srcA) + dstRGB, dstA = dstA
    /// </summary>
    Add = 2,
    /// <summary>color modulate: dstRGB = srcRGB * dstRGB, dstA = dstA</summary>
    Mod = 4,
    /// <summary>
    /// color multiply: dstRGB = (srcRGB * dstRGB) + (dstRGB * (1-srcA)), dstA = dstA
    /// </summary>
    Mul = 8,
    /// <summary>
    /// pre-multiplied alpha blending: dstRGBA = srcRGBA + (dstRGBA * (1-srcA))
    /// </summary>
    BlendPremultiplied = 16, // 0x00000010
    /// <summary>
    /// pre-multiplied additive blending: dstRGB = srcRGB + dstRGB, dstA = dstA
    /// </summary>
    AddPremultiplied = 32, // 0x00000020
    Invalid = 2147483647, // 0x7FFFFFFF
}

