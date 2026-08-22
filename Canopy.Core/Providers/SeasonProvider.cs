// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;

namespace Canopy.Providers;

public class SeasonProvider : IProvider<SeasonType>
{
    public SeasonType Get()
    {
        var now = DateTimeOffset.Now;
        float value = now.Month + (now.Day / 100f);

        if (value is < 3.21f or >= 12.22f)
            return SeasonType.Winter;
        if (value < 6.21f)
            return SeasonType.Spring;
        if (value < 9.23f)
            return SeasonType.Summer;

        return SeasonType.Autumn;
    }

    public void Dispose()
    {
    }
}
