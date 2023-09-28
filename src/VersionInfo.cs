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

using System;

namespace DdsFileTypePlus
{
    internal static class VersionInfo
    {
        private static readonly Lazy<string> pluginVersion = new(GetPluginVersion);

        public static string PluginVersion => pluginVersion.Value;

        private static string GetPluginVersion()
            => typeof(VersionInfo).Assembly.GetName().Version.ToString();
    }
}
