// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Configuration;

public record GeneralConfig(
    bool AutoStartOnStartup,
    int RefreshPeriod,
    Wallpaper.FitMode FitMode,
    bool UseSolarNoonAsMidday
)
{
    public static readonly GeneralConfig DEFAULT = new GeneralConfig(true, 60_000, Wallpaper.FitMode.Fill, true);

    public static readonly StructCodec<GeneralConfig> CODEC = StructCodec.For<GeneralConfig>()
        .Field("AutoStartOnStartup", Codecs.BOOLEAN, g => g.AutoStartOnStartup)
        .Field("RefreshPeriod", Codecs.INT, g => g.RefreshPeriod)
        .Field("FitMode", Codecs.Enum<Wallpaper.FitMode>(), g => g.FitMode)
        .Field("UseSolarNoonAsMidday", Codecs.BOOLEAN, g => g.UseSolarNoonAsMidday)
        .Build((autoStart, refresh, fitMode, noon) => new GeneralConfig(autoStart, refresh, fitMode, noon));
}
