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
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    internal static partial class DdsIO_ARM64
    {
        private const string DllName = "DdsFileTypePlusIO_ARM64.dll";

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        internal static partial int CreateScratchImage(int width,
                                                       int height,
                                                       DXGI_FORMAT format,
                                                       int arraySize,
                                                       int mipLevels,
                                                       [MarshalAs(UnmanagedType.U1)] bool cubeMap,
                                                       out SafeDirectXTexScratchImage image);

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        internal static partial void DestroyScratchImage(IntPtr handle);

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        internal static partial int GetScratchImageData(SafeDirectXTexScratchImage image,
                                                        nuint mip,
                                                        nuint item,
                                                        nuint slice,
                                                        out DirectXTexScratchImageData data);

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        internal static partial int Load(ref IOCallbacks callbacks,
                                         out DDSLoadInfo info,
                                         out SafeDirectXTexScratchImage image);

        [LibraryImport(DllName)]
        [UnmanagedCallConv(CallConvs = new Type[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        internal static unsafe partial int Save(ref NativeDdsSaveInfo input,
                                                SafeDirectXTexScratchImage image,
                                                ref IOCallbacks callbacks,
                                                IntPtr directComputeAdapter,
                                                [MarshalAs(UnmanagedType.FunctionPtr)] DdsProgressCallback progressCallback);
    }
}
