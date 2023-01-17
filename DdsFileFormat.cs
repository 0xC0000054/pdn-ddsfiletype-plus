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

namespace DdsFileTypePlus
{
    // This must be kept in sync with DdsFileFormat in DdsFileTypePlusIO.h
    public enum DdsFileFormat
    {
        BC1,
        BC1Srgb,
        BC2,
        BC2Srgb,
        BC3,
        BC3Srgb,
        BC4Unsigned,
        BC5Unsigned,
        BC5Signed,
        BC6HUnsigned,
        BC7,
        BC7Srgb,
        B8G8R8A8,
        B8G8R8A8Srgb,
        B8G8R8X8,
        B8G8R8X8Srgb,
        R8G8B8A8,
        R8G8B8A8Srgb,
        R8G8B8X8,
        B5G5R5A1,
        B4G4R4A4,
        B5G6R5,
        B8G8R8,
        R8Unsigned,
        R8G8Unsigned,
        R8G8Signed,
        R32Float
    }
}
