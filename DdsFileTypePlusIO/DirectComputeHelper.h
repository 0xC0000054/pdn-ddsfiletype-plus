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

#include <d3d11.h>

class DirectComputeHelper
{
public:
    DirectComputeHelper(IDXGIAdapter* directComputeAdapter);
    ~DirectComputeHelper();

    DirectComputeHelper(const DirectComputeHelper&) = delete;
    DirectComputeHelper& operator=(const DirectComputeHelper&) = delete;

    bool ComputeDeviceAvailable() const;
    ID3D11Device* GetComputeDevice() const;
    void Release();

private:
    HMODULE hModD3D11;
    ID3D11Device* computeDevice;
};
