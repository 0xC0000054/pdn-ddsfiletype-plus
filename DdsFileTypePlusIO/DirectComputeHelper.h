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

#include <d3d11.h>

class DirectComputeHelper
{
public:
	DirectComputeHelper();
	~DirectComputeHelper();

	DirectComputeHelper(const DirectComputeHelper&) = delete;
	DirectComputeHelper& operator=(const DirectComputeHelper&) = delete;

	bool CreateComputeDevice();
	ID3D11Device* GetComputeDevice() const;
	void Release();

private:	
	HMODULE hModD3D11;
	PFN_D3D11_CREATE_DEVICE dynamicD3D11CreateDevice;
	ID3D11Device* computeDevice;
	bool loadedD3D11;
};
