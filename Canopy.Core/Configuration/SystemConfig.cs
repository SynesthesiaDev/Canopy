// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Canopy.Configuration;

public record SystemConfig(
    bool UseLegacyWindowsApi,
    bool ApplyToAllMacOsSpaces,
    bool DontUpdateWhenBatteryLow,
    bool ChangeSystemThemesDependingOnTime
)
{
    public static readonly SystemConfig DEFAULT = new SystemConfig(
        UseLegacyWindowsApi: false,
        ApplyToAllMacOsSpaces: true,
        DontUpdateWhenBatteryLow: true,
        ChangeSystemThemesDependingOnTime: false
    );

    public static readonly StructCodec<SystemConfig> CODEC = StructCodec.For<SystemConfig>()
        .Field("UseLegacyWindowsApi", Codecs.BOOLEAN, s => s.UseLegacyWindowsApi)
        .Field("ApplyToAllMacOsSpaces", Codecs.BOOLEAN, s => s.ApplyToAllMacOsSpaces)
        .Field("DontUpdateWhenBatteryLow", Codecs.BOOLEAN, s => s.DontUpdateWhenBatteryLow)
        .Field("ChangeSystemThemesDependingOnTime", Codecs.BOOLEAN, s => s.ChangeSystemThemesDependingOnTime)
        .Build((b, b1, arg4, arg5) => new SystemConfig(b, b1, arg4, arg5));
}
