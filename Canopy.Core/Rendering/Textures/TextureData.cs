// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.OpenGL;

namespace Canopy.Rendering.Textures;

public class TextureData(int width, int height, byte[] data, PixelFormat pixelFormat)
{
    public int Width { get; set; } = width;
    public int Height { get; set; } = height;
    public byte[] Data { get; set; } = data;
    public PixelFormat PixelFormat { get; set; } = pixelFormat;

    public override string ToString() => $"TextureData(Width={Width}, Height={Height}, Data={Data.Length} PixelFormat={PixelFormat})";
}
