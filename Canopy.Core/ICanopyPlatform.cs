using Synesthesia.Utils;

namespace Canopy;

public interface ICanopyPlatform
{
    RuntimeInfo.Platform Platform { get; }
    void SetDesktop(byte[] image);
}
