// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Configuration;

public record WeatherConfig(
    bool UseAutoLocation,
    int RefreshInterval,
    WeatherConfig.OfflineMode OfflineFallback,
    WeatherConfig.ManualCoordinates? Coordinates
)
{
    public static readonly WeatherConfig DEFAULT = new WeatherConfig(true, 60_000, OfflineMode.UseLastKnownState, new ManualCoordinates(Latitude: 50.087555, Longitude: 14.421194));

    public static readonly StructCodec<WeatherConfig> CODEC = StructCodec.For<WeatherConfig>()
        .Field("UseAutoLocation", Codecs.BOOLEAN, w => w.UseAutoLocation)
        .Field("RefreshInterval", Codecs.INT, w => w.RefreshInterval)
        .Field("OfflineFallback", Codecs.Enum<OfflineMode>(), w => w.OfflineFallback)
        .Field("Coordinates", ManualCoordinates.CODEC.Optional(), w => w.Coordinates.ToOptional())
        .Build((b, i, arg3, arg4) => new WeatherConfig(b, i, arg3, arg4.Value));

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
