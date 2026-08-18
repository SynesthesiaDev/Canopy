// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using OpenMeteoApi;
using Serilog;

namespace Canopy.Providers;

public class WeatherProvider : IProvider<WeatherType>
{
    private static readonly OpenMeteoClient open_meteo_client = new OpenMeteoClient();

    public WeatherType Get()
    {
        var geo = Canopy.GEOPOSITION_PROVIDER.Get();
        var weather = open_meteo_client.GetCurrentWeather(geo.Lat, geo.Lon).GetAwaiter().GetResult();

        var condition = ToWeatherCondition(weather.WeatherCode!.Value);

        var weatherType = condition switch
        {
            WeatherCondition.ClearSky or
                WeatherCondition.MainlyClear or
                WeatherCondition.Unknown => WeatherType.Clear,

            WeatherCondition.PartlyCloudy or
                WeatherCondition.Overcast or
                WeatherCondition.Fog => WeatherType.Cloudy,

            WeatherCondition.DrizzleLight or
                WeatherCondition.DrizzleModerate or
                WeatherCondition.DrizzleDense or
                WeatherCondition.RainSlight or
                WeatherCondition.RainModerate or
                WeatherCondition.RainHeavy or
                WeatherCondition.SnowSlight or
                WeatherCondition.SnowModerate or
                WeatherCondition.SnowHeavy => WeatherType.Rainy,

            WeatherCondition.Thunderstorm => WeatherType.Stormy,

            _ => throw new ArgumentOutOfRangeException()
        };

#if DEBUG

        Log.Verbose(" ");
        Log.Verbose("Weather: {w}", weatherType);
        Log.Verbose("Underlying: {w}", condition);
#endif
        return weatherType;
    }

    public enum WeatherCondition
    {
        ClearSky,
        MainlyClear,
        PartlyCloudy,
        Overcast,
        Fog,
        DrizzleLight,
        DrizzleModerate,
        DrizzleDense,
        RainSlight,
        RainModerate,
        RainHeavy,
        SnowSlight,
        SnowModerate,
        SnowHeavy,
        Thunderstorm,
        Unknown
    }

    public static WeatherCondition ToWeatherCondition(int code) =>
        code switch
        {
            0 => WeatherCondition.ClearSky,
            1 => WeatherCondition.MainlyClear,
            2 => WeatherCondition.PartlyCloudy,
            3 => WeatherCondition.Overcast,
            45 or 48 => WeatherCondition.Fog,
            51 => WeatherCondition.DrizzleLight,
            53 => WeatherCondition.DrizzleModerate,
            55 => WeatherCondition.DrizzleDense,
            61 => WeatherCondition.RainSlight,
            63 => WeatherCondition.RainModerate,
            65 => WeatherCondition.RainHeavy,
            71 => WeatherCondition.SnowSlight,
            73 => WeatherCondition.SnowModerate,
            75 => WeatherCondition.SnowHeavy,
            95 or 96 or 99 => WeatherCondition.Thunderstorm,
            _ => WeatherCondition.Unknown
        };
}
