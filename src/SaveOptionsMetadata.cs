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
using System;
using System.Diagnostics.CodeAnalysis;

namespace DdsFileTypePlus
{
    public sealed class SaveOptionsMetadata
    {
        private const string FormatName = $"{nameof(DdsFileType)}.{nameof(Format)}";
        private const string GenerateMipMapsName = $"{nameof(DdsFileType)}.{nameof(GenerateMipMaps)}";
        private const string CubeMapName = $"{nameof(DdsFileType)}.{nameof(CubeMap)}";

        public DdsFileFormat? Format
        {
            get;
            init;
        }

        public bool? GenerateMipMaps
        {
            get;
            init;
        }

        public bool? CubeMap
        {
            get;
            init;
        }

        public SaveOptionsMetadata()
        {
        }

        public void Save(IFileTypePropertyBag propertyBag)
        {
            if (this.Format.HasValue)
            {
                propertyBag.SetItem(FormatName, this.Format.Value);
            }

            if (this.GenerateMipMaps.HasValue)
            {
                propertyBag.SetItem(GenerateMipMapsName, this.GenerateMipMaps.Value);
            }

            if (this.CubeMap.HasValue)
            {
                propertyBag.SetItem(CubeMapName, this.CubeMap.Value);
            }
        }

        public static bool TryLoad(IReadOnlyFileTypePropertyBag propertyBag, [NotNullWhen(true)] out SaveOptionsMetadata? metadata)
        {
            DdsFileFormat? format = propertyBag.TryGetValue(FormatName, out DdsFileFormat formatValue) ? formatValue : null;
            bool? generateMipMaps = propertyBag.TryGetValue(GenerateMipMapsName, out bool generateMipMapsValue) ? generateMipMapsValue : null;
            bool? cubeMap = propertyBag.TryGetValue(CubeMapName, out bool cubeMapValue) ? cubeMapValue : null;

            if (!format.HasValue && !generateMipMaps.HasValue && !cubeMap.HasValue)
            {
                metadata = null;
                return false;
            }

            metadata = new SaveOptionsMetadata()
            {
                Format = format,
                GenerateMipMaps = generateMipMaps,
                CubeMap = cubeMap
            };

            return true;
        }
    }
}
