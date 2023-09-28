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
using System;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    internal sealed class DirectXTexScratchImage : Disposable
    {
        private SafeDirectXTexScratchImage image;

        public DirectXTexScratchImage(int width,
                                      int height,
                                      int arraySize,
                                      int mipLevels,
                                      DXGI_FORMAT format,
                                      bool cubeMap)
        {
            this.image = CreateNativeImage(width, height, arraySize, mipLevels, format, cubeMap);
        }

        public DirectXTexScratchImage(SafeDirectXTexScratchImage image)
        {
            this.image = image;
        }

        public SafeDirectXTexScratchImage SafeDirectXTexScratchImage
        {
            get
            {
                VerifyNotDisposed();
                return this.image;
            }
        }

        public DirectXTexScratchImageData GetImageData(nuint mip, nuint item, nuint slice)
        {
            VerifyNotDisposed();

            DirectXTexScratchImageData imageData;
            int hr;

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                hr = DdsIO_x64.GetScratchImageData(this.image, mip, item, slice, out imageData);
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                hr = DdsIO_ARM64.GetScratchImageData(this.image, mip, item, slice, out imageData);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            if (HResult.Failed(hr))
            {
                throw hr switch
                {
                    HResult.InvalidArgument => new FormatException("A native method argument is invalid."),
                    HResult.NotFound => new FormatException("The specified item was not found in the image."),
                    _ => new FormatException($"An unspecified error occurred when getting the scratch image data, hr = 0x{hr:X8}."),
                };
            }

            return imageData;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free(ref this.image);
            }

            base.Dispose(disposing);
        }

        private static SafeDirectXTexScratchImage CreateNativeImage(int width,
                                                                    int height,
                                                                    int arraySize,
                                                                    int mipLevels,
                                                                    DXGI_FORMAT format,
                                                                    bool cubeMap)
        {
            SafeDirectXTexScratchImage image;
            int hr;

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                hr = DdsIO_x64.CreateScratchImage(width, height, format, arraySize, mipLevels, cubeMap, out image);
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                hr = DdsIO_ARM64.CreateScratchImage(width, height, format, arraySize, mipLevels, cubeMap, out image);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            if (HResult.Failed(hr))
            {
                throw hr switch
                {
                    HResult.InvalidArgument => new FormatException("A native method argument is invalid."),
                    HResult.OutOfMemory => new OutOfMemoryException(),
                    _ => new FormatException($"An unspecified error occurred when creating the scratch image, hr = 0x{hr:X8}."),
                };
            }

            return image;
        }

        private void VerifyNotDisposed()
        {
            if (this.IsDisposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(DirectXTexScratchImage));
            }
        }
    }
}
