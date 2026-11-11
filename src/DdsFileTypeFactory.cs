////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2026 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using PaintDotNet.FileTypes;

namespace DdsFileTypePlus
{
    public sealed class DdsFileTypeFactory : IFileTypeFactory
    {
        public IFileType[] CreateFileTypes(IFileTypeHost host)
        {
            return [new DdsFileType(host)];
        }
    }
}
