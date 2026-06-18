// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Utils;

public static class Precision
{
    public static bool IsSame(float value1, float value2, float precision = 0.00001f)
    {
        return MathUtil.Abs(value1 - value2) < precision;
    }

    public static bool IsSame(double value1, double value2, double precision = 0.00001)
    {
        return MathUtil.Abs(value1 - value2) < precision;
    }
}
