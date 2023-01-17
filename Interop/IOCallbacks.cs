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

using System;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate int ReadDelegate(IntPtr buffer, uint count, uint* bytesRead);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate int WriteDelegate(IntPtr buffer, uint count, uint* bytesWritten);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate int SeekDelegate(long offset, int origin);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal unsafe delegate int GetSizeDelegate(long* size);

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class IOCallbacks
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
}
