// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Graphics;

[Flags]
public enum Axes
{
    None = 0,
    X = 1 << 0,
    Y = 1 << 1,
    Both = X | Y,
}
