////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2020 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus
{
    internal static class DdsNative
    {
        static DdsNative()
        {
            DdsImage.Initalize();
        }

        // Use a factory delegate to hide the DdsImage constructor from external code.
        // See https://stackoverflow.com/a/1664853
        private static Func<DDSLoadInfo, DdsImage> DdsImageFactory;

        public sealed class DdsImage : IDisposable
        {
            private DDSLoadInfo info;
            private bool disposed;

            public int Width => this.info.width;

            public int Height => this.info.height;

            private DdsImage(DDSLoadInfo info)
            {
                this.info = info;
            }

            internal static void Initalize()
            {
                DdsImageFactory = CreateImage;
            }

            private static DdsImage CreateImage(DDSLoadInfo info)
            {
                return new DdsImage(info);
            }

            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;

                    if (IntPtr.Size == 8)
                    {
                        DdsIO_x64.FreeLoadInfo(ref this.info);
                    }
                    else
                    {
                        DdsIO_x86.FreeLoadInfo(ref this.info);
                    }
                }
            }

            public unsafe byte* GetRowAddressUnchecked(int y)
            {
                return (byte*)this.info.scan0 + (y * this.info.stride);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public sealed class DDSSaveInfo
        {
            public int width;
            public int height;
            public int arraySize;
            public int mipLevels;
            public DdsFileFormat format;
            public DdsErrorMetric errorMetric;
            public BC7CompressionMode compressionMode;
            [MarshalAs(UnmanagedType.U1)]
            public bool cubeMap;
            [MarshalAs(UnmanagedType.U1)]
            public bool enableHardwareAcceleration;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DDSLoadInfo
        {
            public IntPtr scan0;
            public int width;
            public int height;
            public int stride;
        }

        private unsafe struct DDSBitmapData
        {
            public byte* scan0;
            public uint width;
            public uint height;
            public uint stride;

            public unsafe DDSBitmapData(Surface surface)
            {
                if (surface == null)
                {
                    throw new ArgumentNullException(nameof(surface));
                }

                this.scan0 = (byte*)surface.Scan0.VoidStar;
                this.width = (uint)surface.Width;
                this.height = (uint)surface.Height;
                this.stride = (uint)surface.Stride;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.U1)]
        public delegate bool DdsProgressCallback(UIntPtr done, UIntPtr total);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int ReadDelegate(IntPtr buffer, uint count, uint* bytesRead);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int WriteDelegate(IntPtr buffer, uint count, uint* bytesWritten);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SeekDelegate(long offset, int origin);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private unsafe delegate int GetSizeDelegate(long* size);

        [StructLayout(LayoutKind.Sequential)]
        private sealed class IOCallbacks
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ReadDelegate Read;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WriteDelegate Write;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public SeekDelegate Seek;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public GetSizeDelegate GetSize;
        }

        private static class DdsIO_x86
        {
            private const string DllName = "DdsFileTypePlusIO_x86.dll";

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern int Load([In] IOCallbacks callbacks, [In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeLoadInfo([In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern unsafe int Save(
                [In] DDSSaveInfo input,
                [In] DDSBitmapData* bitmapData,
                [In] uint bitmapDataLength,
                [In] IOCallbacks callbacks,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
        }

        private static class DdsIO_x64
        {
            private const string DllName = "DdsFileTypePlusIO_x64.dll";

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern int Load([In] IOCallbacks callbacks, [In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeLoadInfo([In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
            internal static extern unsafe int Save(
                [In] DDSSaveInfo input,
                [In] DDSBitmapData* bitmapData,
                [In] uint bitmapDataLength,
                [In] IOCallbacks callbacks,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
        }

        private static class HResult
        {
            public const int S_OK = 0;
            public const int E_POINTER = unchecked((int)0x80004003);
            public const int CanceledError = unchecked((int)0x800704C7); // HRESULT_FROM_WIN32(ERROR_CANCELLED)
            public const int SeekError = unchecked((int)0x80070019); // HRESULT_FROM_WIN32(ERROR_SEEK)
            public const int NotSupported = unchecked((int)0x80070032); // HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED)
            public const int InvalidData = unchecked((int)0x8007000D); // HRESULT_FROM_WIN32(ERROR_INVALID_DATA)
        }

        private static bool FAILED(int hr)
        {
            return hr < 0;
        }

        public static unsafe DdsImage Load(Stream stream)
        {
            StreamIOCallbacks streamIO = new StreamIOCallbacks(stream);
            IOCallbacks callbacks = new IOCallbacks
            {
                Read = streamIO.Read,
                Write = streamIO.Write,
                Seek = streamIO.Seek,
                GetSize = streamIO.GetSize
            };

            int hr;
            DDSLoadInfo info = new DDSLoadInfo();

            if (IntPtr.Size == 8)
            {
                hr = DdsIO_x64.Load(callbacks, ref info);
            }
            else
            {
                hr = DdsIO_x86.Load(callbacks, ref info);
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
                        case HResult.InvalidData:
                            throw new FormatException("The DDS file is invalid.");
                        case HResult.NotSupported:
                            throw new FormatException("The file is not a supported DDS format.");
                        default:
                            Marshal.ThrowExceptionForHR(hr);
                            break;
                    }
                }
            }

            return DdsImageFactory(info);
        }

        public static unsafe void Save(
            DDSSaveInfo info,
            TextureCollection textures,
            Stream output,
            DdsProgressCallback progressCallback)
        {
            StreamIOCallbacks streamIO = new StreamIOCallbacks(output);
            IOCallbacks callbacks = new IOCallbacks
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
                    if (IntPtr.Size == 8)
                    {
                        hr = DdsIO_x64.Save(info, pBitmapData, (uint)bitmapData.Length, callbacks, progressCallback);
                    }
                    else
                    {
                        hr = DdsIO_x86.Save(info, pBitmapData, (uint)bitmapData.Length, callbacks, progressCallback);
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

        private sealed class StreamIOCallbacks
        {
            private readonly Stream stream;
            private readonly byte[] streamBuffer;

            // 81920 is the largest multiple of 4096 that is below the large object heap threshold.
            private const int MaxBufferSize = 81920;

            public StreamIOCallbacks(Stream stream)
            {
                this.stream = stream;
                this.streamBuffer = new byte[MaxBufferSize];
                this.CallbackExceptionInfo = null;
            }

            public ExceptionDispatchInfo CallbackExceptionInfo
            {
                get;
                private set;
            }

            public unsafe int Read(IntPtr buffer, uint count, uint* bytesRead)
            {
                if (bytesRead != null)
                {
                    *bytesRead = 0;
                }

                if (count == 0)
                {
                    return HResult.S_OK;
                }

                try
                {
                    long totalBytesRead = 0;
                    long remaining = count;

                    do
                    {
                        int streamBytesRead = this.stream.Read(this.streamBuffer, 0, (int)Math.Min(MaxBufferSize, remaining));

                        if (streamBytesRead == 0)
                        {
                            break;
                        }

                        Marshal.Copy(this.streamBuffer, 0, new IntPtr(buffer.ToInt64() + totalBytesRead), streamBytesRead);

                        totalBytesRead += streamBytesRead;
                        remaining -= streamBytesRead;

                    } while (remaining > 0);

                    if (bytesRead != null)
                    {
                        *bytesRead = (uint)totalBytesRead;
                    }
                    return HResult.S_OK;
                }
                catch (Exception ex)
                {
                    this.CallbackExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                    return ex.HResult;
                }
            }

            public unsafe int Write(IntPtr buffer, uint count, uint* bytesWritten)
            {
                if (bytesWritten != null)
                {
                    *bytesWritten = 0;
                }

                if (count == 0)
                {
                    return HResult.S_OK;
                }

                try
                {
                    long offset = 0;
                    long remaining = count;

                    do
                    {
                        int copySize = (int)Math.Min(MaxBufferSize, remaining);

                        Marshal.Copy(new IntPtr(buffer.ToInt64() + offset), this.streamBuffer, 0, copySize);

                        this.stream.Write(this.streamBuffer, 0, copySize);

                        offset += copySize;
                        remaining -= copySize;

                    } while (remaining > 0);

                    if (bytesWritten != null)
                    {
                        *bytesWritten = count;
                    }

                    return HResult.S_OK;
                }
                catch (Exception ex)
                {
                    this.CallbackExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                    return ex.HResult;
                }
            }

            public int Seek(long offset, int origin)
            {
                int hr = HResult.S_OK;

                try
                {
                    long newPosition = this.stream.Seek(offset, (SeekOrigin)origin);

                    if (newPosition != offset)
                    {
                        hr = HResult.SeekError;
                    }
                }
                catch (Exception ex)
                {
                    this.CallbackExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                    hr = ex.HResult;
                }

                return hr;
            }

            public unsafe int GetSize(long* size)
            {
                if (size != null)
                {
                    *size = 0;
                }
                else
                {
                    return HResult.E_POINTER;
                }

                int hr = HResult.S_OK;

                try
                {
                    *size = this.stream.Length;
                }
                catch (Exception ex)
                {
                    this.CallbackExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                    hr = ex.HResult;
                }

                return hr;
            }
        }
    }
}
