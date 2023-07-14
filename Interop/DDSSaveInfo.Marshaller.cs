﻿////////////////////////////////////////////////////////////////////////
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
    internal sealed partial class DDSSaveInfo
    {
        [CustomMarshaller(typeof(DDSSaveInfo), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
        public static class Marshaller
        {
            public struct Native
            {
                public DXGI_FORMAT format;
                public DdsErrorMetric errorMetric;
                public BC7CompressionSpeed compressionSpeed;
                public byte errorDiffusionDithering;
            }

            public static Native ConvertToUnmanaged(DDSSaveInfo managed)
            {
                return new Native
                {
                    format = managed.format,
                    errorMetric = managed.errorMetric,
                    compressionSpeed = managed.compressionSpeed,
                    errorDiffusionDithering = (byte)(managed.errorDiffusionDithering ? 1 : 0)
                };
            }
        }
    }
}