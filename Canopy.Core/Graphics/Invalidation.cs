// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Graphics;

[Flags]
public enum Invalidation
{
    None = 0,

    /// <summary>
    /// Affects ScreenSpace:
    /// Position, Rotation, Scale
    /// Parent -> Child
    /// </summary>
    Geometry = 1 << 0,

    /// <summary>
    /// Affects internal layout:
    /// Flow, Centering, Anchor
    /// No propagation
    /// </summary>
    Layout = 1 << 1,

    /// <summary>
    /// Affects the parent's size or visibility:
    /// AutoSize, RelativeSize
    /// Child -> Parent
    /// </summary>
    Size = 1 << 2,

    /// <summary>
    /// Affects visual state:
    /// Color, Texture, Shaders
    /// No propagation
    /// </summary>
    DrawNode = 1 << 3,

    All = Geometry | Layout | Size | DrawNode,
}
