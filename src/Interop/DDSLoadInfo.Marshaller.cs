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

using System.Runtime.InteropServices.Marshalling;

namespace DdsFileTypePlus.Interop
{
    internal sealed partial class DDSLoadInfo
    {
        [CustomMarshaller(typeof(DDSLoadInfo), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
        public static class Marshaller
        {
            // This must be kept in sync with DDSLoadInfo.cs
            // and the DDSLoadInfo type in DdsFileTypePlusIO.h
            public unsafe struct Native
            {
                public nuint width;
                public nuint height;
                public nuint depth;
                public nuint arraySize;
                public nuint mipLevels;
                public SwizzledImageFormat swizzledImageFormat;
                public byte cubeMap;
                public byte premultipliedAlpha;
                public byte volumeMap;
            }

            public static DDSLoadInfo ConvertToManaged(Native unmanaged)
            {
                return new DDSLoadInfo()
                {
                    Width = unmanaged.width,
                    Height = unmanaged.height,
                    Depth = unmanaged.depth,
                    ArraySize = unmanaged.arraySize,
                    MipLevels = unmanaged.mipLevels,
                    SwizzledImageFormat = unmanaged.swizzledImageFormat,
                    CubeMap = unmanaged.cubeMap != 0,
                    PremultipliedAlpha = unmanaged.premultipliedAlpha != 0,
                    VolumeMap = unmanaged.volumeMap != 0,
                };
            }
        }
    }
}
