// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Configuration;

public record UpdaterConfig(UpdaterConfig.Release ReleaseStream, bool AutoUpdate, string Source)
{

    public static readonly UpdaterConfig DEFAULT = new UpdaterConfig(Release.Release, true, "https://github.com/SynesthesiaDev/Canopy/releases");

    public static readonly StructCodec<UpdaterConfig> CODEC = StructCodec.For<UpdaterConfig>()
        .Field("ReleaseStream", Codecs.Enum<Release>(), u => u.ReleaseStream)
        .Field("AutoUpdate", Codecs.BOOLEAN, u => u.AutoUpdate)
        .Field("Source", Codecs.STRING, u => u.Source)
        .Build((release, b, arg3) => new UpdaterConfig(release, b, arg3));

    public enum Release
    {
        Release,
        PreRelease
    }

}
