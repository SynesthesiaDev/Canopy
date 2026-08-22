// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Providers.VisualCrossing;

public record CurrentConditions(
    double Humidity,
    double Precip,
    double Snow,
    double SnowDepth,
    double Visibility,
    double CloudCover,
    string Conditions,
    string Icon
)
{
    public static readonly Codec<CurrentConditions> CODEC = StructCodec
        .For<CurrentConditions>()
        .Field("humidity", Codecs.DOUBLE, c => c.Humidity)
        .Field("precip", Codecs.DOUBLE, c => c.Precip)
        .Field("snow", Codecs.DOUBLE, c => c.Snow)
        .Field("snowdepth", Codecs.DOUBLE, c => c.SnowDepth)
        .Field("visibility", Codecs.DOUBLE, c => c.Visibility)
        .Field("cloudcover", Codecs.DOUBLE, c => c.CloudCover)
        .Field("conditions", Codecs.STRING, c => c.Conditions)
        .Field("icon", Codecs.STRING, c => c.Icon)
        .Build((humidity, precip, snow, snowdepth, visibility, cloudcover, conditions, icon) => new CurrentConditions(humidity, precip, snow, snowdepth, visibility, cloudcover, conditions, icon));
}
