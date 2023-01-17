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
    internal static class DdsIO_ARM64
    {
        private const string DllName = "DdsFileTypePlusIO_ARM64.dll";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int Load([In] IOCallbacks callbacks, [In, Out] DDSLoadInfo info);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void FreeLoadInfo([In, Out] DDSLoadInfo info);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern unsafe int Save(
            [In] DDSSaveInfo input,
            [In] DDSBitmapData* bitmapData,
            [In] uint bitmapDataLength,
            [In] IOCallbacks callbacks,
            [In] IntPtr directComputeAdapter,
            [In, MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
    }
}
