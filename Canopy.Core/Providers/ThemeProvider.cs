// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;

namespace Canopy.Providers;

public class ThemeProvider : IProvider<ICanopyPlatform.Theme>
{
    private static readonly WeatherType[] dark_weather = [WeatherType.Cloudy, WeatherType.Rainy, WeatherType.Stormy];

    public ICanopyPlatform.Theme Get()
    {
        var timeOfDay = Canopy.TIME_OF_DAY_PROVIDER.Get();
        var weather = Canopy.WeatherProvider?.Get();
        var theme = GetThemeForTimeOfDay(timeOfDay);
        if (weather != null && dark_weather.Contains(weather.Value))
            theme = ICanopyPlatform.Theme.Dark;

        return theme;

    }

    public void Dispose()
    {
    }

    public static ICanopyPlatform.Theme GetThemeForTimeOfDay(TimeOfDay time) =>
        time switch
        {
            TimeOfDay.Sunset or TimeOfDay.Morning or TimeOfDay.Afternoon => ICanopyPlatform.Theme.Light,
            TimeOfDay.Sunrise or TimeOfDay.Night or TimeOfDay.DeepNight => ICanopyPlatform.Theme.Dark,
            _ => ICanopyPlatform.Theme.Light
        };
}
