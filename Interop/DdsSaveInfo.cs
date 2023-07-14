////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2023 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices.Marshalling;

namespace DdsFileTypePlus.Interop
{
    // This must be kept in sync with the Native structure in DDSSaveInfo.Marshaller.cs
    // and the DDSSaveInfo type in DdsFileTypePlusIO.h
    [NativeMarshalling(typeof(Marshaller))]
    internal sealed partial class DDSSaveInfo
    {
        public DXGI_FORMAT format;
        public DdsErrorMetric errorMetric;
        public BC7CompressionSpeed compressionSpeed;
        public bool errorDiffusionDithering;
    }
}
