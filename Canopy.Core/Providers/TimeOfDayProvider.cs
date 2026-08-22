// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using Innovative.Geometry;
using Innovative.SolarCalculator;
using Serilog;

namespace Canopy.Providers;

public class TimeOfDayProvider : IProvider<TimeOfDay>
{
    private const int sunrise_offset_minutes = 60;
    private const int sunset_offset_minutes = -100;
    private const int event_duration_minutes = 60;

    public TimeOfDay Get()
    {
        var now = DateTimeOffset.Now;
        var geo = Canopy.GEOPOSITION_PROVIDER.Get();
        var solarTimes = new SolarTimes(now, new Angle(geo.Lat), new Angle(geo.Lon));

        var baseSunrise = solarTimes.Sunrise;
        var baseSunset = solarTimes.Sunset;

        var sunriseStart = baseSunrise.AddMinutes(sunrise_offset_minutes);
        var sunsetStart = baseSunset.AddMinutes(sunset_offset_minutes);

#if DEBUG
        Log.Verbose(" ");
        Log.Verbose("Solar Noon: {sun}", solarTimes.SolarNoon);
        Log.Verbose("Sunrise: {sun}", sunriseStart);
        Log.Verbose("Sunset: {sun}", sunsetStart);
#endif

        var eventDuration = TimeSpan.FromMinutes(event_duration_minutes);
        var sunriseEnd = sunriseStart + eventDuration;
        var sunsetEnd = sunsetStart + eventDuration;

        if (now >= sunriseStart && now < sunriseEnd)
            return TimeOfDay.Sunrise;

        if (now >= sunsetStart && now < sunsetEnd)
            return TimeOfDay.Sunset;

        if (now >= sunriseEnd && now < sunsetStart)
        {
            DateTimeOffset midday;

            if (Canopy.CurrentConfig.General.UseSolarNoonAsMidday)
                midday = baseSunrise + (baseSunset - baseSunrise) / 2;
            else
                midday = DateTime.Today.AddHours(12);

            return now < midday ? TimeOfDay.Morning : TimeOfDay.Afternoon;
        }

        if ((now >= sunsetEnd && now.Hour < 22) || (now.Hour >= 4 && now < sunriseStart))
            return TimeOfDay.Night;

        return TimeOfDay.DeepNight;
    }

    public void Dispose()
    {

    }
}
