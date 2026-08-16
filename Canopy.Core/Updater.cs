// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Serilog;
using Velopack;
using Velopack.Exceptions;
using Velopack.Sources;

namespace Canopy;

public class Updater
{
    public static async Task CheckForUpdates(Canopy canopy)
    {
        try
        {
            var config = Canopy.CurrentConfig.Updater;
            if (!config.AutoUpdate) return;

            Log.Information("Checking for updates..");
            var manager = new UpdateManager(new GithubSource(Canopy.CurrentConfig.Updater.Source, null, false));
            var newVersion = await manager.CheckForUpdatesAsync();

            if (newVersion == null)
            {
                Log.Information("No new updates available");
                return;
            }
            Log.Information("New update available, downloading..");
            canopy.Platform.ShowNotification("Canopy", $"Downloading new update..");

            await manager.DownloadUpdatesAsync(newVersion);
            manager.ApplyUpdatesAndRestart(newVersion);
        }
        catch (NotInstalledException)
        {
            Log.Warning("Skipping update check, app running in dev mode");
            throw;
        }
    }
}
