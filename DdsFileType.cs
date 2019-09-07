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
using PaintDotNet.Dds;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;
using System.IO;

namespace DdsFileTypePlus
{
    public sealed class DdsFileType : PropertyBasedFileType
    {
        private readonly IServiceProvider services;
        private readonly IDdsFileTypePlusStrings strings;

        public DdsFileType(IServiceProvider services) :
            base(GetString(services.GetService<IDdsFileTypePlusStrings>(), DdsFileTypePlusStringName.FileType_Name),
                 new FileTypeOptions()
                 {
                     LoadExtensions = new string[] { ".dds" },
                     SaveExtensions = new string[] { ".dds" }
                 })
        {
            this.services = services;
            this.strings = services.GetService<IDdsFileTypePlusStrings>();
        }

        private string GetString(DdsFileTypePlusStringName name)
        {
            return GetString(this.strings, name);
        }

        private static string GetString(IDdsFileTypePlusStrings strings, DdsFileTypePlusStringName name)
        {
            string value = strings?.TryGetString(name);
            if (value != null)
            {
                return value;
            }

            switch (name)
            {
                case DdsFileTypePlusStringName.FileType_Name:
                    return "DirectDraw Surface (DDS)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC1:
                    return "BC1 (Linear, DXT1)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC1Srgb:
                    return "BC1 (sRGB, DX 10+)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC2:
                    return "BC2 (Linear, DXT3)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC2Srgb:
                    return "BC2 (sRGB, DX 10+)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC3:
                    return "BC3 (Linear, DXT5)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC3Srgb:
                    return "BC3 (sRGB, DX 10+)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC4Unsigned:
                    return "BC4 (Linear, Unsigned)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC5Unsigned:
                    return "BC5 (Linear, Unsigned)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC5Signed:
                    return "BC5 (Linear, Signed)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC6HUnsigned:
                    return "BC6H (Linear, Unsigned, DX 11+)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC7:
                    return "BC7 (Linear, DX 11+)";

                case DdsFileTypePlusStringName.DdsFileFormat_BC7Srgb:
                    return "BC7 (sRGB, DX 11+)";

                case DdsFileTypePlusStringName.DdsFileFormat_B8G8R8A8:
                    return "B8G8R8A8 (Linear, A8R8G8B8)";

                case DdsFileTypePlusStringName.DdsFileFormat_B8G8R8X8:
                    return "B8G8R8X8 (Linear, X8R8G8B8)";

                case DdsFileTypePlusStringName.DdsFileFormat_R8G8B8A8:
                    return "R8G8B8A8 (Linear, A8B8G8R8)";

                case DdsFileTypePlusStringName.DdsFileFormat_B5G5R5A1:
                    return "B5G5R5A1 (Linear, A1R5G5B5)";

                case DdsFileTypePlusStringName.DdsFileFormat_B4G4R4A4:
                    return "B4G4R4A4 (Linear, A4R4G4B4)";

                case DdsFileTypePlusStringName.DdsFileFormat_B5G6R5:
                    return "B5G6R5 (Linear, R5G6B5)";

                case DdsFileTypePlusStringName.BC7CompressionMode_DisplayName:
                    return "BC6H / BC7 Compression Mode";

                case DdsFileTypePlusStringName.BC7CompressionMode_Fast:
                    return "Fast";

                case DdsFileTypePlusStringName.BC7CompressionMode_Normal:
                    return "Normal";

                case DdsFileTypePlusStringName.BC7CompressionMode_Slow:
                    return "Slow";

                case DdsFileTypePlusStringName.ErrorMetric_DisplayName:
                    return "Error Metric";

                case DdsFileTypePlusStringName.ErrorMetric_Perceptual:
                    return "Perceptual";

                case DdsFileTypePlusStringName.ErrorMetric_Uniform:
                    return "Uniform";

                case DdsFileTypePlusStringName.CubeMap_Description:
                    return "Cube Map from crossed image";

                case DdsFileTypePlusStringName.GenerateMipMaps_Description:
                    return "Generate Mip Maps";

                case DdsFileTypePlusStringName.ResamplingAlgorithm_NearestNeighbor:
                    return "Nearest Neighbor";

                case DdsFileTypePlusStringName.ResamplingAlgorithm_Bicubic:
                    return "Bicubic";

                case DdsFileTypePlusStringName.ResamplingAlgorithm_Bilinear:
                    return "Bilinear";

                case DdsFileTypePlusStringName.ResamplingAlgorithm_Fant:
                    return "Fant";

                case DdsFileTypePlusStringName.ResamplingAlgorithm_SuperSampling:
                    return "Super Sampling";
                case DdsFileTypePlusStringName.ForumLink_DisplayName:
                    return "More Info";
                case DdsFileTypePlusStringName.ForumLink_Description:
                    return "Forum Discussion";

                default:
                    throw ExceptionUtil.InvalidEnumArgumentException(name, nameof(name));
            }
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
                StaticListChoiceProperty.CreateForEnum(PropertyNames.MipMapResamplingAlgorithm, ResamplingAlgorithm.SuperSampling, false),
                new UriProperty(PropertyNames.ForumLink, new Uri("https://forums.getpaint.net/topic/111731-dds-filetype-plus")),
                new UriProperty(PropertyNames.GitHubLink, new Uri("https://github.com/0xC0000054/pdn-ddsfiletype-plus"))
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
            formatPCI.SetValueDisplayName(DdsFileFormat.BC1, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC1));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC1Srgb, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC1Srgb));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC2, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC2));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC2Srgb, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC2Srgb));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC3, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC3));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC3Srgb, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC3Srgb));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC4Unsigned, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC4Unsigned));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC5Unsigned, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC5Unsigned));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC5Signed, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC5Signed));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC6HUnsigned, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC6HUnsigned));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC7, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC7));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC7Srgb, GetString(DdsFileTypePlusStringName.DdsFileFormat_BC7Srgb));
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8A8, GetString(DdsFileTypePlusStringName.DdsFileFormat_B8G8R8A8));
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8X8, GetString(DdsFileTypePlusStringName.DdsFileFormat_B8G8R8X8));
            formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8A8, GetString(DdsFileTypePlusStringName.DdsFileFormat_R8G8B8A8));
            formatPCI.SetValueDisplayName(DdsFileFormat.B5G5R5A1, GetString(DdsFileTypePlusStringName.DdsFileFormat_B5G5R5A1));
            formatPCI.SetValueDisplayName(DdsFileFormat.B4G4R4A4, GetString(DdsFileTypePlusStringName.DdsFileFormat_B4G4R4A4));
            formatPCI.SetValueDisplayName(DdsFileFormat.B5G6R5, GetString(DdsFileTypePlusStringName.DdsFileFormat_B5G6R5));

            PropertyControlInfo compresionModePCI = configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode);
            compresionModePCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = GetString(DdsFileTypePlusStringName.BC7CompressionMode_DisplayName);
            compresionModePCI.SetValueDisplayName(BC7CompressionMode.Fast, GetString(DdsFileTypePlusStringName.BC7CompressionMode_Fast));
            compresionModePCI.SetValueDisplayName(BC7CompressionMode.Normal, GetString(DdsFileTypePlusStringName.BC7CompressionMode_Normal));
            compresionModePCI.SetValueDisplayName(BC7CompressionMode.Slow, GetString(DdsFileTypePlusStringName.BC7CompressionMode_Slow));

            PropertyControlInfo errorMetricPCI = configUI.FindControlForPropertyName(PropertyNames.ErrorMetric);
            errorMetricPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = GetString(DdsFileTypePlusStringName.ErrorMetric_DisplayName);
            errorMetricPCI.ControlType.Value = PropertyControlType.RadioButton;
            errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Perceptual, GetString(DdsFileTypePlusStringName.ErrorMetric_Perceptual));
            errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Uniform, GetString(DdsFileTypePlusStringName.ErrorMetric_Uniform));

            PropertyControlInfo cubemapPCI = configUI.FindControlForPropertyName(PropertyNames.CubeMap);
            cubemapPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            cubemapPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = GetString(DdsFileTypePlusStringName.CubeMap_Description);

            PropertyControlInfo generateMipPCI = configUI.FindControlForPropertyName(PropertyNames.GenerateMipMaps);
            generateMipPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            generateMipPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = GetString(DdsFileTypePlusStringName.GenerateMipMaps_Description);

            PropertyControlInfo mipResamplingPCI = configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm);
            mipResamplingPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.NearestNeighbor, GetString(DdsFileTypePlusStringName.ResamplingAlgorithm_NearestNeighbor));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Bicubic, GetString(DdsFileTypePlusStringName.ResamplingAlgorithm_Bicubic));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Bilinear, GetString(DdsFileTypePlusStringName.ResamplingAlgorithm_Bilinear));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Fant, GetString(DdsFileTypePlusStringName.ResamplingAlgorithm_Fant));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.SuperSampling, GetString(DdsFileTypePlusStringName.ResamplingAlgorithm_SuperSampling));

            PropertyControlInfo forumLinkPCI = configUI.FindControlForPropertyName(PropertyNames.ForumLink);
            forumLinkPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = GetString(DdsFileTypePlusStringName.ForumLink_DisplayName);
            forumLinkPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = GetString(DdsFileTypePlusStringName.ForumLink_Description);

            PropertyControlInfo githubLinkPCI = configUI.FindControlForPropertyName(PropertyNames.GitHubLink);
            githubLinkPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            githubLinkPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = "GitHub"; // GitHub is a brand name that should not be localized.

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

            DdsFile.Save(this.services, input, output, fileFormat, errorMetric, compressionMode, cubeMap, generateMipmaps, mipSampling, scratchSurface, progressCallback);
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
            MipMapResamplingAlgorithm,
            ForumLink,
            GitHubLink
        }
    }
}
