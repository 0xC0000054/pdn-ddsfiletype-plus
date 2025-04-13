﻿////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2025 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    // This must be kept in sync with DDSSaveInfo.cs
    // and the DDSSaveInfo type in DdsFileTypePlusIO.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeDdsSaveInfo
    {
        public DXGI_FORMAT format;
        public DdsFileOptions fileOptions;
        public DdsErrorMetric errorMetric;
        public BC7CompressionSpeed compressionSpeed;
        public byte errorDiffusionDithering;
    }
}
