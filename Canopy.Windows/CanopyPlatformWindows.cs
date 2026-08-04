// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Synesthesia.Utils;

namespace Canopy.Windows;

public class CanopyPlatformWindows : ICanopyPlatform
{
    RuntimeInfo.Platform ICanopyPlatform.Platform => RuntimeInfo.Platform.Windows;

    public void SetDesktop(byte[] image)
    {

    }

}
