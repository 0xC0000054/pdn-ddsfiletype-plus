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

using System.Runtime.InteropServices.Marshalling;

namespace DdsFileTypePlus.Interop
{
    // This must be kept in sync with the Native structure in DDSLoadInfo.Marshaller.cs
    // and the DDSLoadInfo type in DdsFileTypePlusIO.h
    [NativeMarshalling(typeof(Marshaller))]
    internal sealed partial class DDSLoadInfo
    {
        public nuint width;
        public nuint height;
        public nuint depth;
        public nuint arraySize;
        public nuint mipLevels;
        public bool cubeMap;
        public bool premultipliedAlpha;
        public bool volumeMap;

        public bool IsTextureArray
            => this.cubeMap ? this.arraySize > 6 : this.arraySize > 1;
    }
}
