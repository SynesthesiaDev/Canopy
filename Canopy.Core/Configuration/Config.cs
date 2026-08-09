// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Codon.Codec.Versioned;
using SynesthesiaDev.Synx.Types;

namespace Canopy.Configuration;

public record Config(
    // ReSharper disable once InconsistentNaming
    string _schema,
    GeneralConfig General,
    SystemConfig System,
    UpdaterConfig Updater,
    WeatherConfig Weather,
    WebsocketConfig Websocket,
    List<Wallpaper> Wallpapers
)
{
    public static readonly Config DEFAULT = new Config
    (
        _schema: "https://github.com/SynesthesiaDev/Canopy/blob/main/schema.md",
        General: GeneralConfig.DEFAULT,
        System: SystemConfig.DEFAULT,
        Updater: UpdaterConfig.DEFAULT,
        Weather: WeatherConfig.DEFAULT,
        Websocket: WebsocketConfig.DEFAULT,
        Wallpapers: Wallpaper.DEFAULT_WALLPAPERS
    );

    public static readonly StructCodec<Config> CODEC = StructCodec.For<Config>()
        .Field("_schema", Codecs.STRING, c => c._schema)
        .Field("General", GeneralConfig.CODEC, c => c.General)
        .Field("System", SystemConfig.CODEC, c => c.System)
        .Field("Updater", UpdaterConfig.CODEC, c => c.Updater)
        .Field("Weather", WeatherConfig.CODEC, c => c.Weather)
        .Field("Websocket", WebsocketConfig.CODEC, c => c.Websocket)
        .Field("Wallpapers", Wallpaper.CODEC.List(), c => c.Wallpapers)
        .Build((s, config, arg3, arg4, arg5, arg6, arg7) => new Config(s, config, arg3, arg4, arg5, arg6, arg7));

    public static readonly VersionedStructCodec<Config> VERSIONED_CODEC = new VersionedStructCodec<Config>
    {
        CurrentSchemaVersion = 2,
        InnerCodec = CODEC,
        SchemaMigrationRegistry = SchemaMigrationRegistry.Builder().For<ISynxElement>(builder =>
        {
            builder.Add(1, (transcoder, _, output) =>
            {
                output.Put(transcoder.EncodeString("_schema"), transcoder.EncodeString(DEFAULT._schema));
                Canopy.ConfigMigrated = true;
            });
            builder.Add(2, (transcoder, _, output) =>
            {
                output.Put(transcoder.EncodeString("ChangeSystemThemesDependingOnTime"), transcoder.EncodeBool(DEFAULT.System.ChangeSystemThemesDependingOnTime));
                Canopy.ConfigMigrated = true;
            });
        })
    };

}
