////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

#pragma once

#include "targetver.h"

#include <Windows.h>
#include "DirectXTex.h"

#ifdef __cplusplus
extern "C" {
#endif // __cplusplus

	typedef void (__stdcall *WriteImageFn)(const void* image, const size_t imageSize);

	struct DDSLoadInfo
	{
		int width;
		int height;
		int stride;
		void* scan0;
	};

	enum DdsFileFormat
	{
		// DXT1
		DDS_FORMAT_BC1,
		// DXT3
		DDS_FORMAT_BC2,
		// DXT5
		DDS_FORMAT_BC3,
		// BC4 (DX 10+)
		DDS_FORMAT_BC4,
		// BC5 (DX 10+)
		DDS_FORMAT_BC5,
		// BC6H (DX 11+)
		DDS_FORMAT_BC6H,
		// BC7 (DX 11+)
		DDS_FORMAT_BC7,
		DDS_FORMAT_A8R8G8B8,
		DDS_FORMAT_X8R8G8B8,
		DDS_FORMAT_A1R5G5B5,
		DDS_FORMAT_R5G6B5
	};

	enum DdsErrorMetric
	{
		DDS_ERROR_METRIC_PERCEPTUAL,
		DDS_ERROR_METRIC_UNIFORM
	};

	enum BC7CompressionMode
	{
		BC7_COMPRESSION_MODE_QUICK,
		BC7_COMPRESSION_MODE_NORMAL,
		BC7_COMPRESSION_MODE_MAX
	};

	enum MipmapSampling
	{
		DDS_MIPMAP_SAMPLING_NEAREST_NEIGHBOR,
		DDS_MIPMAP_SAMPLING_BLINEAR,
		DDS_MIPMAP_SAMPLING_BICUBIC,
		DDS_MIPMAP_SAMPLING_FANT
	};

	struct DDSSaveInfo
	{
		int width;
		int height;
		int stride;
		DdsFileFormat format;
		DdsErrorMetric errorMetric;
		BC7CompressionMode compressionMode;
		bool generateMipmaps;
		MipmapSampling mipmapSampling;
		void* scan0;
	};

	__declspec(dllexport) HRESULT __stdcall Load(const BYTE* input, const size_t inputSize, DDSLoadInfo* info);
	__declspec(dllexport) void __stdcall FreeLoadInfo(DDSLoadInfo* info);
	__declspec(dllexport) HRESULT __stdcall Save(const DDSSaveInfo* input, const WriteImageFn writeFn, DirectX::CompressProgressProc progressFn);

#ifdef __cplusplus
}
#endif // __cplusplus