// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Text.Json;
using System.Web;
using Canopy.Configuration;
using Codon.Codec.Json;
using Serilog;

namespace Canopy.Providers.VisualCrossing;

public class VisualCrossingProvider : IProvider<WeatherType>
{
    private static readonly HttpClient http_client = new();
    private const string api_endpoint = "https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/";

    private static readonly NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
    private WeatherType lastCached = WeatherType.Clear;

    public WeatherType Get()
    {
        try
        {
            var geo = Canopy.GEOPOSITION_PROVIDER.Get();
            string location = $"{geo.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{geo.Lon.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
            Log.Verbose("Input location: {location}", location);
            string baseUrl = $"{api_endpoint}{Uri.EscapeDataString(location)}/today";

            if (query.Count == 0)
            {
                query["unitGroup"] = "us";
                query["elements"] = "cloudcover,conditions,humidity,icon,name,offset,precip,precipremote,preciptype,snow,snowdepth,visibility,solarradiation,solarenergy";
                query["include"] = "current";
                query["key"] = Canopy.CurrentConfig.Weather.VisualCrossingApiKey!;
                query["contentType"] = "json";
            }

            var finalUrl = $"{baseUrl}?{query}";

            var message = new HttpRequestMessage(HttpMethod.Get, finalUrl);
            var response = http_client.Send(message);
            var body = response.Content.ReadAsStringAsync().Result;

#if DEBUG
            Log.Verbose(body);
#endif
            var json = JsonDocument.Parse(body).RootElement;
            var decoded = VisualCrossingResponse.CODEC.Decode(JsonTranscoder.INSTANCE, json);

            var result = decoded.CurrentConditions;

            var icon = ParseIcon(result.Icon);

            var baseline = icon switch
            {
                VcIcon.ClearDay or VcIcon.ClearNight => WeatherType.Clear,

                VcIcon.PartlyCloudyDay or VcIcon.PartlyCloudyNight or
                    VcIcon.Cloudy or VcIcon.Fog or VcIcon.Wind => WeatherType.Cloudy,

                VcIcon.Rain or VcIcon.ShowersDay or VcIcon.ShowersNight or
                    VcIcon.Snow or VcIcon.SnowShowersDay or VcIcon.SnowShowersNight or
                    VcIcon.Sleet or VcIcon.Hail => WeatherType.Rainy,

                VcIcon.ThunderRain or VcIcon.ThunderShowersDay or VcIcon.ThunderShowersNight
                    => WeatherType.Stormy,

                _ => WeatherType.Clear
            };

            bool hasThunder = result.Conditions.Contains("Thunder", StringComparison.OrdinalIgnoreCase);
            bool hasMeasurablePrecip = result.Precip > 0.01;
            var weatherType = baseline;

            if (hasThunder)
            {
                weatherType = WeatherType.Stormy;
            }
            else if (hasMeasurablePrecip && weatherType != WeatherType.Stormy)
            {
                weatherType = WeatherType.Rainy;
            }

            var elevation = ClearSkyModel.SolarElevationDegrees(geo.Lat, geo.Lon, DateTimeOffset.Now);
            bool isDaytime = elevation > 15.0; // was 7.0, Haurwitz model is unreliable below this

            if (isDaytime && !hasThunder && !hasMeasurablePrecip)
            {
                double clearSkyRadiation = ClearSkyModel.EstimateClearSkyRadiation(elevation);
                double ratio = clearSkyRadiation > 1.0
                    ? Math.Clamp(result.SolarRadiation / clearSkyRadiation, 0.0, 1.5)
                    : 1.0;

                weatherType = (result.CloudCover < 40, ratio) switch
                {
                    (true, _) => WeatherType.Clear,
                    (false, >= 0.60) => WeatherType.Clear,
                    _ => WeatherType.Cloudy
                };

#if DEBUG
                Log.Verbose("SolarRatio: {r:0.00} (measured {m}, clearSky {c:0.00}), elevation {e:0.00}",
                    ratio, result.SolarRadiation, clearSkyRadiation, elevation);
#endif
            }
            else if (!hasThunder && !hasMeasurablePrecip && (weatherType == WeatherType.Cloudy || (weatherType == WeatherType.Clear && icon == VcIcon.Unknown)))
            {
                // Night/twilight fallback cause the sun do not radiate when it night
                weatherType = result.CloudCover > 35 ? WeatherType.Cloudy : WeatherType.Clear;
            }

#if DEBUG
            Log.Verbose(" ");
            Log.Verbose("Weather: {w}", weatherType);
            Log.Verbose("Icon: {i}, Conditions: {c}, Precip: {p}, CloudCover: {cc}", icon, result.Conditions, result.Precip, result.CloudCover);
#endif

            lastCached = weatherType;
            return weatherType;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while fetching weather with VisualCrossingProvider, providing last cached weather ({cached})", lastCached);
            return lastCached;
        }
    }

    public void Dispose()
    {
    }

    public enum VcIcon
    {
        ClearDay, ClearNight,
        PartlyCloudyDay, PartlyCloudyNight,
        Cloudy, Fog, Wind,
        Rain, ShowersDay, ShowersNight,
        Snow, SnowShowersDay, SnowShowersNight, Sleet, Hail,
        ThunderRain, ThunderShowersDay, ThunderShowersNight,
        Unknown
    }

    public static VcIcon ParseIcon(string? icon) =>
        icon switch
        {
            "clear-day" => VcIcon.ClearDay,
            "clear-night" => VcIcon.ClearNight,
            "partly-cloudy-day" => VcIcon.PartlyCloudyDay,
            "partly-cloudy-night" => VcIcon.PartlyCloudyNight,
            "cloudy" => VcIcon.Cloudy,
            "fog" => VcIcon.Fog,
            "wind" => VcIcon.Wind,
            "rain" => VcIcon.Rain,
            "showers-day" => VcIcon.ShowersDay,
            "showers-night" => VcIcon.ShowersNight,
            "snow" => VcIcon.Snow,
            "snow-showers-day" => VcIcon.SnowShowersDay,
            "snow-showers-night" => VcIcon.SnowShowersNight,
            "sleet" => VcIcon.Sleet,
            "hail" => VcIcon.Hail,
            "thunder-rain" => VcIcon.ThunderRain,
            "thunder-showers-day" => VcIcon.ThunderShowersDay,
            "thunder-showers-night" => VcIcon.ThunderShowersNight,
            _ => VcIcon.Unknown
        };
}
