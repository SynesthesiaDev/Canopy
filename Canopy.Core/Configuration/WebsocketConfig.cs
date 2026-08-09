// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Configuration;

public record WebsocketConfig(bool Enabled, string Url)
{
    public static readonly WebsocketConfig DEFAULT = new WebsocketConfig(false, "http://localhost:5808/");

    public static readonly StructCodec<WebsocketConfig> CODEC = StructCodec.For<WebsocketConfig>()
        .Field("Enabled", Codecs.BOOLEAN, s => s.Enabled)
        .Field("Url", Codecs.STRING, s => s.Url)
        .Build((b, s) => new WebsocketConfig(b, s));
}
