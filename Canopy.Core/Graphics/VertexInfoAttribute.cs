// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Silk.NET.OpenGL;

namespace Canopy.Graphics;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class VertexInfoAttribute(int index, int count, VertexAttribPointerType type, bool normalized = false) : Attribute
{
    public int Index { get; } = index;
    public int Count { get; } = count;
    public VertexAttribPointerType Type { get; } = type;
    public bool Normalized { get; } = normalized;
}
