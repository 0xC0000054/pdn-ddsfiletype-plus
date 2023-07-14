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
    internal sealed partial class DDSSaveInfo
    {
        [CustomMarshaller(typeof(DDSSaveInfo), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
        public static class Marshaller
        {
            // This must be kept in sync with DDSSaveInfo.cs
            // and the DDSSaveInfo type in DdsFileTypePlusIO.h
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
                    format = managed.Format,
                    errorMetric = managed.ErrorMetric,
                    compressionSpeed = managed.CompressionSpeed,
                    errorDiffusionDithering = (byte)(managed.ErrorDiffusionDithering ? 1 : 0)
                };
            }
        }
    }
}
