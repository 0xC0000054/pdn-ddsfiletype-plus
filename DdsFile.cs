////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017, 2018 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus
{
    static class DdsFile
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct DDSLoadInfo
        {
            public int width;
            public int height;
            public int stride;
            public IntPtr scan0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DDSSaveInfo
        {
            public int width;
            public int height;
            public int stride;
            public DdsFileFormat format;
            public DdsErrorMetric errorMetric;
            public BC7CompressionMode compressionMode;
            [MarshalAs(UnmanagedType.U1)]
            public bool generateMipmaps;
            public MipMapSampling mipmapSampling;
            public IntPtr scan0;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void DdsProgressCallback(UIntPtr done, UIntPtr total);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint ReadDelegate(IntPtr buffer, uint count);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint WriteDelegate(IntPtr buffer, uint count);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate long SeekDelegate(long offset, int origin);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate long GetSizeDelegate();

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
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport("DdsFileTypePlusIO_x86.dll", CallingConvention = CallingConvention.StdCall)]
            internal static unsafe extern int Load([In] IOCallbacks callbacks, [In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport("DdsFileTypePlusIO_x86.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeLoadInfo([In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport("DdsFileTypePlusIO_x86.dll", CallingConvention = CallingConvention.StdCall)]
            internal static unsafe extern int Save(
                [In] ref DDSSaveInfo input,
                [In] IOCallbacks callbacks,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
        }

        private static class DdsIO_x64
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport("DdsFileTypePlusIO_x64.dll", CallingConvention = CallingConvention.StdCall)]
            internal static unsafe extern int Load([In] IOCallbacks callbacks, [In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport("DdsFileTypePlusIO_x64.dll", CallingConvention = CallingConvention.StdCall)]
            internal static extern void FreeLoadInfo([In, Out] ref DDSLoadInfo info);

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
            [DllImport("DdsFileTypePlusIO_x64.dll", CallingConvention = CallingConvention.StdCall)]
            internal static unsafe extern int Save(
                [In] ref DDSSaveInfo input,
                [In] IOCallbacks callbacks,
                [In, MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
        }

        private static class HResult
        {
            public const int NotSupported = unchecked((int)0x80070032); // HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED)
            public const int InvalidData = unchecked((int)0x8007000D); // HRESULT_FROM_WIN32(ERROR_INVALID_DATA)
        }

        private static bool FAILED(int hr)
        {
            return hr < 0;
        }

        public static unsafe Document Load(Stream input)
        {
            DDSLoadInfo info = new DDSLoadInfo();

            LoadDdsFile(input, ref info);

            Document doc = null;

            try
            {
                doc = new Document(info.width, info.height);

                BitmapLayer layer = Layer.CreateBackgroundLayer(info.width, info.height);

                Surface surface = layer.Surface;

                for (int y = 0; y < surface.Height; y++)
                {
                    byte* src = (byte*)info.scan0 + (y * info.stride);
                    ColorBgra* dst = surface.GetRowAddressUnchecked(y);

                    for (int x = 0; x < surface.Width; x++)
                    {
                        dst->R = src[0];
                        dst->G = src[1];
                        dst->B = src[2];
                        dst->A = src[3];

                        src += 4;
                        dst++;
                    }
                }

                doc.Layers.Add(layer);
            }
            finally
            {
                FreeLoadInfo(ref info);
            }

            return doc;
        }

        public static unsafe void Save(
            Document input,
            Stream output,
            DdsFileFormat format,
            DdsErrorMetric errorMetric,
            BC7CompressionMode compressionMode,
            bool generateMipmaps,
            MipMapSampling sampling,
            Surface scratchSurface,
            ProgressEventHandler progressCallback)
        {
            using (RenderArgs args = new RenderArgs(scratchSurface))
            {
                input.Render(args, true);
            }

            DdsProgressCallback ddsProgress = delegate (UIntPtr done, UIntPtr total)
            {
                double progress = (double)done.ToUInt64() / (double)total.ToUInt64();
                progressCallback(null, new ProgressEventArgs(progress * 100.0, true));
            };

            SaveDdsFile(scratchSurface, format, errorMetric, compressionMode, generateMipmaps, sampling, output, ddsProgress);
        }

        private static unsafe void LoadDdsFile(Stream stream, ref DDSLoadInfo info)
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

        private static void FreeLoadInfo(ref DDSLoadInfo info)
        {
            if (IntPtr.Size == 8)
            {
                DdsIO_x64.FreeLoadInfo(ref info);
            }
            else
            {
                DdsIO_x86.FreeLoadInfo(ref info);
            }
        }

        private static unsafe void SaveDdsFile(
            Surface surface,
            DdsFileFormat format,
            DdsErrorMetric errorMetric,
            BC7CompressionMode compressionMode,
            bool generateMipmaps,
            MipMapSampling mipMapSampling,
            Stream output,
            DdsProgressCallback progressCallback)
        {
            DDSSaveInfo info = new DDSSaveInfo
            {
                width = surface.Width,
                height = surface.Height,
                stride = surface.Stride,
                format = format,
                errorMetric = errorMetric,
                compressionMode = compressionMode,
                generateMipmaps = generateMipmaps,
                mipmapSampling = mipMapSampling,
                scan0 = surface.Scan0.Pointer
            };

            StreamIOCallbacks streamIO = new StreamIOCallbacks(output);
            IOCallbacks callbacks = new IOCallbacks
            {
                Read = streamIO.Read,
                Write = streamIO.Write,
                Seek = streamIO.Seek,
                GetSize = streamIO.GetSize
            };

            int hr;

            if (IntPtr.Size == 8)
            {
                hr = DdsIO_x64.Save(ref info, callbacks, progressCallback);
            }
            else
            {
                hr = DdsIO_x86.Save(ref info, callbacks, progressCallback);
            }

            GC.KeepAlive(streamIO);
            GC.KeepAlive(callbacks);
            GC.KeepAlive(progressCallback);

            if (FAILED(hr))
            {
                Marshal.ThrowExceptionForHR(hr);
            }
        }

        private sealed class StreamIOCallbacks
        {
            private readonly Stream stream;

            // 81920 is the largest multiple of 4096 that is below the large object heap threshold.
            private const int MaxBufferSize = 81920;

            public StreamIOCallbacks(Stream stream)
            {
                this.stream = stream;
            }

            public uint Read(IntPtr buffer, uint count)
            {
                if (count == 0)
                {
                    return 0;
                }

                int bufferSize = (int)Math.Min(MaxBufferSize, count);
                byte[] bytes = new byte[bufferSize];

                long totalBytesRead = 0;
                long remaining = count;

                do
                {
                    int bytesRead = stream.Read(bytes, 0, (int)Math.Min(MaxBufferSize, remaining));

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    Marshal.Copy(bytes, 0, new IntPtr(buffer.ToInt64() + totalBytesRead), bytesRead);

                    totalBytesRead += bytesRead;
                    remaining -= bytesRead;

                } while (remaining > 0);

                return (uint)totalBytesRead;
            }

            public uint Write(IntPtr buffer, uint count)
            {
                if (count > 0)
                {
                    int bufferSize = (int)Math.Min(MaxBufferSize, count);
                    byte[] bytes = new byte[bufferSize];

                    long offset = 0;
                    long remaining = count;

                    do
                    {
                        int copySize = (int)Math.Min(MaxBufferSize, remaining);

                        Marshal.Copy(new IntPtr(buffer.ToInt64() + offset), bytes, 0, copySize);

                        stream.Write(bytes, 0, copySize);

                        offset += copySize;
                        remaining -= copySize;

                    } while (remaining > 0);
                }

                return count;
            }

            public long Seek(long offset, int origin)
            {
                return stream.Seek(offset, (SeekOrigin)origin);
            }

            public long GetSize()
            {
                return stream.Length;
            }
        }
    }
}
