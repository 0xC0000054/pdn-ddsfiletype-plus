////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2024 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

namespace DdsFileTypePlus.Interop
{
    // This must be kept in sync with NativeDdsSaveInfo.cs
    // and the DDSSaveInfo type in DdsFileTypePlusIO.h
    internal sealed partial class DDSSaveInfo
    {
        public DXGI_FORMAT Format { get; init; }

        public DdsFileOptions FileOptions { get; init; }

        public DdsErrorMetric ErrorMetric { get; init; }

        public BC7CompressionSpeed CompressionSpeed { get; init; }

        public bool ErrorDiffusionDithering { get; init; }

        public NativeDdsSaveInfo ToNative() => new()
        {
            format = this.Format,
            fileOptions = this.FileOptions,
            errorMetric = this.ErrorMetric,
            compressionSpeed = this.CompressionSpeed,
            errorDiffusionDithering = (byte)(this.ErrorDiffusionDithering ? 1 : 0)
        };
    }
}
