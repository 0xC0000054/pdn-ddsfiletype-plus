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

using PaintDotNet;
using PaintDotNet.Imaging;
using System;
using System.Runtime.InteropServices.Marshalling;

namespace DdsFileTypePlus.Interop
{
    // This must be kept in sync with the Native structure in DirectXTexScratchImageData.Marshaller.cs
    // and the ScratchImageData type in DdsFileTypePlusIO.h
    [NativeMarshalling(typeof(Marshaller))]
    internal sealed unsafe partial class DirectXTexScratchImageData
    {
        public byte* pixels;
        public nuint width;
        public nuint height;
        public nuint stride;
        public nuint totalImageDataSize;
        public DXGI_FORMAT format;

        public unsafe RegionPtr<T> AsRegionPtr<T>(bool checkPixelType = true) where T : unmanaged
        {
            if (checkPixelType)
            {
                EnsureCompatiblePixelType(typeof(T));
            }

            return new((T*)this.pixels,
                       checked((int)this.width),
                       checked((int)this.height),
                       checked((int)this.stride));
        }

        private void EnsureCompatiblePixelType(Type pixelType)
        {
            switch (this.format)
            {
            case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
            case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
                VerifyPixelTypeMatches(pixelType, typeof(ColorBgra32));
                break;
            case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
            case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
                VerifyPixelTypeMatches(pixelType, typeof(ColorRgba32));
                break;
            default:
                throw new InvalidOperationException($"Unsupported {nameof(DXGI_FORMAT)} value: {this.format}.");
            }

            void VerifyPixelTypeMatches(Type pixelType, Type expectedType)
            {
                if (pixelType != expectedType)
                {
                    string message = $"Unexpected pixel type for {this.format}, pixelType={pixelType} expectedType={expectedType}.";
                    ExceptionUtil.ThrowInvalidOperationException(message);
                }
            }
        }
    }
}
