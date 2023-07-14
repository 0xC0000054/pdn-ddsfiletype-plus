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
    internal sealed unsafe partial class DirectXTexScratchImageData
    {
        [CustomMarshaller(typeof(DirectXTexScratchImageData), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
        public static class Marshaller
        {
            // This must be kept in sync with DirectXTexScratchImageData.cs
            // and the ScratchImageData type in DdsFileTypePlusIO.h
            public unsafe struct Native
            {
                public byte* pixels;
                public nuint width;
                public nuint height;
                public nuint stride;
                public nuint totalImageDataSize;
                public DXGI_FORMAT format;
            }

            public static DirectXTexScratchImageData ConvertToManaged(Native unmanaged)
            {
                return new DirectXTexScratchImageData()
                {
                    pixels = unmanaged.pixels,
                    width = unmanaged.width,
                    height = unmanaged.height,
                    stride = unmanaged.stride,
                    totalImageDataSize = unmanaged.totalImageDataSize,
                    format = unmanaged.format,
                };
            }
        }
    }
}
