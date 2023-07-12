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

using PaintDotNet;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe sealed class DirectXTexScratchImageData
    {
        public byte* pixels;
        public nuint width;
        public nuint height;
        public nuint stride;
        public nuint totalImageDataSize;
        public DXGI_FORMAT format;

        public unsafe RegionPtr<T> AsRegionPtr<T>() where T : unmanaged
            => new((T*)this.pixels,
                   checked((int)this.width),
                   checked((int)this.height),
                   checked((int)this.stride));
    }
}
