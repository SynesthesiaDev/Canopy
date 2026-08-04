// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Codec.Versioned;
using SynesthesiaDev.Synx.Types;

namespace Canopy.Configuration;

public record Config(
    // ReSharper disable once InconsistentNaming
    string _schema,
    bool AutoStartOnStartup,
    bool UseLegacyWindowsApi,
    bool UpdateLockScreen,
    bool ApplyToAllMacOsSpaces,
    int RefreshPeriod,
    Wallpaper.FitMode FitMode,
    UpdaterConfig Updater,
    WeatherConfig Weather,
    List<Wallpaper> Wallpapers
)
{
    public static readonly Config DEFAULT = new Config
    (
        _schema: "https://github.com/SynesthesiaDev/Canopy/blob/main/schema.md",
        AutoStartOnStartup: true,
        UseLegacyWindowsApi: false,
        UpdateLockScreen: false,
        ApplyToAllMacOsSpaces: true,
        RefreshPeriod: 60_000,
        FitMode: Wallpaper.FitMode.Fit,
        Updater: UpdaterConfig.DEFAULT,
        Weather: WeatherConfig.DEFAULT,
        Wallpapers: Wallpaper.DEFAULT_WALLPAPERS
    );

    public static readonly StructCodec<Config> CODEC = StructCodec.For<Config>()
        .Field("_schema", Codecs.STRING, c => c._schema)
        .Field("AutoStartOnStartup", Codecs.BOOLEAN, c => c.AutoStartOnStartup)
        .Field("UseLegacyWindowsApi", Codecs.BOOLEAN, c => c.UseLegacyWindowsApi)
        .Field("UpdateLockScreen", Codecs.BOOLEAN, c => c.UpdateLockScreen)
        .Field("ApplyToAllMacOsSpaces", Codecs.BOOLEAN, c => c.ApplyToAllMacOsSpaces)
        .Field("RefreshPeriod", Codecs.INT, c => c.RefreshPeriod)
        .Field("FitMode", Codecs.Enum<Wallpaper.FitMode>(), c => c.FitMode)
        .Field("Updater", UpdaterConfig.CODEC, c => c.Updater)
        .Field("Weather", WeatherConfig.CODEC, c => c.Weather)
        .Field("Wallpapers", Wallpaper.CODEC.List(), c => c.Wallpapers)
        .Build((b, b1, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) => new Config(b, b1, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));

    public static readonly VersionedStructCodec<Config> VERSIONED_CODEC = new VersionedStructCodec<Config>
    {
        CurrentSchemaVersion = 1,
        InnerCodec = CODEC,
        SchemaMigrationRegistry = SchemaMigrationRegistry.Builder().For<ISynxElement>(builder =>
        {
            builder.Add(1, (transcoder, input, output) =>
            {
                output.Put(transcoder.EncodeString("_schema"), transcoder.EncodeString(DEFAULT._schema));
                Canopy.ConfigMigrated = true;
            });
        })
    };

}
