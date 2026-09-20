////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2026 Nicholas Hayes
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
        public nuint Width { get; init; }

        public nuint Height { get; init; }

        public nuint Depth { get; init; }

        public nuint ArraySize { get; init; }

        /// <summary>
        /// The mip levels that were loaded.
        /// </summary>
        public nuint MipLevelsLoaded { get; init; }

        /// <summary>
        /// The mip level count from the file header, this may be larger than <see cref="MipLevelsLoaded"/>.
        /// </summary>
        public nuint MipLevelsActual { get; init; }

        public DXGI_FORMAT Format { get; init; }

        public SwizzledImageFormat SwizzledImageFormat { get; init; }

        public bool CubeMap { get; init; }

        public bool PremultipliedAlpha { get; init; }

        public bool VolumeMap { get; init; }

        public bool IsTextureArray
            => this.CubeMap ? this.ArraySize > 6 : this.ArraySize > 1;
    }
}
