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

#include "stdafx.h"
#include "DdsFileTypePlusIO.h"
#include "DirectComputeHelper.h"
#include "DDS.h"
#include <memory>

using namespace DirectX;

namespace
{
    SwizzledImageFormat GetSwizzledImageFormat(const TexMetadata& metadata, const DDSMetaData& ddsPixelFormat)
    {
        SwizzledImageFormat format = SwizzledImageFormat::Unknown;

        if (metadata.format == DXGI_FORMAT_BC3_UNORM)
        {
            switch (ddsPixelFormat.fourCC)
            {
            case MAKEFOURCC('R', 'x', 'B', 'G'):
            case MAKEFOURCC('G', 'R', 'X', 'B'):
                format = SwizzledImageFormat::Rxbg;
                break;
            case MAKEFOURCC('R', 'G', 'x', 'B'):
            case MAKEFOURCC('B', 'R', 'G', 'X'):
                format = SwizzledImageFormat::Rgxb;
                break;
            case MAKEFOURCC('R', 'B', 'x', 'G'):
                format = SwizzledImageFormat::Rbxg;
                break;
            case MAKEFOURCC('x', 'G', 'B', 'R'):
            case MAKEFOURCC('R', 'X', 'G', 'B'):
                format = SwizzledImageFormat::Xgbr;
                break;
            case MAKEFOURCC('x', 'R', 'B', 'G'):
            case MAKEFOURCC('G', 'X', 'R', 'B'):
                format = SwizzledImageFormat::Xrbg;
                break;
            case MAKEFOURCC('x', 'G', 'x', 'R'):
                format = SwizzledImageFormat::Xgxr;
                break;
            }
        }

        return format;
    }

    HRESULT SaveImage(const ImageIOCallbacks* callbacks, const ScratchImage* const image)
    {
        TexMetadata metadata = image->GetMetadata();

        if (HasAlpha(metadata.format) && metadata.format != DXGI_FORMAT_A8_UNORM)
        {
            if (image->IsAlphaAllOpaque())
            {
                metadata.SetAlphaMode(TEX_ALPHA_MODE_OPAQUE);
            }
            else if (metadata.GetAlphaMode() == TEX_ALPHA_MODE_UNKNOWN)
            {
                metadata.SetAlphaMode(TEX_ALPHA_MODE_CUSTOM);
            }
        }

        return SaveToDDSIOCallbacks(image->GetImages(), image->GetImageCount(), metadata, DDS_FLAGS_NONE, callbacks);
    }
}

