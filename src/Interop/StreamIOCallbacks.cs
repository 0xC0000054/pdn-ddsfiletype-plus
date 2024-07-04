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

using System;
using System.Buffers;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    internal sealed class StreamIOCallbacks
    {
        private readonly Stream stream;
        private readonly ReadDelegate read;
        private readonly WriteDelegate write;
        private readonly SeekDelegate seek;
        private readonly GetSizeDelegate getSize;

        // 81920 is the largest multiple of 4096 that is below the large object heap threshold.
        private const int MaxBufferSize = 81920;

        public unsafe StreamIOCallbacks(Stream stream)
        {
            this.stream = stream;
            this.read = Read;
            this.write = Write;
            this.seek = Seek;
            this.getSize = GetSize;
            this.CallbackExceptionInfo = null;
        }

        public ExceptionDispatchInfo CallbackExceptionInfo
        {
            get;
            private set;
        }

        public IOCallbacks GetIOCallbacks()
        {
            return new IOCallbacks()
            {
                Read = Marshal.GetFunctionPointerForDelegate(this.read),
                Write = Marshal.GetFunctionPointerForDelegate(this.write),
                Seek = Marshal.GetFunctionPointerForDelegate(this.seek),
                GetSize = Marshal.GetFunctionPointerForDelegate(this.getSize)
            };
        }

        private unsafe int Read(IntPtr buffer, uint count)
        {
            if (count == 0)
            {
                return HResult.S_OK;
            }

            try
            {
                long totalBytesRead = 0;
                long remaining = count;
                byte[] streamBuffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);

                try
                {
                    do
                    {
                        int streamBytesRead = this.stream.Read(streamBuffer, 0, (int)Math.Min(streamBuffer.Length, remaining));

                        if (streamBytesRead == 0)
                        {
                            return HResult.EndOfFile;
                        }

                        Marshal.Copy(streamBuffer, 0, new IntPtr(buffer.ToInt64() + totalBytesRead), streamBytesRead);

                        totalBytesRead += streamBytesRead;
                        remaining -= streamBytesRead;

                    } while (remaining > 0);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(streamBuffer);
                }

                return HResult.S_OK;
            }
            catch (Exception ex)
            {
                this.CallbackExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                return ex.HResult;
            }
        }

        private unsafe int Write(IntPtr buffer, uint count)
        {
            if (count == 0)
            {
                return HResult.S_OK;
            }

            try
            {
                byte[] streamBuffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);
                try
                {
                    long offset = 0;
                    long remaining = count;

                    do
                    {
                        int copySize = (int)Math.Min(streamBuffer.Length, remaining);

                        Marshal.Copy(new IntPtr(buffer.ToInt64() + offset), streamBuffer, 0, copySize);

                        this.stream.Write(streamBuffer, 0, copySize);

                        offset += copySize;
                        remaining -= copySize;

                    } while (remaining > 0);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(streamBuffer);
                }

                return HResult.S_OK;
            }
            catch (Exception ex)
            {
                this.CallbackExceptionInfo = ExceptionDispatchInfo.Capture(ex);
                return ex.HResult;
            }
        }

        private int Seek(long offset, int origin)
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

        private unsafe int GetSize(long* size)
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
