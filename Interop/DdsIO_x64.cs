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
    internal static class DdsIO_x64
    {
        private const string DllName = "DdsFileTypePlusIO_x64.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int CreateScratchImage([In] int width,
                                                      [In] int height,
                                                      [In] DXGI_FORMAT format,
                                                      [In] int arraySize,
                                                      [In] int mipLevels,
                                                      [Out] out SafeDirectXTexScratchImage image);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void DestroyScratchImage([In] IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int GetScratchImageData([In] SafeDirectXTexScratchImage image,
                                                       [In] nuint mip,
                                                       [In] nuint item,
                                                       [In] nuint slice,
                                                       [In, Out] DirectXTexScratchImageData data);

        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int Load([In] IOCallbacks callbacks,
                                        [In, Out] DDSLoadInfo info,
                                        [Out] out SafeDirectXTexScratchImage image);

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
