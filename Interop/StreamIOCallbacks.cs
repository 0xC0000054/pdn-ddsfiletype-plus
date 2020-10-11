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

using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    internal sealed class StreamIOCallbacks
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
