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

using PaintDotNet;

namespace DdsFileTypePlus
{
    public sealed class DdsFileTypeFactory : IFileTypeFactory2
    {
        public FileType[] GetFileTypeInstances(IFileTypeHost host)
        {
            return new FileType[] { new DdsFileType(host.Services) };
        }
    }
}
