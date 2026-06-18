// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Utils;

public static class MathUtil
{
    public static float ToRads(this float deg)
    {
        return deg * (MathF.PI / 180f);
    }
}
