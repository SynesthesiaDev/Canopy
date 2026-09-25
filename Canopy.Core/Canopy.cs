// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using Canopy.Providers;
using Canopy.Providers.VisualCrossing;
using Canopy.Server;
using Canopy.Server.Messages;
using Serilog;
using Synesthesia.Utils.Extensions;
using SynesthesiaDev.ConfigLibrary;
using SynesthesiaDev.ConfigLibrary.Location;
using SynesthesiaDev.Synx.Codon;
using SynesthesiaDev.Synx.Types;

namespace Canopy;

public class Canopy(ICanopyPlatform platform)
{
    public readonly ICanopyPlatform Platform = platform;

    public static Config CurrentConfig => CONFIG.Current;

    public static readonly GeopositionProvider GEOPOSITION_PROVIDER = new GeopositionProvider();
    public static readonly TimeOfDayProvider TIME_OF_DAY_PROVIDER = new TimeOfDayProvider();
    public static readonly SeasonProvider SEASON_PROVIDER = new SeasonProvider();
    public static readonly HolidayProvider HOLIDAY_PROVIDER = new HolidayProvider();

    public static readonly ConfigLib<Config, ISynxElement> CONFIG = ConfigLib.For<Config, ISynxElement>()
        .Encoding(Config.VERSIONED_CODEC, SynxTranscoder.INSTANCE)
        .Default(Config.DEFAULT)
#if DEBUG
        .Location(ConfigLocation.UserFolder(".canopy-development", "config.synx"))
#else
        .Location(ConfigLocation.UserFolder(".canopy", "config.synx"))
#endif
        .DirectoryCreateHook(location =>
        {
            Utils.CopyEmbeddedFolder(typeof(Canopy).Assembly, "Canopy.Core.Resources", location.FolderPath);
        })
        .Build();

    public static IProvider<WeatherType>? WeatherProvider;

    private CanopyState? lastState;

    public static bool ConfigMigrated = false;

    public CanopyWebsocketServer? WebsocketServer;

    public void Initialize()
    {
        Log.Information("Initializing Canopy..");

        LoadRefreshable();

        Task.Run(async () => await Updater.CheckForUpdates(this).ConfigureAwait(false));

        Task.Run(async () =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(CurrentConfig.General.RefreshPeriod));
            while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
            {
                try
                {
                    Refresh();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to refresh");
                }

                try
                {
                    await Updater.CheckForUpdates(this).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to fetch update");
                }
            }
        });

        Platform.InitializeTray(this);

        Platform.BlockThread();
    }

    public void LoadRefreshable()
    {
        loadConfig();

        WeatherProvider?.Dispose();
        WeatherProvider = CurrentConfig.Weather.VisualCrossingApiKey != null ? new VisualCrossingProvider() : new OpenMeteoProvider();

        Log.Verbose("Using {provider} as weather  provider", WeatherProvider.GetType().Name);
        lastState = null;
        GEOPOSITION_PROVIDER.InvalidateCache();

        WebsocketServer?.Dispose();
        if (CurrentConfig.Websocket.Enabled)
        {
            WebsocketServer = new CanopyWebsocketServer();
            WebsocketServer.Initialize();
        }

        Refresh();
    }

    private void loadConfig()
    {
        try
        {
            CONFIG.Load();
        }
        catch (Exception e)
        {
            Platform.ShowErrorPopup("Failed to load Canopy config:", e.ToString());
            Console.WriteLine(e);
            throw;
        }

        Log.Information("Loaded {wallpapers} wallpapers", CurrentConfig.Wallpapers.Count);
        validateConfig();
    }

    public void Refresh()
    {
#if DEBUG
        Log.Debug("Refreshing state.. ({time})", DateTime.Now);
#endif
        var time = TIME_OF_DAY_PROVIDER.Get();
        var weather = WeatherProvider?.Get() ?? WeatherType.Clear;
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
            Platform.ShowErrorPopup("(Canopy) Error while picking wallpaper", "No wallpaper configuration found for current state ({state})");
            Environment.Exit(-1);
            return;
        }

        Log.Information("Picked new wallpaper: {pick} with state {state}!", next.Path, state);
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

        var topMatches = new List<Wallpaper>();
        int topScore = -1;

        foreach (var w in CurrentConfig.Wallpapers)
        {
            if (!w.Holiday.IsMissing ||
                !containsOrEmpty(w.Season, season) ||
                !containsOrEmpty(w.Time, time) ||
                !containsOrEmpty(w.Weather, weather)) continue;

            int score = scoreWallpaper(w, time, weather, season);
            if (score > topScore)
            {
                topScore = score;
                topMatches.Clear();
                topMatches.Add(w);
            }
            else if (score == topScore)
            {
                topMatches.Add(w);
            }
        }

        return topMatches.IsEmpty() ? null : topMatches.Random();
    }

    private static ICanopyPlatform.Theme getThemeForTimeOfDay(TimeOfDay time) =>
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
            Log.Error(error);
            Platform.ShowErrorPopup("(Canopy) Error while parsing config", error);

            Environment.Exit(-1);
        }
    }

    public static string ResolveWallpaperPath(string rawPath)
    {
        return Path.IsPathRooted(rawPath) ? rawPath : Path.GetFullPath(Path.Combine(CONFIG.Location.FolderPath, rawPath));
    }
}
