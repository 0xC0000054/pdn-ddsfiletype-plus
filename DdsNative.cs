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

using DdsFileTypePlus.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus
{
    internal static class DdsNative
    {
        public static unsafe DdsImage Load(Stream stream)
        {
            StreamIOCallbacks streamIO = new(stream);
            IOCallbacks callbacks = new()
            {
                Read = streamIO.Read,
                Write = streamIO.Write,
                Seek = streamIO.Seek,
                GetSize = streamIO.GetSize
            };

            int hr;
            DDSLoadInfo info = new();

            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                hr = DdsIO_x64.Load(callbacks, info);
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                hr = DdsIO_ARM64.Load(callbacks, info);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            GC.KeepAlive(streamIO);
            GC.KeepAlive(callbacks);

            if (FAILED(hr))
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

            return new DdsImage(info);
        }

        public static unsafe void Save(
            DDSSaveInfo info,
            TextureCollection textures,
            Stream output,
            IntPtr directComputeAdapter,
            DdsProgressCallback progressCallback)
        {
            StreamIOCallbacks streamIO = new(output);
            IOCallbacks callbacks = new()
            {
                Read = streamIO.Read,
                Write = streamIO.Write,
                Seek = streamIO.Seek,
                GetSize = streamIO.GetSize
            };

            DDSBitmapData[] bitmapData = CreateBitmapDataArray(textures, info.arraySize, info.mipLevels);

            int hr;

            unsafe
            {
                fixed (DDSBitmapData* pBitmapData = bitmapData)
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    {
                        hr = DdsIO_x64.Save(info,
                                            pBitmapData,
                                            (uint)bitmapData.Length,
                                            callbacks,
                                            directComputeAdapter,
                                            progressCallback);
                    }
                    else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    {
                        hr = DdsIO_ARM64.Save(info,
                                              pBitmapData,
                                              (uint)bitmapData.Length,
                                              callbacks,
                                              directComputeAdapter,
                                              progressCallback);
                    }
                    else
                    {
                        throw new PlatformNotSupportedException();
                    }
                }
            }

            GC.KeepAlive(streamIO);
            GC.KeepAlive(callbacks);
            GC.KeepAlive(progressCallback);

            if (FAILED(hr))
            {
                if (streamIO.CallbackExceptionInfo != null)
                {
                    streamIO.CallbackExceptionInfo.Throw();
                }
                else
                {
                    switch (hr)
                    {
                        case HResult.CanceledError:
                            throw new OperationCanceledException();
                        case HResult.UnknownDdsSaveFormat:
                            throw new InvalidOperationException("The DDSFileFormat value does not map to a DXGI format.");
                        default:
                            Marshal.ThrowExceptionForHR(hr);
                            break;
                    }
                }
            }
        }

        private static DDSBitmapData[] CreateBitmapDataArray(TextureCollection textures, int arraySize, int mipLevels)
        {
            DDSBitmapData[] array = new DDSBitmapData[textures.Count];

            for (int i = 0; i < arraySize; ++i)
            {
                int startIndex = i * mipLevels;

                for (int j = 0; j < mipLevels; ++j)
                {
                    int index = startIndex + j;

                    array[index] = new DDSBitmapData(textures[index].Surface);
                }
            }

            return array;
        }

        private static bool FAILED(int hr)
        {
            return hr < 0;
        }
    }
}
