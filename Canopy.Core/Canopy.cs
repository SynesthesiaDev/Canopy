// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using Serilog;
using SynesthesiaDev.Synx;
using SynesthesiaDev.Synx.Codon;

namespace Canopy;

public class Canopy(ICanopyPlatform platform)
{
    public readonly ICanopyPlatform Platform = platform;

    public static Config CurrentConfig = null!;

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

    public void Initialize()
    {
        Log.Verbose("Initializing Canopy..");
        loadConfig();
    }

    private void loadConfig()
    {
        Log.Verbose("Loading config..");
        if (!Directory.Exists(CANOPY_FOLDER_PATH))
        {
            Directory.CreateDirectory(CANOPY_FOLDER_PATH);
        }

        if (!File.Exists(CONFIG_FILE_PATH))
        {
            Log.Verbose("Config file doesn't exist.. creating new one");

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
    }
}
