////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2019 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System.Collections.Generic;
using PaintDotNet.PropertySystem;
using PaintDotNet.IndirectUI;
using System.IO;

namespace DdsFileTypePlus
{
    public sealed class DdsFileType : PropertyBasedFileType
    {
        public DdsFileType() :
            base("DirectDraw Surface (DDS)",
                 FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving | FileTypeFlags.SavesWithProgress,
                 new string[] { ".dds2" })
        {
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                StaticListChoiceProperty.CreateForEnum(PropertyNames.FileFormat, DdsFileFormat.BC1, false),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.BC7CompressionMode, BC7CompressionMode.Normal, false),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.ErrorMetric, DdsErrorMetric.Perceptual, false),
                new BooleanProperty(PropertyNames.CubeMap, false),
                new BooleanProperty(PropertyNames.GenerateMipMaps, false),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.MipMapResamplingAlgorithm, ResamplingAlgorithm.Fant, false)
            };

            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>
            {
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(
                    PropertyNames.BC7CompressionMode,
                    PropertyNames.FileFormat,
                    new object[]
                    {
                        DdsFileFormat.BC6HUnsigned,
                        DdsFileFormat.BC7,
                        DdsFileFormat.BC7Srgb
                    },
                    true),
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(
                    PropertyNames.ErrorMetric,
                    PropertyNames.FileFormat,
                    new object[]
                    {
                        DdsFileFormat.B8G8R8A8,
                        DdsFileFormat.B8G8R8X8,
                        DdsFileFormat.R8G8B8A8,
                        DdsFileFormat.B5G5R5A1,
                        DdsFileFormat.B4G4R4A4,
                        DdsFileFormat.B5G6R5
                    },
                    false),
                new ReadOnlyBoundToBooleanRule(PropertyNames.MipMapResamplingAlgorithm, PropertyNames.GenerateMipMaps, true)
            };

            return new PropertyCollection(props, rules);
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultSaveConfigUI(props);

            PropertyControlInfo formatPCI = configUI.FindControlForPropertyName(PropertyNames.FileFormat);
            formatPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            formatPCI.SetValueDisplayName(DdsFileFormat.BC1, "BC1 (Linear, DXT1)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC1Srgb, "BC1 (sRGB, DX 10+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC2, "BC2 (Linear, DXT3)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC2Srgb, "BC2 (sRGB, DX 10+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC3, "BC3 (Linear, DXT5)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC3Srgb, "BC3 (sRGB, DX 10+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC4Unsigned, "BC4 (Linear, Unsigned, DX 10+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC5Unsigned, "BC5 (Linear, Unsigned, DX 10+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC5Signed, "BC5 (Linear, Signed, DX 10+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC6HUnsigned, "BC6H (Linear, Unsigned, DX 11+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC7, "BC7 (Linear, DX 11+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.BC7Srgb, "BC7 (sRGB, DX 11+)");
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8A8, "B8G8R8A8 (Linear, A8R8G8B8)");
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8X8, "B8G8R8X8 (Linear, X8R8G8B8)");
            formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8A8, "R8G8B8A8 (Linear, A8B8G8R8)");
            formatPCI.SetValueDisplayName(DdsFileFormat.B5G5R5A1, "B5G5R5A1 (Linear, A1R5G5B5)");
            formatPCI.SetValueDisplayName(DdsFileFormat.B4G4R4A4, "B4G4R4A4 (Linear, A4R4G4B4)");
            formatPCI.SetValueDisplayName(DdsFileFormat.B5G6R5, "B5G6R5 (Linear, R5G6B5)");

            PropertyControlInfo compresionModePCI = configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode);
            compresionModePCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = "BC6H / BC7 Compression Mode";
            compresionModePCI.SetValueDisplayName(BC7CompressionMode.Fast, "Fast");
            compresionModePCI.SetValueDisplayName(BC7CompressionMode.Normal, "Normal");
            compresionModePCI.SetValueDisplayName(BC7CompressionMode.Slow, "Slow");

            PropertyControlInfo errorMetricPCI = configUI.FindControlForPropertyName(PropertyNames.ErrorMetric);
            errorMetricPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = "Error Metric";
            errorMetricPCI.ControlType.Value = PropertyControlType.RadioButton;
            errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Perceptual, "Perceptual");
            errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Uniform, "Uniform");

            PropertyControlInfo cubemapPCI = configUI.FindControlForPropertyName(PropertyNames.CubeMap);
            cubemapPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            cubemapPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = "Cube Map from crossed image";

            PropertyControlInfo generateMipPCI = configUI.FindControlForPropertyName(PropertyNames.GenerateMipMaps);
            generateMipPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            generateMipPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = "Generate Mip Maps";

            PropertyControlInfo mipResamplingPCI = configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm);
            mipResamplingPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.NearestNeighbor, "Nearest Neighbor");
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Bicubic, "Bicubic");
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Bilinear, "Bilinear");
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Fant, "Fant");
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.SuperSampling, "Super Sampling");

            return configUI;
        }

        protected override Document OnLoad(Stream input)
        {
            return DdsFile.Load(input);
        }

        protected override void OnSaveT(Document input, Stream output, PropertyBasedSaveConfigToken token, Surface scratchSurface, ProgressEventHandler progressCallback)
        {
            DdsFileFormat fileFormat = (DdsFileFormat)token.GetProperty(PropertyNames.FileFormat).Value;
            BC7CompressionMode compressionMode = (BC7CompressionMode)token.GetProperty(PropertyNames.BC7CompressionMode).Value;
            DdsErrorMetric errorMetric = (DdsErrorMetric)token.GetProperty(PropertyNames.ErrorMetric).Value;
            bool cubeMap = token.GetProperty<BooleanProperty>(PropertyNames.CubeMap).Value;
            bool generateMipmaps = token.GetProperty<BooleanProperty>(PropertyNames.GenerateMipMaps).Value;
            ResamplingAlgorithm mipSampling = (ResamplingAlgorithm)token.GetProperty(PropertyNames.MipMapResamplingAlgorithm).Value;

            DdsFile.Save(input, output, fileFormat, errorMetric, compressionMode, cubeMap, generateMipmaps, mipSampling, scratchSurface, progressCallback);
        }

        protected override bool IsReflexive(PropertyBasedSaveConfigToken token)
        {
            DdsFileFormat format = (DdsFileFormat)token.GetProperty(PropertyNames.FileFormat).Value;

            return format == DdsFileFormat.B8G8R8A8 || format == DdsFileFormat.R8G8B8A8;
        }

        public enum PropertyNames
        {
            FileFormat,
            BC7CompressionMode,
            ErrorMetric,
            CubeMap,
            GenerateMipMaps,
            MipMapResamplingAlgorithm
        }
    }
}
