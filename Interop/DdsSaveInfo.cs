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
    internal sealed class DDSSaveInfo
    {
        public int width;
        public int height;
        public int arraySize;
        public int mipLevels;
        public DdsFileFormat format;
        public DdsErrorMetric errorMetric;
        public BC7CompressionSpeed compressionSpeed;
        [MarshalAs(UnmanagedType.U1)]
        public bool cubeMap;
    }
}
