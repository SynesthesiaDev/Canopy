// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Graphics;
using Canopy.Storage;

namespace Canopy.Utils;

public static class UserLand
{
    public static void EntryPoint(ICanopyPlatform platform)
    {
        var storage = new DirectoryAssetStorage(@"C:\Users\Synesthesia\RiderProjects\Canopy\resources");
        var wallpaper = new Wallpaper("furina.png", storage);

        platform.PushWallpaper(wallpaper);
    }


}
