// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// ReSharper disable InconsistentNaming
namespace Canopy.Utils;

public enum HardwareDecoder
{
    None,
    NVDEC,
    AMD_VCN,
    INTEL_QSV,
    FFMPEG
}
