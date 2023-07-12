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

using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DDSLoadInfo
    {
        public nuint width;
        public nuint height;
        [MarshalAs(UnmanagedType.U1)]
        public bool cubeMap;
        [MarshalAs(UnmanagedType.U1)]
        public bool premultipliedAlpha;
    }
}
