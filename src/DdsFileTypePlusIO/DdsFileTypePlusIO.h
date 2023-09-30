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

    // This must be kept in sync with DdsFileOptions.cs
    enum class DdsFileOptions : int32_t
    {
        None = 0,
        ForceLegacyDX9Formats,
        ForceBC3ToRXGB
    };

    // This must be kept in sync with BC7CompressionSpeed.cs
    enum class BC7CompressionSpeed : int32_t
    {
        Fast,
        Medium,
        Slow
    };

    enum class SwizzledImageFormat : int32_t
    {
        // Not a swizzled format or the format is unsupported. It will be loaded as-is.
        Unknown = 0,

        // The green and alpha channels are swapped. Identical to GRXB.
        Rxbg,

        // The blue and alpha channels are swapped. Identical to BRGX.
        Rgxb,

        // The blue channel is swapped with green and the green channel is swapped with alpha.
        Rbxg,

        // The red and alpha channels are swapped. Identical to RXGB.
        Xgbr,

        // The red channel is swapped with green and the green channel is swapped with alpha.
        // Identical to GXRB.
        Xrbg,

        // A 2 channel RGxx format where the red and alpha channels are swapped.
        Xgxr
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
        SwizzledImageFormat swizzledImageFormat;
        bool cubeMap;
        bool premultipliedAlpha;
        bool volumeMap;
    };

    // This must be kept in sync with DdsSaveInfo.cs and NativeDdsSaveInfo.cs
    struct DDSSaveInfo
    {
        DXGI_FORMAT format;
        DdsFileOptions fileOptions;
        DdsErrorMetric errorMetric;
        BC7CompressionSpeed compressionSpeed;
        bool errorDiffusionDithering;
    };

    // This is based on DirectX::Image.
    // It must be kept in sync with DirectXTexScratchImageData.cs and
    // the Native structure in DirectXTexScratchImageData.Marshaller.cs
    struct ScratchImageData
    {
        uint8_t*    pixels;
        size_t      width;
        size_t      height;
        size_t      stride;
        size_t      totalImageDataSize;
        DXGI_FORMAT format;
    };

    typedef bool(__stdcall *ProgressProc)(double progressPercentage);

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
        ProgressProc progressFn);

#ifdef __cplusplus
}
#endif // __cplusplus
