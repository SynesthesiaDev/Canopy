// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Optionals;
using Synesthesia.Utils.Extensions;

namespace Canopy.Configuration;

public record Wallpaper(
    string Path,
    List<TimeOfDay> Time,
    List<WeatherType> Weather,
    List<SeasonType> Season,
    Optional<Holiday> Holiday,
    string? Accent = null
)
{

    public static readonly List<Wallpaper> DEFAULT_WALLPAPERS =
    [
        new Wallpaper
        (
            Path: "./default/cloudy-quasar.png",
            Time: [TimeOfDay.Night, TimeOfDay.DeepNight],
            Weather: [WeatherType.Cloudy],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#c5d9d7"
        ),

        new Wallpaper
        (
            Path: "./default/beach.jpg",
            Time: [TimeOfDay.Afternoon],
            Weather: [WeatherType.Clear],
            Season: [SeasonType.Summer],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#207ad9"
        ),

        new Wallpaper
        (
            Path: "./default/halloween.jpg",
            Time: [],
            Weather: [],
            Season: [],
            Holiday: Optional.Of(Configuration.Holiday.Halloween),
            Accent: "#f56b3d"
        ),

        new Wallpaper
        (
            Path: "./default/eclipse.jpg",
            Time: [TimeOfDay.Sunset],
            Weather: [WeatherType.Cloudy, WeatherType.Clear],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#f4545e"
        ),

        new Wallpaper
        (
            Path: "./default/flower-field.jpg",
            Time: [TimeOfDay.Morning, TimeOfDay.Afternoon],
            Weather: [WeatherType.Clear],
            Season: [SeasonType.Spring],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#9ca15e"
        ),

        new Wallpaper
        (
            Path: "./default/i-touch-this.jpg",
            Time: [TimeOfDay.Morning],
            Weather: [WeatherType.Clear],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#89b238"
        ),
        new Wallpaper
        (
            Path: "./default/pink-clouds.jpg",
            Time: [TimeOfDay.Sunset, TimeOfDay.Sunrise],
            Weather: [WeatherType.Clear, WeatherType.Cloudy],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#e69c94"
        ),
        new Wallpaper
        (
            Path: "./default/snowflakes.jpg",
            Time: [TimeOfDay.Night, TimeOfDay.DeepNight],
            Weather: [WeatherType.Rainy, WeatherType.Clear],
            Season: [SeasonType.Winter],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#c2e6ff"
        ),
        new Wallpaper
        (
            Path: "./default/swirly-painting.jpg",
            Time: [TimeOfDay.Sunset, TimeOfDay.Sunrise],
            Weather: [WeatherType.Clear, WeatherType.Cloudy],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#df7488"
        ),
        new Wallpaper
        (
            Path: "./default/flowering-rain.png",
            Time: [TimeOfDay.Morning, TimeOfDay.Afternoon],
            Weather: [WeatherType.Rainy, WeatherType.Stormy],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#598fb1"
        ),

        // Fallback Wallpapers per each time

        new Wallpaper
        (
            Path: "./default/fallback/Sunrise.jpg",
            Time: [TimeOfDay.Sunrise],
            Weather: [],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#1a4a4a"
        ),
        new Wallpaper
        (
            Path: "./default/fallback/Morning.jpg",
            Time: [TimeOfDay.Morning],
            Weather: [],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#1b4a40"
        ),
        new Wallpaper
        (
            Path: "./default/fallback/Afternoon.jpg",
            Time: [TimeOfDay.Afternoon],
            Weather: [],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#3d76a1"
        ),
        new Wallpaper
        (
            Path: "./default/fallback/Sunset.jpg",
            Time: [TimeOfDay.Sunset],
            Weather: [],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#e56f32"
        ),
        new Wallpaper
        (
            Path: "./default/fallback/Night.jpg",
            Time: [TimeOfDay.Night],
            Weather: [],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#314d3f"
        ),
        new Wallpaper
        (
            Path: "./default/fallback/DeepNight.jpg",
            Time: [TimeOfDay.DeepNight],
            Weather: [],
            Season: [],
            Holiday: Optional.Empty<Holiday>(),
            Accent: "#1b2836"
        ),
    ];

    public static readonly StructCodec<Wallpaper> CODEC = StructCodec.For<Wallpaper>()
        .Field("Path", Codecs.STRING, w => w.Path)
        .Field("Time", Codecs.Enum<TimeOfDay>().List().Default([]), w => w.Time)
        .Field("Weather", Codecs.Enum<WeatherType>().List().Default([]), w => w.Weather)
        .Field("Season", Codecs.Enum<SeasonType>().List().Default([]), w => w.Season)
        .Field("Holiday", Codecs.Enum<Holiday>().Optional(), w => w.Holiday)
        .Field("Accent", Codecs.STRING.Optional(), w => w.Accent.ToOptional())
        .Build((s, days, arg3, arg4, arg5, acc) => new Wallpaper(s, days, arg3, arg4, arg5, acc.Value));

    public enum FitMode
    {
        Fill,
        Fit,
        Stretch,
        Tile,
        Center,
        Span
    }

    public override string ToString() => $"Wallpaper(Path={Path}, Time={Time.ToListString()}, Weather={Weather.ToListString()}, Season={Season.ToListString()}, Holiday={Holiday}, Accent={Accent})";
}
