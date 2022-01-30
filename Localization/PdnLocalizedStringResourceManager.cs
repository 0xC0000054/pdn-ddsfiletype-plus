////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2021 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using PaintDotNet.Dds;
using System;
using System.Linq;
using System.Collections.Generic;
using DdsFileTypePlus.Properties;

namespace DdsFileTypePlus
{
    internal sealed class PdnLocalizedStringResourceManager : IDdsStringResourceManager
    {
        private readonly IDdsFileTypePlusStrings strings;
        private static readonly IReadOnlyDictionary<string, DdsFileTypePlusStringName> pdnLocalizedStringMap;

        static PdnLocalizedStringResourceManager()
        {
            // Use a dictionary to map the resource name to its enumeration value.
            // This avoids repeated calls to Enum.TryParse.
            // Adapted from https://stackoverflow.com/a/13677446
            pdnLocalizedStringMap = Enum.GetValues<DdsFileTypePlusStringName>()
                                        .ToDictionary(kv => kv.ToString(), kv => kv, StringComparer.OrdinalIgnoreCase);
        }

        public PdnLocalizedStringResourceManager(IDdsFileTypePlusStrings strings)
        {
            this.strings = strings;
        }

        public string GetString(string name)
        {
            if (pdnLocalizedStringMap.TryGetValue(name, out DdsFileTypePlusStringName value))
            {
                return this.strings?.TryGetString(value) ?? Resources.ResourceManager.GetString(name);
            }
            else
            {
                return Resources.ResourceManager.GetString(name);
            }
        }
    }
}
