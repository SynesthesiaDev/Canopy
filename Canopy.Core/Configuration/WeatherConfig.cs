// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Optionals;

namespace Canopy.Configuration;

public record WeatherConfig(
    bool UseAutoLocation,
    WeatherConfig.OfflineMode OfflineFallback,
    WeatherConfig.ManualCoordinates? Coordinates,
    string? VisualCrossingApiKey = null
)
{
    public static readonly WeatherConfig DEFAULT = new WeatherConfig(true, OfflineMode.UseLastKnownState, new ManualCoordinates(Latitude: 50.087555, Longitude: 14.421194));

    public static readonly StructCodec<WeatherConfig> CODEC = StructCodec.For<WeatherConfig>()
        .Field("UseAutoLocation", Codecs.BOOLEAN, w => w.UseAutoLocation)
        .Field("OfflineFallback", Codecs.Enum<OfflineMode>(), w => w.OfflineFallback)
        .Field("Coordinates", ManualCoordinates.CODEC.Optional(), w => w.Coordinates.ToOptional())
        .Field("VisualCrossingApiKey", Codecs.STRING.Optional(), w => w.VisualCrossingApiKey.ToOptional())
        .Build((b, arg3, arg4, apiKey) => new WeatherConfig(b, arg3, arg4.ToNullableClass(), apiKey.ToNullableClass()));

    public enum OfflineMode
    {
        UseLastKnownState,
        IgnoreWeather
    }

    public record ManualCoordinates(double Longitude, double Latitude)
    {
        public static readonly StructCodec<ManualCoordinates> CODEC = StructCodec.For<ManualCoordinates>()
            .Field("Longitude", Codecs.DOUBLE, m => m.Longitude)
            .Field("Latitude", Codecs.DOUBLE, m => m.Latitude)
            .Build((lon, lat) => new ManualCoordinates(lon, lat));
    }
}
