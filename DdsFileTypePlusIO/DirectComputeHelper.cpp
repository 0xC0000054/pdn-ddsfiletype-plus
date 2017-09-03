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

#include "DirectComputeHelper.h"

DirectComputeHelper::DirectComputeHelper() : hModD3D11(nullptr), dynamicD3D11CreateDevice(nullptr), loadedD3D11(false), computeDevice(nullptr)
{
}

DirectComputeHelper::~DirectComputeHelper()
{
	Release();
}

bool DirectComputeHelper::CreateComputeDevice()
{
	if (!loadedD3D11)
	{
		loadedD3D11 = true;

		hModD3D11 = LoadLibrary(L"d3d11.dll");
		if (hModD3D11 == nullptr)
		{
			return false;
		}

		dynamicD3D11CreateDevice = reinterpret_cast<PFN_D3D11_CREATE_DEVICE>(GetProcAddress(hModD3D11, "D3D11CreateDevice"));
	}

	if (dynamicD3D11CreateDevice == nullptr)
	{
		return false;
	}

	if (computeDevice == nullptr)
	{
		const D3D_FEATURE_LEVEL featureLevels[] =
		{
			D3D_FEATURE_LEVEL_11_0,
			D3D_FEATURE_LEVEL_10_1,
			D3D_FEATURE_LEVEL_10_0
		};

		UINT createDeviceFlags = 0;
#ifdef _DEBUG
		createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif
		D3D_FEATURE_LEVEL featureLevelOut;

		HRESULT hr = dynamicD3D11CreateDevice(nullptr, D3D_DRIVER_TYPE_HARDWARE, nullptr, createDeviceFlags, featureLevels, _countof(featureLevels),
			D3D11_SDK_VERSION, &computeDevice, &featureLevelOut, nullptr);

		if (SUCCEEDED(hr))
		{
			if (featureLevelOut < D3D_FEATURE_LEVEL_11_0)
			{
				D3D11_FEATURE_DATA_D3D10_X_HARDWARE_OPTIONS hwopts;
				hr = computeDevice->CheckFeatureSupport(D3D11_FEATURE_D3D10_X_HARDWARE_OPTIONS, &hwopts, sizeof(hwopts));

				if (SUCCEEDED(hr))
				{
					if (!hwopts.ComputeShaders_Plus_RawAndStructuredBuffers_Via_Shader_4_x)
					{
						hr = CO_E_NOT_SUPPORTED;
					}
				}
			}
		}

		if (FAILED(hr) && computeDevice != nullptr)
		{
			computeDevice->Release();
			computeDevice = nullptr;
		}
	}

	return computeDevice != nullptr;
}

ID3D11Device* DirectComputeHelper::GetComputeDevice() const
{
	return computeDevice;
}

void DirectComputeHelper::Release()
{
	if (computeDevice != nullptr)
	{
		computeDevice->Release();
		computeDevice = nullptr;
	}

	if (hModD3D11 != nullptr)
	{
		FreeLibrary(hModD3D11);
		hModD3D11 = nullptr;
	}
	dynamicD3D11CreateDevice = nullptr;
	loadedD3D11 = false;
}
