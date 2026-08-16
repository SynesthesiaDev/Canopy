// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using Vanara.PInvoke;

namespace Canopy.Windows.Extensions;

public static class FitModeExtensions
{
    public static Shell32.DESKTOP_WALLPAPER_POSITION ToShellPos(this Wallpaper.FitMode fitMode)
    {
        return fitMode switch
        {
            Wallpaper.FitMode.Fill => Shell32.DESKTOP_WALLPAPER_POSITION.DWPOS_FILL,
            Wallpaper.FitMode.Fit => Shell32.DESKTOP_WALLPAPER_POSITION.DWPOS_FIT,
            Wallpaper.FitMode.Stretch => Shell32.DESKTOP_WALLPAPER_POSITION.DWPOS_STRETCH,
            Wallpaper.FitMode.Tile => Shell32.DESKTOP_WALLPAPER_POSITION.DWPOS_TILE,
            Wallpaper.FitMode.Center => Shell32.DESKTOP_WALLPAPER_POSITION.DWPOS_CENTER,
            Wallpaper.FitMode.Span => Shell32.DESKTOP_WALLPAPER_POSITION.DWPOS_SPAN,
            _ => throw new ArgumentOutOfRangeException(nameof(fitMode), fitMode, null)
        };
    }
}
