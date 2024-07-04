////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2024 Nicholas Hayes
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
        public const int E_ABORT = unchecked((int)0x80004004);
        public const int SeekError = unchecked((int)0x80070019); // HRESULT_FROM_WIN32(ERROR_SEEK)
        public const int EndOfFile = unchecked((int)0x80070026); // HRESULT_FROM_WIN32(ERROR_HANDLE_EOF)
        public const int InvalidArgument = unchecked((int)0x80070057); // HRESULT_FROM_WIN32(ERROR_INVALID_PARAMETER)
        public const int NotFound = unchecked((int)0x80070490); // HRESULT_FROM_WIN32(ERROR_NOT_FOUND)
        public const int OutOfMemory = unchecked((int)0x8007000E); // HRESULT_FROM_WIN32(ERROR_OUTOFMEMORY)
        public const int NotSupported = unchecked((int)0x80070032); // HRESULT_FROM_WIN32(ERROR_NOT_SUPPORTED)
        public const int InvalidData = unchecked((int)0x8007000D); // HRESULT_FROM_WIN32(ERROR_INVALID_DATA)
        public const int InvalidDdsFileSignature = unchecked((int)0x8007000B); // HRESULT_FROM_WIN32(ERROR_BAD_FORMAT)

        public static bool Failed(int hr) => hr < 0;
    }
}
