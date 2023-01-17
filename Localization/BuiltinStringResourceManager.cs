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

using DdsFileTypePlus.Properties;

namespace DdsFileTypePlus
{
    internal sealed class BuiltinStringResourceManager : IDdsStringResourceManager
    {
        public string GetString(string name)
        {
            return Resources.ResourceManager.GetString(name);
        }
    }
}
