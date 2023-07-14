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

    // This must be kept in sync with DDSLoadInfo.cs and
    // the Native structure in DDSLoadInfo.Marshaller.cs
    struct DDSLoadInfo
    {
        size_t width;
        size_t height;
        size_t depth;
        size_t arraySize;
        size_t mipLevels;
        bool cubeMap;
        bool premultipliedAlpha;
        bool volumeMap;
    };

    struct DDSSaveInfo
    {
        DXGI_FORMAT format;
        DdsErrorMetric errorMetric;
        BC7CompressionSpeed compressionSpeed;
        bool errorDiffusionDithering;
    };

    // This is based on DirectX::Image.
    struct ScratchImageData
    {
        uint8_t*    pixels;
        size_t      width;
        size_t      height;
        size_t      stride;
        size_t      totalImageDataSize;
        DXGI_FORMAT format;
    };

    __declspec(dllexport) HRESULT __stdcall CreateScratchImage(
        int32_t width,
        int32_t height,
        DXGI_FORMAT format,
        int32_t arraySize,
        int32_t mipLevels,
        bool cubeMap,
        DirectX::ScratchImage** image);

    __declspec(dllexport) void __stdcall DestroyScratchImage(DirectX::ScratchImage* image);

    __declspec(dllexport) HRESULT __stdcall GetScratchImageData(
        DirectX::ScratchImage* image,
        size_t mip,
        size_t item,
        size_t slice,
        ScratchImageData* data);

    __declspec(dllexport) HRESULT __stdcall Load(
        const DirectX::ImageIOCallbacks* callbacks,
        DDSLoadInfo* info,
        DirectX::ScratchImage** image);
    __declspec(dllexport) HRESULT __stdcall Save(
        const DDSSaveInfo* input,
        const DirectX::ScratchImage* const originalImage,
        const DirectX::ImageIOCallbacks* callbacks,
        IDXGIAdapter* directComputeAdapter,
        DirectX::ProgressProc progressFn);

#ifdef __cplusplus
}
#endif // __cplusplus
