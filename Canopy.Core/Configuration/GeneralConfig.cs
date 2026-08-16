// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Configuration;

public record GeneralConfig(
    int RefreshPeriod,
    Wallpaper.FitMode FitMode,
    bool UseSolarNoonAsMidday
)
{
    public static readonly GeneralConfig DEFAULT = new GeneralConfig( 60_000, Wallpaper.FitMode.Fill, true);

    public static readonly StructCodec<GeneralConfig> CODEC = StructCodec.For<GeneralConfig>()
        .Field("RefreshPeriod", Codecs.INT, g => g.RefreshPeriod)
        .Field("FitMode", Codecs.Enum<Wallpaper.FitMode>(), g => g.FitMode)
        .Field("UseSolarNoonAsMidday", Codecs.BOOLEAN, g => g.UseSolarNoonAsMidday)
        .Build((refresh, fitMode, noon) => new GeneralConfig(refresh, fitMode, noon));
}