HRESULT __stdcall CreateScratchImage(
    int32_t width,
    int32_t height,
    DXGI_FORMAT format,
    int32_t arraySize,
    int32_t mipLevels,
    bool cubeMap,
    DirectX::ScratchImage** image)
{
    *image = nullptr;

    std::unique_ptr<ScratchImage> scratchImage(new(std::nothrow) ScratchImage);

    if (scratchImage == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    TexMetadata metadata = {};

    metadata.width = width;
    metadata.height = height;
    metadata.depth = 1;
    metadata.arraySize = arraySize;
    metadata.mipLevels = mipLevels;
    metadata.format = format;
    metadata.dimension = TEX_DIMENSION_TEXTURE2D;

    if (cubeMap)
    {
        metadata.miscFlags |= TEX_MISC_TEXTURECUBE;
    }

    HRESULT hr = scratchImage->Initialize(metadata);
    if (FAILED(hr))
    {
        return hr;
    }

    *image = scratchImage.release();
    return S_OK;
}

void __stdcall DestroyScratchImage(DirectX::ScratchImage* image)
{
    if (image)
    {
        delete image;
    }
}

HRESULT __stdcall GetScratchImageData(
    DirectX::ScratchImage* image,
    size_t mip,
    size_t item,
    size_t slice,
    ScratchImageData* data)
{
    if (image == nullptr || data == nullptr)
    {
        return E_INVALIDARG;
    }

    const Image* requestedImage = image->GetImage(mip, item, slice);

    if (requestedImage == nullptr)
    {
        return HRESULT_FROM_WIN32(ERROR_NOT_FOUND);
    }

    data->pixels = requestedImage->pixels;
    data->width = requestedImage->width;
    data->height = requestedImage->height;
    data->stride = requestedImage->rowPitch;
    data->totalImageDataSize = requestedImage->slicePitch;
    data->format = requestedImage->format;

    return S_OK;
}

HRESULT __stdcall Load(
    const ImageIOCallbacks* callbacks,
    DDSLoadInfo* loadInfo,
    DirectX::ScratchImage** image)
{
    if (callbacks == nullptr || loadInfo == nullptr || image == nullptr)
    {
        return E_INVALIDARG;
    }

    *image = nullptr;

    TexMetadata info;
    DDSMetaData ddsPixelFormat{};
    std::unique_ptr<ScratchImage> ddsImage(new(std::nothrow) ScratchImage);

    if (ddsImage == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    HRESULT hr = LoadFromDDSIOCallbacks(callbacks, DDS_FLAGS_ALLOW_LARGE_FILES | DDS_FLAGS_PERMISSIVE, &info, &ddsPixelFormat, *ddsImage);

    if (FAILED(hr))
    {
        return hr;
    }

    if (IsTypeless(info.format))
    {
        info.format = MakeTypelessUNORM(info.format);

        if (IsTypeless(info.format))
        {
            return HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED);
        }

        ddsImage->OverrideFormat(info.format);
    }

    if (IsPlanar(info.format))
    {
        std::unique_ptr<ScratchImage> interleavedImage(new(std::nothrow) ScratchImage);

        if (interleavedImage == nullptr)
        {
            return E_OUTOFMEMORY;
        }

        hr = ConvertToSinglePlane(ddsImage->GetImages(), ddsImage->GetImageCount(), info, *interleavedImage);

        if (FAILED(hr))
        {
            return hr;
        }

        info = interleavedImage->GetMetadata();
        ddsImage.swap(interleavedImage);
    }

    const TexMetadata originalImageMetadata = info;

    const DXGI_FORMAT targetFormat = IsSRGB(info.format) ? DXGI_FORMAT_R8G8B8A8_UNORM_SRGB : DXGI_FORMAT_R8G8B8A8_UNORM;
    std::unique_ptr<ScratchImage> targetImage(new(std::nothrow) ScratchImage);

    if (targetImage == nullptr)
    {
        return E_OUTOFMEMORY;
    }

    if (info.format == targetFormat)
    {
        targetImage.swap(ddsImage);
    }
    else
    {
        if (IsCompressed(info.format))
        {
            hr = Decompress(ddsImage->GetImages(), ddsImage->GetImageCount(), ddsImage->GetMetadata(), targetFormat, *targetImage);
        }
        else
        {
            hr = Convert(ddsImage->GetImages(), ddsImage->GetImageCount(), ddsImage->GetMetadata(), targetFormat,
                TEX_FILTER_DEFAULT, TEX_THRESHOLD_DEFAULT, *targetImage);
        }

        if (FAILED(hr))
        {
            return hr;
        }

        info = targetImage->GetMetadata();
    }

    loadInfo->width = info.width;
    loadInfo->height = info.height;
    loadInfo->depth = info.depth;
    loadInfo->arraySize = info.arraySize;
    loadInfo->mipLevels = info.mipLevels;
    loadInfo->swizzledImageFormat = GetSwizzledImageFormat(originalImageMetadata, ddsPixelFormat);
    loadInfo->cubeMap = info.IsCubemap();
    loadInfo->premultipliedAlpha = HasAlpha(info.format) && info.format != DXGI_FORMAT_A8_UNORM && info.IsPMAlpha();
    loadInfo->volumeMap = info.IsVolumemap();

    *image = targetImage.release();
    return S_OK;
}


HRESULT __stdcall Save(
    const DDSSaveInfo* input,
    const DirectX::ScratchImage* const originalImage,
    const ImageIOCallbacks* callbacks,
    IDXGIAdapter* directComputeAdapter,
    ProgressProc progressFn)
{
    if (input == nullptr || originalImage == nullptr || callbacks == nullptr)
    {
        return E_INVALIDARG;
    }

    HRESULT hr = S_OK;

    const DXGI_FORMAT dxgiFormat = input->format;
    std::unique_ptr<ScratchImage> output;

    std::function<bool __cdecl(size_t, size_t)> progressCallback = nullptr;
    if (progressFn != nullptr)
    {
        progressCallback = [progressFn](size_t done, size_t total) -> bool
        {
            double progress = static_cast<double>(done) / static_cast<double>(total);
            double progressPercentage = progress * 100.0;

            return progressFn(progressPercentage);
        };
    }

    if (IsCompressed(dxgiFormat))
    {
        std::unique_ptr<ScratchImage> compressedImage(new(std::nothrow) ScratchImage);

        if (compressedImage == nullptr)
        {
            return E_OUTOFMEMORY;
        }

        TEX_COMPRESS_FLAGS compressFlags = TEX_COMPRESS_DEFAULT | TEX_COMPRESS_PARALLEL;

        if (input->errorMetric == DdsErrorMetric::Uniform)
        {
            compressFlags |= TEX_COMPRESS_UNIFORM;
        }

        std::unique_ptr<DirectComputeHelper> dcHelper = nullptr;
        bool useDirectCompute = false;

        if (dxgiFormat == DXGI_FORMAT_BC7_UNORM || dxgiFormat == DXGI_FORMAT_BC7_UNORM_SRGB || dxgiFormat == DXGI_FORMAT_BC7_TYPELESS ||
            dxgiFormat == DXGI_FORMAT_BC6H_UF16 || dxgiFormat == DXGI_FORMAT_BC6H_SF16 || dxgiFormat == DXGI_FORMAT_BC6H_TYPELESS)
        {
            switch (input->compressionSpeed)
            {
            case BC7CompressionSpeed::Fast:
                compressFlags |= TEX_COMPRESS_BC7_QUICK;
                break;
            case BC7CompressionSpeed::Slow:
                compressFlags |= TEX_COMPRESS_BC7_USE_3SUBSETS;
                break;
            case BC7CompressionSpeed::Medium:
            default:
                break;
            }

            dcHelper.reset(new(std::nothrow) DirectComputeHelper(directComputeAdapter));
            if (dcHelper != nullptr)
            {
                useDirectCompute = dcHelper->ComputeDeviceAvailable();
            }
        }
        else
        {
            if (input->errorDiffusionDithering)
            {
                compressFlags |= TEX_COMPRESS_DITHER;
            }
        }

        CompressOptions options = {};
        options.flags = compressFlags;
        options.threshold = TEX_THRESHOLD_DEFAULT;
        options.alphaWeight = TEX_ALPHA_WEIGHT_DEFAULT;

        if (useDirectCompute)
        {
            hr = CompressEx(dcHelper->GetComputeDevice(), originalImage->GetImages(), originalImage->GetImageCount(),
                originalImage->GetMetadata(), dxgiFormat, options, *compressedImage, progressCallback);
        }
        else
        {
            hr = CompressEx(originalImage->GetImages(), originalImage->GetImageCount(), originalImage->GetMetadata(),
                dxgiFormat, options, *compressedImage, progressCallback);
        }

        if (FAILED(hr))
        {
            return hr;
        }

        output.swap(compressedImage);
    }
    else if (originalImage->GetMetadata().format != dxgiFormat)
    {
        std::unique_ptr<ScratchImage> convertedImage(new(std::nothrow) ScratchImage);

        if (convertedImage == nullptr)
        {
            return E_OUTOFMEMORY;
        }

        TEX_FILTER_FLAGS filter = TEX_FILTER_DEFAULT | TEX_FILTER_SEPARATE_ALPHA;

        if (input->errorDiffusionDithering)
        {
            filter |= TEX_FILTER_DITHER_DIFFUSION;
        }

        ConvertOptions options = {};
        options.filter = filter;
        options.threshold = TEX_THRESHOLD_DEFAULT;

        hr = ConvertEx(originalImage->GetImages(), originalImage->GetImageCount(), originalImage->GetMetadata(),
            dxgiFormat, options, *convertedImage, progressCallback);

        if (FAILED(hr))
        {
            return hr;
        }

        output.swap(convertedImage);
    }

    return SaveImage(callbacks, output ? output.get() : originalImage);
}
