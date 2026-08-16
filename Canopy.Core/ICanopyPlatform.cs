using Synesthesia.Utils;

namespace Canopy;

public interface ICanopyPlatform
{
    RuntimeInfo.Platform Platform { get; }
    void SetDesktop(string path);
    void SetTheme(Theme theme);
    void InitializeTray(Canopy canopy);
    void ShowNotification(string title, string message, NotificationLevel level = NotificationLevel.Info);

    void BlockThread();

    enum Theme
    {
        Light,
        Dark
    }

    enum NotificationLevel
    {
        Info,
        Warning,
        Error
    }
}
