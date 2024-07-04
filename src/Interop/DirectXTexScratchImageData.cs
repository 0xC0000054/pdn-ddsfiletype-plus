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
        public unsafe byte* Pixels { get; init; }

        public nuint Width { get; init; }

        public nuint Height { get; init; }

        public nuint Stride { get; init; }

        public nuint TotalImageDataSize { get; init; }

        public DXGI_FORMAT Format { get; init; }

        public unsafe RegionPtr<T> AsRegionPtr<T>(bool checkPixelType = true) where T : unmanaged
        {
            if (checkPixelType)
            {
                EnsureCompatiblePixelType(typeof(T));
            }

            return new((T*)this.Pixels,
                       checked((int)this.Width),
                       checked((int)this.Height),
                       checked((int)this.Stride));
        }

        private void EnsureCompatiblePixelType(Type pixelType)
        {
            switch (this.Format)
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
                throw new InvalidOperationException($"Unsupported {nameof(DXGI_FORMAT)} value: {this.Format}.");
            }

            void VerifyPixelTypeMatches(Type pixelType, Type expectedType)
            {
                if (pixelType != expectedType)
                {
                    string message = $"Unexpected pixel type for {this.Format}, pixelType={pixelType} expectedType={expectedType}.";
                    ExceptionUtil.ThrowInvalidOperationException(message);
                }
            }
        }
    }
}
