////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2019 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

#pragma once

#include <stdint.h>
#include "DirectXTex.h"

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

    struct DDSLoadInfo
    {
        void* scan0;
        int32_t width;
        int32_t height;
        int32_t stride;
    };

    enum DdsFileFormat
    {
        // DXT1
        DDS_FORMAT_BC1,
        // BC1 sRGB (DX 10+)
        DDS_FORMAT_BC1_SRGB,
        // DXT3
        DDS_FORMAT_BC2,
        // BC2 sRGB (DX 10+)
        DDS_FORMAT_BC2_SRGB,
        // DXT5
        DDS_FORMAT_BC3,
        // BC3 sRGB (DX 10+)
        DDS_FORMAT_BC3_SRGB,
        // BC4 (DX 10+)
        DDS_FORMAT_BC4,
        // BC5 (DX 10+)
        DDS_FORMAT_BC5,
        // BC6H (DX 11+)
        DDS_FORMAT_BC6H,
        // BC7 (DX 11+)
        DDS_FORMAT_BC7,
        // BC7 sRGB (DX 11+)
        DDS_FORMAT_BC7_SRGB,
        DDS_FORMAT_B8G8R8A8,
        DDS_FORMAT_B8G8R8X8,
        DDS_FORMAT_R8G8B8A8,
        DDS_FORMAT_B5G5R5A1,
        DDS_FORMAT_B4G4R4A4,
        DDS_FORMAT_B5G6R5
    };

    enum DdsErrorMetric
    {
        DDS_ERROR_METRIC_PERCEPTUAL,
        DDS_ERROR_METRIC_UNIFORM
    };

    enum BC7CompressionMode
    {
        BC7_COMPRESSION_MODE_FAST,
        BC7_COMPRESSION_MODE_NORMAL,
        BC7_COMPRESSION_MODE_SLOW
    };

    struct DDSSaveInfo
    {
        int32_t width;
        int32_t height;
        int32_t arraySize;
        int32_t mipLevels;
        DdsFileFormat format;
        DdsErrorMetric errorMetric;
        BC7CompressionMode compressionMode;
        bool cubeMap;
    };

    struct DDSBitmapData
    {
        uint8_t* scan0;
        uint32_t width;
        uint32_t height;
        uint32_t stride;
    };

    __declspec(dllexport) HRESULT __stdcall Load(const DirectX::ImageIOCallbacks* callbacks, DDSLoadInfo* info);
    __declspec(dllexport) void __stdcall FreeLoadInfo(DDSLoadInfo* info);
    __declspec(dllexport) HRESULT __stdcall Save(
        const DDSSaveInfo* input,
        const DDSBitmapData* imageData,
        const uint32_t imageDataLength,
        const DirectX::ImageIOCallbacks* callbacks,
        DirectX::ProgressProc progressFn);

#ifdef __cplusplus
}
#endif // __cplusplus
