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

using DdsFileTypePlus.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus
{
    internal static class DdsNative
    {
        public static unsafe DirectXTexScratchImage Load(Stream stream, out DDSLoadInfo info)
        {
            StreamIOCallbacks streamIO = new(stream);
            IOCallbacks callbacks = streamIO.GetIOCallbacks();

            int hr;
            SafeDirectXTexScratchImage scratchImageHandle;

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                hr = DdsIO_x64.Load(ref callbacks, out info, out scratchImageHandle);
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                hr = DdsIO_ARM64.Load(ref callbacks, out info, out scratchImageHandle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            GC.KeepAlive(streamIO);

            if (HResult.Failed(hr))
            {
                if (streamIO.CallbackExceptionInfo != null)
                {
                    streamIO.CallbackExceptionInfo.Throw();
                }
                else
                {
                    switch (hr)
                    {
                        case HResult.InvalidDdsFileSignature:
                        case HResult.InvalidData:
                            throw new FormatException("The DDS file is invalid.") { HResult = hr };
                        case HResult.NotSupported:
                            throw new FormatException("The file is not a supported DDS format.") { HResult = hr };
                        default:
                            Marshal.ThrowExceptionForHR(hr);
                            break;
                    }
                }
            }

            DirectXTexScratchImage scratchImage;
            try
            {
                scratchImage = new DirectXTexScratchImage(scratchImageHandle);
                scratchImageHandle = null;
            }
            finally
            {
                scratchImageHandle?.Dispose();
            }

            return scratchImage;
        }

        public static unsafe void Save(
            DDSSaveInfo info,
            DirectXTexScratchImage image,
            Stream output,
            IntPtr directComputeAdapter,
            DdsProgressCallback progressCallback)
        {
            StreamIOCallbacks streamIO = new(output);
            IOCallbacks callbacks = streamIO.GetIOCallbacks();
            NativeDdsSaveInfo nativeDdsSaveInfo = info.ToNative();

            int hr;

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                hr = DdsIO_x64.Save(ref nativeDdsSaveInfo,
                                    image.SafeDirectXTexScratchImage,
                                    ref callbacks,
                                    directComputeAdapter,
                                    progressCallback);
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                hr = DdsIO_ARM64.Save(ref nativeDdsSaveInfo,
                                      image.SafeDirectXTexScratchImage,
                                      ref callbacks,
                                      directComputeAdapter,
                                      progressCallback);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            GC.KeepAlive(streamIO);
            GC.KeepAlive(progressCallback);

            if (HResult.Failed(hr))
            {
                if (streamIO.CallbackExceptionInfo != null)
                {
                    streamIO.CallbackExceptionInfo.Throw();
                }
                else
                {
                    switch (hr)
                    {
                        case HResult.E_ABORT:
                            throw new OperationCanceledException();
                        default:
                            Marshal.ThrowExceptionForHR(hr);
                            break;
                    }
                }
            }
        }
    }
}
