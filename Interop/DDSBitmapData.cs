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
using System;

namespace DdsFileTypePlus.Interop
{
    internal unsafe struct DDSBitmapData
    {
        public byte* scan0;
        public uint width;
        public uint height;
        public uint stride;

        public unsafe DDSBitmapData(Surface surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(nameof(surface));
            }

            this.scan0 = (byte*)surface.Scan0.VoidStar;
            this.width = (uint)surface.Width;
            this.height = (uint)surface.Height;
            this.stride = (uint)surface.Stride;
        }
    }
}
