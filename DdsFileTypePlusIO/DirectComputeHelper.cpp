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

#include "stdafx.h"
#include "DirectComputeHelper.h"

DirectComputeHelper::DirectComputeHelper()
{
    hModD3D11 = LoadLibraryW(L"d3d11.dll");
    computeDevice = nullptr;

    if (hModD3D11 != nullptr)
    {
        PFN_D3D11_CREATE_DEVICE dynamicD3D11CreateDevice = reinterpret_cast<PFN_D3D11_CREATE_DEVICE>(GetProcAddress(hModD3D11, "D3D11CreateDevice"));

        if (dynamicD3D11CreateDevice != nullptr)
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

                    if (FAILED(hr) || !hwopts.ComputeShaders_Plus_RawAndStructuredBuffers_Via_Shader_4_x)
                    {
                        computeDevice->Release();
                        computeDevice = nullptr;
                    }
                }
            }
            else
            {
                computeDevice = nullptr;
            }
        }
    }
}

DirectComputeHelper::~DirectComputeHelper()
{
    Release();
}

bool DirectComputeHelper::ComputeDeviceAvailable() const
{
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
}
