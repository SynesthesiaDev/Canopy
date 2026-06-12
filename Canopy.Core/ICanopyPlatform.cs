using Synesthesia.Utils;

namespace Canopy;

public interface ICanopyPlatform
{
    RuntimeInfo.Platform Platform { get; }

    void Initialize();

    void InjectIntoDesktop(IntPtr sdlWindowHandle);
}
