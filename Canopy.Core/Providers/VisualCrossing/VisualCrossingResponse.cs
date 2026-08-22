// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Providers.VisualCrossing;

public record VisualCrossingResponse(
    int QueryCost,
    double Latitude,
    double Longitude,
    string ResolvedAddress,
    string Address,
    string Timezone,
    double TimezoneOffset,
    CurrentConditions CurrentConditions
)
{
    public static readonly Codec<VisualCrossingResponse> CODEC = StructCodec
        .For<VisualCrossingResponse>()
        .Field("queryCost", Codecs.INT, c => c.QueryCost)
        .Field("latitude", Codecs.DOUBLE, c => c.Latitude)
        .Field("longitude", Codecs.DOUBLE, c => c.Longitude)
        .Field("resolvedAddress", Codecs.STRING, c => c.ResolvedAddress)
        .Field("address", Codecs.STRING, c => c.Address)
        .Field("timezone", Codecs.STRING, c => c.Timezone)
        .Field("tzoffset", Codecs.DOUBLE, c => c.TimezoneOffset)
        .Field("currentConditions", CurrentConditions.CODEC, c => c.CurrentConditions)
        .Build((querycost, latitude, longitude, resolvedaddress, address, timezone, tzoffset, currentconditions) => new VisualCrossingResponse(querycost, latitude, longitude, resolvedaddress, address, timezone, tzoffset, currentconditions));
}
