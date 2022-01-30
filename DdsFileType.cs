////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2022 Nicholas Hayes
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
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    public sealed class DdsFileType : PropertyBasedFileType
    {
        private readonly IServiceProvider services;
        private readonly IDdsStringResourceManager strings;

        public DdsFileType(IServiceProvider services) :
            base(GetFileTypeName(services.GetService<IDdsFileTypePlusStrings>()),
                 new FileTypeOptions()
                 {
                     LoadExtensions = new string[] { ".dds" },
                     SaveExtensions = new string[] { ".dds" }
                 })
        {
            this.services = services;
            IDdsFileTypePlusStrings ddsFileTypePlusStrings = services.GetService<IDdsFileTypePlusStrings>();

            if (ddsFileTypePlusStrings != null)
            {
                this.strings = new PdnLocalizedStringResourceManager(ddsFileTypePlusStrings);
            }
            else
            {
                this.strings = new BuiltinStringResourceManager();
            }
        }

        private static string GetFileTypeName(IDdsFileTypePlusStrings strings)
        {
            string name = strings?.TryGetString(DdsFileTypePlusStringName.FileType_Name);

            if (name is null)
            {
                name = Properties.Resources.FileType_Name;
            }

            return name;
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                StaticListChoiceProperty.CreateForEnum(PropertyNames.FileFormat, DdsFileFormat.BC1, false),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.BC7CompressionSpeed, BC7CompressionSpeed.Medium, false),
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
                    PropertyNames.BC7CompressionSpeed,
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
                        DdsFileFormat.BC1,
                        DdsFileFormat.BC1Srgb,
                        DdsFileFormat.BC2,
                        DdsFileFormat.BC2Srgb,
                        DdsFileFormat.BC3,
                        DdsFileFormat.BC3Srgb,
                        DdsFileFormat.BC4Unsigned,
                        DdsFileFormat.BC5Signed,
                        DdsFileFormat.BC5Unsigned,
                        DdsFileFormat.BC6HUnsigned,
                        DdsFileFormat.BC7,
                        DdsFileFormat.BC7Srgb
                    },
                    true),
                new ReadOnlyBoundToBooleanRule(PropertyNames.MipMapResamplingAlgorithm, PropertyNames.GenerateMipMaps, true)
            };

            return new PropertyCollection(props, rules);
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultSaveConfigUI(props);

            PropertyControlInfo formatPCI = configUI.FindControlForPropertyName(PropertyNames.FileFormat);
            formatPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            formatPCI.SetValueDisplayName(DdsFileFormat.BC1, this.strings.GetString("DdsFileFormat_BC1"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC1Srgb, this.strings.GetString("DdsFileFormat_BC1Srgb"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC2, this.strings.GetString("DdsFileFormat_BC2"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC2Srgb, this.strings.GetString("DdsFileFormat_BC2Srgb"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC3, this.strings.GetString("DdsFileFormat_BC3"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC3Srgb, this.strings.GetString("DdsFileFormat_BC3Srgb"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC4Unsigned, this.strings.GetString("DdsFileFormat_BC4Unsigned"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC5Unsigned, this.strings.GetString("DdsFileFormat_BC5Unsigned"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC5Signed, this.strings.GetString("DdsFileFormat_BC5Signed"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC6HUnsigned, this.strings.GetString("DdsFileFormat_BC6HUnsigned"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC7, this.strings.GetString("DdsFileFormat_BC7"));
            formatPCI.SetValueDisplayName(DdsFileFormat.BC7Srgb, this.strings.GetString("DdsFileFormat_BC7Srgb"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8A8, this.strings.GetString("DdsFileFormat_B8G8R8A8"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8A8Srgb, this.strings.GetString("DdsFileFormat_B8G8R8A8Srgb"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8X8, this.strings.GetString("DdsFileFormat_B8G8R8X8"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8X8Srgb, this.strings.GetString("DdsFileFormat_B8G8R8X8Srgb"));
            formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8A8, this.strings.GetString("DdsFileFormat_R8G8B8A8"));
            formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8A8Srgb, this.strings.GetString("DdsFileFormat_R8G8B8A8Srgb"));
            formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8X8, this.strings.GetString("DdsFileFormat_R8G8B8X8"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B5G5R5A1, this.strings.GetString("DdsFileFormat_B5G5R5A1"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B4G4R4A4, this.strings.GetString("DdsFileFormat_B4G4R4A4"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B5G6R5, this.strings.GetString("DdsFileFormat_B5G6R5"));
            formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8, this.strings.GetString("DdsFileFormat_B8G8R8"));
            formatPCI.SetValueDisplayName(DdsFileFormat.R8Unsigned, this.strings.GetString("DdsFileFormat_R8Unsigned"));
            formatPCI.SetValueDisplayName(DdsFileFormat.R8G8Unsigned, this.strings.GetString("DdsFileFormat_R8G8Unsigned"));
            formatPCI.SetValueDisplayName(DdsFileFormat.R8G8Signed, this.strings.GetString("DdsFileFormat_R8G8Signed"));
            formatPCI.SetValueDisplayName(DdsFileFormat.R32Float, this.strings.GetString("DdsFileFormat_R32Float"));

            PropertyControlInfo compresionModePCI = configUI.FindControlForPropertyName(PropertyNames.BC7CompressionSpeed);
            compresionModePCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = this.strings.GetString("BC7CompressionSpeed_DisplayName");
            compresionModePCI.SetValueDisplayName(BC7CompressionSpeed.Fast, this.strings.GetString("BC7CompressionSpeed_Fast"));
            compresionModePCI.SetValueDisplayName(BC7CompressionSpeed.Medium, this.strings.GetString("BC7CompressionSpeed_Medium"));
            compresionModePCI.SetValueDisplayName(BC7CompressionSpeed.Slow, this.strings.GetString("BC7CompressionSpeed_Slow"));

            PropertyControlInfo errorMetricPCI = configUI.FindControlForPropertyName(PropertyNames.ErrorMetric);
            errorMetricPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = this.strings.GetString("ErrorMetric_DisplayName");
            errorMetricPCI.ControlType.Value = PropertyControlType.RadioButton;
            errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Perceptual, this.strings.GetString("ErrorMetric_Perceptual"));
            errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Uniform, this.strings.GetString("ErrorMetric_Uniform"));

            PropertyControlInfo cubemapPCI = configUI.FindControlForPropertyName(PropertyNames.CubeMap);
            cubemapPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            cubemapPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.strings.GetString("CubeMap_Description");

            PropertyControlInfo generateMipPCI = configUI.FindControlForPropertyName(PropertyNames.GenerateMipMaps);
            generateMipPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            generateMipPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.strings.GetString("GenerateMipMaps_Description");

            PropertyControlInfo mipResamplingPCI = configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm);
            mipResamplingPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.NearestNeighbor, this.strings.GetString("ResamplingAlgorithm_NearestNeighbor"));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Bicubic, this.strings.GetString("ResamplingAlgorithm_Bicubic"));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Bilinear, this.strings.GetString("ResamplingAlgorithm_Bilinear"));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.Fant, this.strings.GetString("ResamplingAlgorithm_Fant"));
            mipResamplingPCI.SetValueDisplayName(ResamplingAlgorithm.SuperSampling, this.strings.GetString("ResamplingAlgorithm_SuperSampling"));

            PropertyControlInfo forumLinkPCI = configUI.FindControlForPropertyName(PropertyNames.ForumLink);
            forumLinkPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = this.strings.GetString("ForumLink_DisplayName");
            forumLinkPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.strings.GetString("ForumLink_Description");

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
            BC7CompressionSpeed compressionSpeed = (BC7CompressionSpeed)token.GetProperty(PropertyNames.BC7CompressionSpeed).Value;
            DdsErrorMetric errorMetric = (DdsErrorMetric)token.GetProperty(PropertyNames.ErrorMetric).Value;
            bool cubeMap = token.GetProperty<BooleanProperty>(PropertyNames.CubeMap).Value;
            bool generateMipmaps = token.GetProperty<BooleanProperty>(PropertyNames.GenerateMipMaps).Value;
            ResamplingAlgorithm mipSampling = (ResamplingAlgorithm)token.GetProperty(PropertyNames.MipMapResamplingAlgorithm).Value;

            DdsFile.Save(this.services,
                         input,
                         output,
                         fileFormat,
                         errorMetric,
                         compressionSpeed,
                         cubeMap,
                         generateMipmaps,
                         mipSampling,
                         scratchSurface,
                         progressCallback);
        }

        public enum PropertyNames
        {
            FileFormat,
            BC7CompressionSpeed,
            ErrorMetric,
            CubeMap,
            GenerateMipMaps,
            MipMapResamplingAlgorithm,
            ForumLink,
            GitHubLink
        }
    }
}
