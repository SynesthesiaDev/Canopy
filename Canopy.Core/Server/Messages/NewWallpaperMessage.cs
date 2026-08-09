// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using Codon.Codec;
using Codon.Codec.Json;

namespace Canopy.Server.Messages;

public record NewWallpaperMessage(long Timestamp, Wallpaper Wallpaper) : ISocketMessage
{
    public static readonly StructCodec<NewWallpaperMessage> CODEC = StructCodec.For<NewWallpaperMessage>()
        .Field("Timestamp", Codecs.LONG, w => w.Timestamp)
        .Field("Wallpaper", Wallpaper.CODEC, w => w.Wallpaper)
        .Build((time, wallpaper) => new NewWallpaperMessage(time, wallpaper));

    public string Encode() => CODEC.Encode(JsonTranscoder.INSTANCE, this).ToStringPretty();

    public ISocketMessage Decode(string message)
    {
        throw new NotImplementedException();
    }
}
