// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Codec.Json;

namespace Canopy.Providers;

public class GeopositionProvider : IProvider<GeopositionProvider.GeoPosition>
{
    private const string geo_location_endpoint = "http://ip-api.com/json/?fields=lat,lon";

    private static readonly HttpClient http_client = new();
    private GeoPosition? cached;

    public void InvalidateCache()
    {
        cached = null;
    }

    public GeoPosition Get()
    {
        if (cached != null) return cached;

        var weatherConfig = Canopy.CurrentConfig.Weather;
        if (!weatherConfig.UseAutoLocation)
        {
            if (weatherConfig.Coordinates == null)
                throw new InvalidOperationException("Cannot have 'UseAutoLocation' disabled and have no coordinates specified");

            cached = new GeoPosition(weatherConfig.Coordinates.Latitude, weatherConfig.Coordinates.Longitude);
            return cached;
        }

        var request = new HttpRequestMessage(HttpMethod.Get, geo_location_endpoint);
        var res = http_client.Send(request);
        var body = res.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var geoPosition = GeoPosition.CODEC.Decode(JsonTranscoder.INSTANCE, body.ToJson());
        cached = geoPosition;
        return geoPosition;
    }

    public record GeoPosition(double Lat, double Lon)
    {
        public static readonly StructCodec<GeoPosition> CODEC = StructCodec.For<GeoPosition>()
            .Field("lat", Codecs.DOUBLE, r => r.Lat)
            .Field("lon", Codecs.DOUBLE, r => r.Lon)
            .Build((lat, lon) => new GeoPosition(lat, lon));
    }

    public void Dispose()
    {
        InvalidateCache();
    }
}
