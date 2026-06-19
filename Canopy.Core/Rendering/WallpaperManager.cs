// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Graphics;

namespace Canopy.Rendering;

public class WallpaperManager(ICanopyPlatform platform) : IDrawable
{
    public ICanopyPlatform Platform => platform;

    protected Wallpaper? CurrentWallpaper;
    protected Wallpaper? NextWallpaper;

    private bool firstSwap = true;

    public void Dispose()
    {
        CurrentWallpaper?.Dispose();
        NextWallpaper?.Dispose();
    }

    public void PushWallpaper(Wallpaper wallpaper)
    {
        NextWallpaper = wallpaper;
    }

    public void Draw(OpenGLRenderer gl)
    {
        var currentWallpaperCanDraw = CurrentWallpaper is { Texture.IsUploaded: true };
        if(currentWallpaperCanDraw) CurrentWallpaper!.Draw(gl);

        if (NextWallpaper is { Texture.IsUploaded: true })
        {
            if (currentWallpaperCanDraw)
            {
                //TODO CurrentWallpaper.FadeAlpha(5000, 1f, Easings.OutSine).Then(() => //swap wallpapers)
                CurrentWallpaper!.Alpha = 0f;
                var old = CurrentWallpaper;

                CurrentWallpaper = NextWallpaper;
                NextWallpaper = null;
                old.Dispose();
            }
            else
            {
                CurrentWallpaper = NextWallpaper;
                NextWallpaper = null;
            }
        }

        if (firstSwap && currentWallpaperCanDraw)
        {
            firstSwap = false;
            Platform.ShowWindow();
        }
    }
}
