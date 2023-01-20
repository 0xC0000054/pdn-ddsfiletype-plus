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

namespace DdsFileTypePlus.Interop
{
    internal static class HResult
    {
        public const int S_OK = 0;
        public const int E_POINTER = unchecked((int)0x80004003);
        public const int CanceledError = unchecked((int)0x800704C7); // HRESULT_FROM_WIN32(ERROR_CANCELLED)
        public const int SeekError = unchecked((int)0x80070019); // HRESULT_FROM_WIN32(ERROR_SEEK)
        public const int NotSupported = unchecked((int)0x80070032); // HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED)
        public const int InvalidData = unchecked((int)0x8007000D); // HRESULT_FROM_WIN32(ERROR_INVALID_DATA)
        public const int InvalidDdsFileSignature = unchecked((int)0x8007000B); // HRESULT_FROM_WIN32(ERROR_BAD_FORMAT)
        public const int UnknownDdsSaveFormat = unchecked((int)0xA00707D0);
    }
}
