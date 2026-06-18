// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Canopy.Graphics;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Vertex2D(
    Vector2 position,
    Vector2 size,
    Vector2 texCoord,
    uint color,
    float alpha,
    float radius,
    Vector2 localUv
)
{
    [VertexInfo(0, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 Position = position;

    [VertexInfo(1, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 TextureCoord = texCoord;

    [VertexInfo(2, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 Size = size;

    [VertexInfo(3, 4, VertexAttribPointerType.UnsignedByte, normalized: true)]
    public readonly uint Color = color;

    [VertexInfo(4, 1, VertexAttribPointerType.Float)]
    public readonly float Alpha = alpha;

    [VertexInfo(5, 1, VertexAttribPointerType.Float)]
    public readonly float Radius = radius;

    [VertexInfo(6, 2, VertexAttribPointerType.Float)]
    public readonly Vector2 LocalUV = localUv;
}
