using Canopy.Graphics;
using Synesthesia.Utils;

namespace Canopy;

public interface ICanopyPlatform
{
    RuntimeInfo.Platform Platform { get; }

    void Initialize();

    void InjectIntoDesktop(IntPtr chibiWindowHandle);

    void HideWindow();
    void ShowWindow();

    void PushWallpaper(Wallpaper wallpaper);
}
