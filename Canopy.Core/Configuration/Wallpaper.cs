// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Configuration;

public record Wallpaper(
    string Path,
    List<TimeOfDay> Time,
    List<WeatherType> Weather,
    List<SeasonType> Season,
    List<Holiday> Holiday
)
{

    public static readonly List<Wallpaper> DEFAULT_WALLPAPERS =
    [
        new Wallpaper
        (
            Path: "default/cloudy-quasar.png",
            Time: [TimeOfDay.Night, TimeOfDay.DeepNight],
            Weather: [WeatherType.Cloudy],
            Season: [],
            Holiday: []
        ),

        new Wallpaper
        (
            Path: "default/galaxy.png",
            Time: [TimeOfDay.Night, TimeOfDay.DeepNight],
            Weather: [],
            Season: [],
            Holiday: []
        ),

        new Wallpaper
        (
            Path: "default/beach.jpg",
            Time: [TimeOfDay.Afternoon],
            Weather: [WeatherType.Clear],
            Season: [SeasonType.Summer],
            Holiday: []
        ),

        new Wallpaper
        (
            Path: "default/bluehour.jpg",
            Time: [TimeOfDay.Sunset],
            Weather: [WeatherType.Clear],
            Season: [],
            Holiday: []
        ),

        new Wallpaper
        (
            Path: "default/call-it-a-day.jpg",
            Time: [TimeOfDay.Sunset],
            Weather: [WeatherType.Clear, WeatherType.Cloudy],
            Season: [SeasonType.Winter],
            Holiday: []
        ),

        new Wallpaper
        (
            Path: "default/eclipse.jpg",
            Time: [TimeOfDay.Sunset],
            Weather: [WeatherType.Cloudy, WeatherType.Clear],
            Season: [],
            Holiday: []
        ),

        new Wallpaper
        (
            Path: "default/flower-field.jpg",
            Time: [TimeOfDay.Morning, TimeOfDay.Afternoon],
            Weather: [WeatherType.Clear],
            Season: [SeasonType.Spring],
            Holiday: []
        ),

        new Wallpaper
        (
            Path: "default/i-touch-this.jpg",
            Time: [TimeOfDay.Morning],
            Weather: [WeatherType.Clear],
            Season: [],
            Holiday: []
        ),
        new Wallpaper
        (
            Path: "default/pink-clouds.jpg",
            Time: [TimeOfDay.Sunset, TimeOfDay.Sunrise],
            Weather: [WeatherType.Clear, WeatherType.Cloudy],
            Season: [],
            Holiday: []
        ),
        new Wallpaper
        (
            Path: "default/snowflakes.jpg",
            Time: [TimeOfDay.Night, TimeOfDay.DeepNight],
            Weather: [],
            Season: [SeasonType.Winter],
            Holiday: []
        ),
        new Wallpaper
        (
            Path: "default/swirly-painting.jpg",
            Time: [TimeOfDay.Sunset, TimeOfDay.Sunrise],
            Weather: [WeatherType.Clear, WeatherType.Cloudy],
            Season: [],
            Holiday: []
        ),
        new Wallpaper
        (
            Path: "default/flowering-rain.png",
            Time: [TimeOfDay.Morning, TimeOfDay.Afternoon],
            Weather: [WeatherType.Rainy, WeatherType.Stormy],
            Season: [],
            Holiday: []
        ),
    ];

    public static readonly StructCodec<Wallpaper> CODEC = StructCodec.For<Wallpaper>()
        .Field("Path", Codecs.STRING, w => w.Path)
        .Field("Time", Codecs.Enum<TimeOfDay>().List().Default([]), w => w.Time)
        .Field("Weather", Codecs.Enum<WeatherType>().List().Default([]), w => w.Weather)
        .Field("Season", Codecs.Enum<SeasonType>().List().Default([]), w => w.Season)
        .Field("Holiday", Codecs.Enum<Holiday>().List().Default([]), w => w.Holiday)
        .Build((s, days, arg3, arg4, arg5) => new Wallpaper(s, days, arg3, arg4, arg5));

    public enum FitMode
    {
        Fill,
        Fit,
        Stretch,
        Tile,
        Center,
        Span
    }
}
