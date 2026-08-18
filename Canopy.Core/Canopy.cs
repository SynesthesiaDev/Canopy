// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using Canopy.Providers;
using Canopy.Server;
using Canopy.Server.Messages;
using Serilog;
using Synesthesia.Utils.Extensions;
using SynesthesiaDev.Synx;
using SynesthesiaDev.Synx.Codon;

namespace Canopy;

public class Canopy(ICanopyPlatform platform)
{
    public readonly ICanopyPlatform Platform = platform;

    public static Config CurrentConfig = null!;

    public static readonly GeopositionProvider GEOPOSITION_PROVIDER = new GeopositionProvider();
    public static readonly WeatherProvider WEATHER_PROVIDER = new WeatherProvider();
    public static readonly TimeOfDayProvider TIME_OF_DAY_PROVIDER = new TimeOfDayProvider();
    public static readonly SeasonProvider SEASON_PROVIDER = new SeasonProvider();
    public static readonly HolidayProvider HOLIDAY_PROVIDER = new HolidayProvider();

    private CanopyState? lastState;

#if DEBUG
    public static readonly string CANOPY_FOLDER_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".canopy-development"
    );
#else
    public static readonly string CANOPY_FOLDER_PATH = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".canopy"
    );
#endif

    public static bool ConfigMigrated = false;
    public static readonly string CONFIG_FILE_PATH = Path.Combine(CANOPY_FOLDER_PATH, "config.synx");

    public CanopyWebsocketServer? WebsocketServer;

    public void Initialize()
    {
        Log.Information("Initializing Canopy..");

        LoadRefreshable();

        Task.Run(async () => await Updater.CheckForUpdates(this));

        Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(CurrentConfig.General.RefreshPeriod));
            while (await timer.WaitForNextTickAsync())
            {
                Refresh();
                await Updater.CheckForUpdates(this);
            }
        });

        Platform.InitializeTray(this);

        Platform.BlockThread();
    }

    public void LoadRefreshable()
    {
        loadConfig();
        lastState = null;
        GEOPOSITION_PROVIDER.InvalidateCache();

        WebsocketServer?.Stop();
        if (CurrentConfig.Websocket.Enabled)
        {
            WebsocketServer = new CanopyWebsocketServer();
            WebsocketServer.Initialize();
        }

        Refresh();
    }

    private void loadConfig()
    {
        if (!Directory.Exists(CANOPY_FOLDER_PATH))
        {
            Log.Information("Folder doesn't exist, creating and copying default contents");
            Directory.CreateDirectory(CANOPY_FOLDER_PATH);
            Utils.CopyEmbeddedFolder(GetType().Assembly, "Canopy.Core.Resources", CANOPY_FOLDER_PATH);
        }

        if (!File.Exists(CONFIG_FILE_PATH))
        {
            Log.Information("Config file doesn't exist.. creating new one");

            File.Create(CONFIG_FILE_PATH).Close();
            var encodedText = Config.VERSIONED_CODEC.Encode(SynxTranscoder.INSTANCE, Config.DEFAULT).Object().EncodeToString();
            File.WriteAllText(CONFIG_FILE_PATH, encodedText);

            CurrentConfig = Config.DEFAULT;
        }
        else
        {
            var decoded = Config.VERSIONED_CODEC.Decode(SynxTranscoder.INSTANCE, File.ReadAllText(CONFIG_FILE_PATH).ToSynxObject());
            CurrentConfig = decoded;
            if (ConfigMigrated)
            {
                var encoded = Config.VERSIONED_CODEC.Encode(SynxTranscoder.INSTANCE, CurrentConfig).Object().EncodeToString();
                File.WriteAllText(CONFIG_FILE_PATH, encoded);
                Log.Information("A migration was applied to your config and it was re-written");
            }
        }

        Log.Information("Loaded {wallpapers} wallpapers", CurrentConfig.Wallpapers.Count);
        validateConfig();
    }

    public void Refresh()
    {
#if DEBUG
        Log.Debug("Refreshing state..");
#endif
        var time = TIME_OF_DAY_PROVIDER.Get();
        var weather = WEATHER_PROVIDER.Get();
        var season = SEASON_PROVIDER.Get();
        var holiday = HOLIDAY_PROVIDER.Get();

        var state = new CanopyState(time, weather, season, holiday);

        if (CurrentConfig.System.ChangeSystemThemesDependingOnTime && time != lastState?.Time)
        {
            var theme = getThemeForTimeOfDay(time);
            Log.Information("Changing system theme to {theme}", theme);
            Platform.SetTheme(theme);
        }

        if (lastState == state)
        {
#if DEBUG
            Log.Debug("State is same, no updates");
#endif
            return;
        }

        lastState = state;

        var next = PickNextWallpaper(time, weather, season, holiday);
        if (next == null)
        {
            Platform.ShowNotification("Canopy", $"No wallpaper configuration found for current state ({state})", ICanopyPlatform.NotificationLevel.Warning);
            Log.Error("No wallpapers found for current state ({state})", state);
            return;
        }

        Log.Information("Picked new wallpaper: {pick}!", next.Path);
#if DEBUG
        Log.Verbose("Setting wallpaper via {type}", Platform.GetType().Name);
#endif
        Platform.SetDesktop(ResolveWallpaperPath(next.Path));

        var message = new NewWallpaperMessage(DateTimeOffset.Now.ToUnixTimeMilliseconds(), next);
        WebsocketServer?.Send(message);

        GC.Collect(2, GCCollectionMode.Optimized, blocking: false, compacting: true);
    }

    private record CanopyState(TimeOfDay Time, WeatherType Weather, SeasonType Season, Holiday? Holiday);

    public Wallpaper? PickNextWallpaper(TimeOfDay time, WeatherType weather, SeasonType season, Holiday? holiday)
    {
        if (holiday != null)
        {
            var holidayWallpapers = CurrentConfig.Wallpapers.Filter(w => w.Holiday.Value == holiday);
            if (holidayWallpapers.IsNotEmpty())
            {
                var wallpaper = holidayWallpapers.Random();
                return wallpaper;
            }
        }

        var eligible = CurrentConfig.Wallpapers.Filter(w =>
            containsOrEmpty(w.Season, season) &&
            containsOrEmpty(w.Time, time) &&
            containsOrEmpty(w.Weather, weather) &&
            w.Holiday.IsMissing
        );

        if (eligible.IsEmpty()) return null;

        var scored = eligible
            .Select(w => (Wallpaper: w, Score: scoreWallpaper(w, time, weather, season)))
            .ToList();

        var topScore = scored.Max(s => s.Score);

        var topMatches = scored.Where(s => s.Score == topScore).Select(s => s.Wallpaper).ToList();

        return topMatches.IsEmpty() ? null : topMatches.Random();
    }

    private ICanopyPlatform.Theme getThemeForTimeOfDay(TimeOfDay time) =>
        time switch
        {
            TimeOfDay.Sunset or TimeOfDay.Morning or TimeOfDay.Afternoon => ICanopyPlatform.Theme.Light,
            TimeOfDay.Sunrise or TimeOfDay.Night or TimeOfDay.DeepNight => ICanopyPlatform.Theme.Dark,
            _ => ICanopyPlatform.Theme.Light
        };

    private static int scoreWallpaper(Wallpaper w, TimeOfDay time, WeatherType weather, SeasonType season)
    {
        int score = 0;
        if (w.Time.Contains(time)) score++;
        if (w.Weather.Contains(weather))
        {
            score++;
            if (weather is WeatherType.Rainy or WeatherType.Stormy)
                score++;
        }

        if (w.Season.Contains(season)) score++;
        return score;
    }

    private static bool containsOrEmpty<T>(List<T> list, T item)
    {
        if (list.IsEmpty()) return true;
        return list.Contains(item);
    }

    private void validateConfig()
    {
        string? error = null;
        foreach (var wallpaper in CurrentConfig.Wallpapers)
        {
            if (!File.Exists(ResolveWallpaperPath(wallpaper.Path)))
            {
                error = $"Wallpaper with path {ResolveWallpaperPath(wallpaper.Path)} doesn't exist";
            }

            if (wallpaper.Accent != null)
            {
                if (wallpaper.Accent.Length != 7 || !wallpaper.Accent.StartsWith('#'))
                    error = $"Invalid accent format. Must be hex color like #ff00ff (Wallpaper {wallpaper.Path})";
            }
        }

        if (error != null)
        {
            Platform.ShowNotification("Canopy", error, ICanopyPlatform.NotificationLevel.Error);
            Log.Error(error);
            Environment.Exit(0);
        }
    }

    public static string ResolveWallpaperPath(string rawPath)
    {
        if (Path.IsPathRooted(rawPath))
            return rawPath;

        return Path.GetFullPath(Path.Combine(CANOPY_FOLDER_PATH, rawPath));
    }
}
