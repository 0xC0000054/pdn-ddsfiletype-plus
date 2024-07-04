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

namespace DdsFileTypePlus.Interop
{
    internal enum SwizzledImageFormat
    {
        /// <summary>
        /// Not a swizzled format or the format is unsupported. It will be loaded as-is.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The green and alpha channels are swapped. Identical to GRXB.
        /// </summary>
        Rxbg,

        /// <summary>
        /// The blue and alpha channels are swapped. Identical to BRGX.
        /// </summary>
        Rgxb,

        /// <summary>
        /// The blue channel is swapped with green and the green channel is swapped with alpha.
        /// </summary>
        Rbxg,

        /// <summary>
        /// The red and alpha channels are swapped. Identical to RXGB.
        /// </summary>
        Xgbr,

        /// <summary>
        /// The red channel is swapped with green and the green channel is swapped with alpha.
        /// Identical to GXRB.
        /// </summary>
        Xrbg,

        /// <summary>
        /// A 2 channel RGxx format where the red and alpha channels are swapped.
        /// </summary>
        Xgxr
    }
}
