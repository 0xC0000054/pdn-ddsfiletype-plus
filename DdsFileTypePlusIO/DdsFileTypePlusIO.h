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

#pragma once

#include <stdint.h>
#include <d3d11.h>

// Suppress the C26812 'The enum type 'x' is unscoped.Prefer 'enum class' over 'enum' (Enum.3)'
// warning for the DirectXTex headers.
#pragma warning(push)
#pragma warning(disable: 26812)

#include "DirectXTex.h"

#pragma warning(pop)

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

    // This must be kept in sync with DdsFileFormat.cs
    enum class DdsFileFormat : int32_t
    {
        // DXT1
        BC1,
        // BC1 sRGB (DX 10+)
        BC1_SRGB,
        // DXT3
        BC2,
        // BC2 sRGB (DX 10+)
        BC2_SRGB,
        // DXT5
        BC3,
        // BC3 sRGB (DX 10+)
        BC3_SRGB,
        // BC4 Unsigned
        BC4_UNORM,
        // BC5 Unsigned
        BC5_UNORM,
        // BC5 Signed
        BC5_SNORM,
        // BC6H Unsigned (DX 11+)
        BC6H_UF16,
        // BC7 (DX 11+)
        BC7,
        // BC7 sRGB (DX 11+)
        BC7_SRGB,
        B8G8R8A8,
        B8G8R8A8_SRGB,
        B8G8R8X8,
        B8G8R8X8_SRGB,
        R8G8B8A8,
        R8G8B8A8_SRGB,
        R8G8B8X8, // Not supported by DirectX 10+, but included for documentation.
        B5G5R5A1,
        B4G4R4A4,
        B5G6R5,
        B8G8R8, // Not supported by DirectX 10+, but included for documentation.
        R8_UNORM,
        R8G8_UNORM,
        R8G8_SNORM,
        R32_FLOAT,
    };

    // This must be kept in sync with DdsErrorMetric.cs
    enum class DdsErrorMetric : int32_t
    {
        Perceptual,
        Uniform
    };

    // This must be kept in sync with BC7CompressionSpeed.cs
    enum class BC7CompressionSpeed : int32_t
    {
        Fast,
        Medium,
        Slow
    };

    struct DDSSaveInfo
    {
        int32_t width;
        int32_t height;
        int32_t arraySize;
        int32_t mipLevels;
        DdsFileFormat format;
        DdsErrorMetric errorMetric;
        BC7CompressionSpeed compressionSpeed;
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
        IDXGIAdapter* directComputeAdapter,
        DirectX::ProgressProc progressFn);

#ifdef __cplusplus
}
#endif // __cplusplus
