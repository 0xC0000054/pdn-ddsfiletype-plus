////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2020 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;
using System.Reflection;

namespace DdsFileTypePlus
{
    public sealed class PluginSupportInfo : IPluginSupportInfo
    {
        public string DisplayName => "DDS FileType Plus";

        public string Author => "null54";

        public string Copyright
        {
            get
            {
                object[] attributes = typeof(PluginSupportInfo).Assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);

                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public Version Version => typeof(PluginSupportInfo).Assembly.GetName().Version;

        public Uri WebsiteUri => new Uri(@"https://forums.getpaint.net/topic/111731-dds-filetype-plus");
    }
}
