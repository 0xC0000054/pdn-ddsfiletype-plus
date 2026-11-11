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

using PaintDotNet;
using PaintDotNet.FileTypes;
using PaintDotNet.FileTypes.Dds;
using PaintDotNet.Imaging;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using System;
using System.Collections.Generic;

namespace DdsFileTypePlus
{
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    public sealed class DdsFileType : PropertyBasedFileType
    {
        private static readonly IReadOnlyList<string> FileExtensions = new string[] { ".dds" };

        private readonly IDdsStringResourceManager strings;

        public DdsFileType(IFileTypeHost host) :
            base(host,
                 GetFileTypeName(host.Services.GetService<IDdsFileTypePlusStrings>()!),
                 FileTypeOptions.Create() with
                 {
                     LoadExtensions = FileExtensions,
                     SaveExtensions = FileExtensions,
                     IsSavingConfigurable = true
                 })
        {
            IDdsFileTypePlusStrings ddsFileTypePlusStrings = this.Services.GetService<IDdsFileTypePlusStrings>()!;

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
            return strings?.TryGetString(DdsFileTypePlusStringNames.FileType_Name) ?? Properties.Resources.FileType_Name;
        }

        protected override PropertyBasedFileTypeSaver OnCreatePropertyBasedSaver()
        {
            return new Saver(this);
        }

        private sealed class Saver
            : PropertyBasedFileTypeSaver
        {
            private readonly DdsFileType fileType;

            public Saver(DdsFileType fileType) : base(fileType)
            {
                this.fileType = fileType;
            }

            protected override PropertyCollection OnCreateSavePropertiesForDocument(ICreateSavePropertiesForDocumentContext context)
            {
                PropertyCollection properties = context.ProposedProperties.Clone();

                if (SaveOptionsMetadata.TryLoad(context.MetadataFromLoad, out SaveOptionsMetadata? metadata))
                {
                    if (metadata.Format.HasValue)
                    {
                        properties[PropertyNames.FileFormat].Value = metadata.Format.Value;
                    }

                    if (metadata.GenerateMipMaps.HasValue)
                    {
                        properties[PropertyNames.GenerateMipMaps].Value = metadata.GenerateMipMaps.Value;
                    }

                    if (metadata.CubeMap.HasValue)
                    {
                        properties[PropertyNames.CubeMap].Value = metadata.CubeMap.Value;
                    }
                }

                return properties;
            }

            protected override PropertyCollection OnCreateDefaultSaveProperties()
            {
                List<Property> props = new()
                {
                    CreateFileFormat(),
                    new BooleanProperty(PropertyNames.ErrorDiffusionDithering, true),
                    StaticListChoiceProperty.CreateForEnum(PropertyNames.BC7CompressionSpeed, BC7CompressionSpeed.Medium, false),
                    StaticListChoiceProperty.CreateForEnum(PropertyNames.ErrorMetric, DdsErrorMetric.Perceptual, false),
                    new BooleanProperty(PropertyNames.CubeMap, false),
                    new BooleanProperty(PropertyNames.GenerateMipMaps, false),
                    CreateMipMapResamplingAlgorithm(),
                    new BooleanProperty(PropertyNames.UseGammaCorrection, true),
                    new UriProperty(PropertyNames.ForumLink, new Uri("https://forums.getpaint.net/topic/111731-dds-filetype-plus")),
                    new UriProperty(PropertyNames.GitHubLink, new Uri("https://github.com/0xC0000054/pdn-ddsfiletype-plus")),
                    new StringProperty(PropertyNames.PluginVersion),
                };

                List<PropertyCollectionRule> rules = new()
                {
                    new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(
                        PropertyNames.ErrorDiffusionDithering,
                        PropertyNames.FileFormat,
                        new object[]
                        {
                            DdsFileFormat.BC1,
                            DdsFileFormat.BC1Srgb,
                            DdsFileFormat.BC2,
                            DdsFileFormat.BC2Srgb,
                            DdsFileFormat.BC3,
                            DdsFileFormat.BC3Srgb,
                            DdsFileFormat.BC3Rxgb,
                            DdsFileFormat.B4G4R4A4,
                            DdsFileFormat.B5G5R5A1,
                            DdsFileFormat.B5G6R5
                        },
                        true),
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
                            DdsFileFormat.BC3Rxgb,
                        },
                        true),
                    new ReadOnlyBoundToBooleanRule(PropertyNames.MipMapResamplingAlgorithm, PropertyNames.GenerateMipMaps, true),
                    new ReadOnlyBoundToBooleanRule(PropertyNames.UseGammaCorrection, PropertyNames.GenerateMipMaps, true)
                };

                return new PropertyCollection(props, rules);

                static StaticListChoiceProperty CreateFileFormat()
                {
                    object[] values = new object[]
                    {
                        DdsFileFormat.BC1,
                        DdsFileFormat.BC1Srgb,
                        DdsFileFormat.BC2,
                        DdsFileFormat.BC2Srgb,
                        DdsFileFormat.BC3,
                        DdsFileFormat.BC3Srgb,
                        DdsFileFormat.BC3Rxgb,
                        DdsFileFormat.BC4Unsigned,
                        DdsFileFormat.BC4Ati1,
                        DdsFileFormat.BC5Unsigned,
                        DdsFileFormat.BC5Ati2,
                        DdsFileFormat.BC5Signed,
                        DdsFileFormat.BC6HUnsigned,
                        DdsFileFormat.BC7,
                        DdsFileFormat.BC7Srgb,
                        DdsFileFormat.B8G8R8A8,
                        DdsFileFormat.B8G8R8A8Srgb,
                        DdsFileFormat.B8G8R8X8,
                        DdsFileFormat.B8G8R8X8Srgb,
                        DdsFileFormat.R8G8B8A8,
                        DdsFileFormat.R8G8B8A8Srgb,
                        DdsFileFormat.R8G8B8X8,
                        DdsFileFormat.B5G5R5A1,
                        DdsFileFormat.B4G4R4A4,
                        DdsFileFormat.B5G6R5,
                        DdsFileFormat.B8G8R8,
                        DdsFileFormat.R8Unsigned,
                        DdsFileFormat.R8G8Unsigned,
                        DdsFileFormat.R8G8Signed,
                        DdsFileFormat.R32Float,
                    };

                    int defaultChoiceIndex = Array.IndexOf(values, DdsFileFormat.BC1);

                    return new StaticListChoiceProperty(PropertyNames.FileFormat, values, defaultChoiceIndex, false);
                }

                static StaticListChoiceProperty CreateMipMapResamplingAlgorithm()
                {
                    object[] values = new object[]
                    {
                        BitmapInterpolationMode2.HighQualityCubic,
                        BitmapInterpolationMode2.CubicSmooth,
                        BitmapInterpolationMode2.HighQualityLinear,
                        BitmapInterpolationMode2.Linear,
                        BitmapInterpolationMode2.HighQualityAdaptiveSharp,
                        BitmapInterpolationMode2.Lanczos3,
                        BitmapInterpolationMode2.Fant,
                        BitmapInterpolationMode2.NearestNeighbor,
                    };

                    int defaultChoiceIndex = Array.IndexOf(values, BitmapInterpolationMode2.HighQualityCubic);

                    return new StaticListChoiceProperty(PropertyNames.MipMapResamplingAlgorithm, values, defaultChoiceIndex, false);
                }
            }

            protected override ControlInfo OnCreateSaveOptionsUI(PropertyCollection props)
            {
                ControlInfo configUI = CreateDefaultSaveOptionsUI(props);

                PropertyControlInfo formatPCI = configUI.FindControlForPropertyName(PropertyNames.FileFormat);
                formatPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                formatPCI.SetValueDisplayName(DdsFileFormat.BC1, this.fileType.strings.GetString("DdsFileFormat_BC1"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC1Srgb, this.fileType.strings.GetString("DdsFileFormat_BC1Srgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC2, this.fileType.strings.GetString("DdsFileFormat_BC2"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC2Srgb, this.fileType.strings.GetString("DdsFileFormat_BC2Srgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC3, this.fileType.strings.GetString("DdsFileFormat_BC3"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC3Srgb, this.fileType.strings.GetString("DdsFileFormat_BC3Srgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC3Rxgb, this.fileType.strings.GetString("DdsFileFormat_BC3Rxgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC4Unsigned, this.fileType.strings.GetString("DdsFileFormat_BC4Unsigned"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC4Ati1, this.fileType.strings.GetString("DdsFileFormat_BC4ATI1"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC5Unsigned, this.fileType.strings.GetString("DdsFileFormat_BC5Unsigned"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC5Ati2, this.fileType.strings.GetString("DdsFileFormat_BC5ATI2"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC5Signed, this.fileType.strings.GetString("DdsFileFormat_BC5Signed"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC6HUnsigned, this.fileType.strings.GetString("DdsFileFormat_BC6HUnsigned"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC7, this.fileType.strings.GetString("DdsFileFormat_BC7"));
                formatPCI.SetValueDisplayName(DdsFileFormat.BC7Srgb, this.fileType.strings.GetString("DdsFileFormat_BC7Srgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8A8, this.fileType.strings.GetString("DdsFileFormat_B8G8R8A8"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8A8Srgb, this.fileType.strings.GetString("DdsFileFormat_B8G8R8A8Srgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8X8, this.fileType.strings.GetString("DdsFileFormat_B8G8R8X8"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8X8Srgb, this.fileType.strings.GetString("DdsFileFormat_B8G8R8X8Srgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8A8, this.fileType.strings.GetString("DdsFileFormat_R8G8B8A8"));
                formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8A8Srgb, this.fileType.strings.GetString("DdsFileFormat_R8G8B8A8Srgb"));
                formatPCI.SetValueDisplayName(DdsFileFormat.R8G8B8X8, this.fileType.strings.GetString("DdsFileFormat_R8G8B8X8"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B5G5R5A1, this.fileType.strings.GetString("DdsFileFormat_B5G5R5A1"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B4G4R4A4, this.fileType.strings.GetString("DdsFileFormat_B4G4R4A4"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B5G6R5, this.fileType.strings.GetString("DdsFileFormat_B5G6R5"));
                formatPCI.SetValueDisplayName(DdsFileFormat.B8G8R8, this.fileType.strings.GetString("DdsFileFormat_B8G8R8"));
                formatPCI.SetValueDisplayName(DdsFileFormat.R8Unsigned, this.fileType.strings.GetString("DdsFileFormat_R8Unsigned"));
                formatPCI.SetValueDisplayName(DdsFileFormat.R8G8Unsigned, this.fileType.strings.GetString("DdsFileFormat_R8G8Unsigned"));
                formatPCI.SetValueDisplayName(DdsFileFormat.R8G8Signed, this.fileType.strings.GetString("DdsFileFormat_R8G8Signed"));
                formatPCI.SetValueDisplayName(DdsFileFormat.R32Float, this.fileType.strings.GetString("DdsFileFormat_R32Float"));

                PropertyControlInfo ditheringPCI = configUI.FindControlForPropertyName(PropertyNames.ErrorDiffusionDithering);
                ditheringPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                ditheringPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.fileType.strings.GetString("ErrorDiffusionDithering_Description");

                PropertyControlInfo compressionModePCI = configUI.FindControlForPropertyName(PropertyNames.BC7CompressionSpeed);
                compressionModePCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = this.fileType.strings.GetString("BC7CompressionSpeed_DisplayName");
                compressionModePCI.SetValueDisplayName(BC7CompressionSpeed.Fast, this.fileType.strings.GetString("BC7CompressionSpeed_Fast"));
                compressionModePCI.SetValueDisplayName(BC7CompressionSpeed.Medium, this.fileType.strings.GetString("BC7CompressionSpeed_Medium"));
                compressionModePCI.SetValueDisplayName(BC7CompressionSpeed.Slow, this.fileType.strings.GetString("BC7CompressionSpeed_Slow"));

                PropertyControlInfo errorMetricPCI = configUI.FindControlForPropertyName(PropertyNames.ErrorMetric);
                errorMetricPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = this.fileType.strings.GetString("ErrorMetric_DisplayName");
                errorMetricPCI.ControlType.Value = PropertyControlType.RadioButton;
                errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Perceptual, this.fileType.strings.GetString("ErrorMetric_Perceptual"));
                errorMetricPCI.SetValueDisplayName(DdsErrorMetric.Uniform, this.fileType.strings.GetString("ErrorMetric_Uniform"));

                PropertyControlInfo cubemapPCI = configUI.FindControlForPropertyName(PropertyNames.CubeMap);
                cubemapPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                cubemapPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.fileType.strings.GetString("CubeMap_Description");

                PropertyControlInfo generateMipPCI = configUI.FindControlForPropertyName(PropertyNames.GenerateMipMaps);
                generateMipPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                generateMipPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.fileType.strings.GetString("GenerateMipMaps_Description");

                PropertyControlInfo mipResamplingPCI = configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm);
                mipResamplingPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.HighQualityCubic, this.fileType.strings.GetString("ResamplingAlgorithm_Cubic"));
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.CubicSmooth, this.fileType.strings.GetString("ResamplingAlgorithm_CubicSmooth"));
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.HighQualityLinear, this.fileType.strings.GetString("ResamplingAlgorithm_Linear"));
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.Linear, this.fileType.strings.GetString("ResamplingAlgorithm_LinearLowQuality"));
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.HighQualityAdaptiveSharp, this.fileType.strings.GetString("ResamplingAlgorithm_AdaptiveHighQuality"));
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.Lanczos3, this.fileType.strings.GetString("ResamplingAlgorithm_Lanczos3"));
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.Fant, this.fileType.strings.GetString("ResamplingAlgorithm_Fant"));
                mipResamplingPCI.SetValueDisplayName(BitmapInterpolationMode2.NearestNeighbor, this.fileType.strings.GetString("ResamplingAlgorithm_NearestNeighbor"));

                PropertyControlInfo gammaCorrectionPCI = configUI.FindControlForPropertyName(PropertyNames.UseGammaCorrection);
                gammaCorrectionPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                gammaCorrectionPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.fileType.strings.GetString("UseGammaCorrection_Description");

                PropertyControlInfo forumLinkPCI = configUI.FindControlForPropertyName(PropertyNames.ForumLink);
                forumLinkPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = this.fileType.strings.GetString("ForumLink_DisplayName");
                forumLinkPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = this.fileType.strings.GetString("ForumLink_Description");

                PropertyControlInfo githubLinkPCI = configUI.FindControlForPropertyName(PropertyNames.GitHubLink);
                githubLinkPCI.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                githubLinkPCI.ControlProperties[ControlInfoPropertyNames.Description].Value = "GitHub"; // GitHub is a brand name that should not be localized.

                PropertyControlInfo pluginVersionInfo = configUI.FindControlForPropertyName(PropertyNames.PluginVersion);
                pluginVersionInfo.ControlType.Value = PropertyControlType.Label;
                pluginVersionInfo.ControlProperties[ControlInfoPropertyNames.DisplayName].Value = string.Empty;
                pluginVersionInfo.ControlProperties[ControlInfoPropertyNames.Description].Value = "DdsFileTypePlus v" + VersionInfo.PluginVersion;

                return configUI;
            }

            protected override void OnSave(IPropertyBasedFileTypeSaveContext context)
            {
                IPropertyBasedFileTypeSaveOptions options = context.Options;

                DdsFileFormat fileFormat = (DdsFileFormat)options.GetProperty(PropertyNames.FileFormat).Value;
                bool errorDiffusionDithering = options.GetProperty<BooleanProperty>(PropertyNames.ErrorDiffusionDithering).Value;
                BC7CompressionSpeed compressionSpeed = (BC7CompressionSpeed)options.GetProperty(PropertyNames.BC7CompressionSpeed).Value;
                DdsErrorMetric errorMetric = (DdsErrorMetric)options.GetProperty(PropertyNames.ErrorMetric).Value;
                bool cubeMap = options.GetProperty<BooleanProperty>(PropertyNames.CubeMap).Value;
                bool generateMipmaps = options.GetProperty<BooleanProperty>(PropertyNames.GenerateMipMaps).Value;
                BitmapInterpolationMode2 mipSampling = (BitmapInterpolationMode2)options.GetProperty(PropertyNames.MipMapResamplingAlgorithm).Value;
                bool useGammaCorrection = options.GetProperty<BooleanProperty>(PropertyNames.UseGammaCorrection).Value;

                DdsWriter.Save(this.Services,
                               context.Document,
                               context.Output,
                               fileFormat,
                               errorDiffusionDithering,
                               compressionSpeed,
                               errorMetric,
                               cubeMap,
                               generateMipmaps,
                               mipSampling,
                               useGammaCorrection,
                               context.ProgressCallback);
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
                GitHubLink,
                ErrorDiffusionDithering,
                UseGammaCorrection,
                PluginVersion
            }
        }

        protected override PropertyBasedFileTypeLoader OnCreatePropertyBasedLoader()
        {
            return new Loader(this);
        }

        private sealed class Loader
            : PropertyBasedFileTypeLoader
        {
            public Loader(DdsFileType fileType) : base(fileType)
            {
            }

            protected override IFileTypeDocument OnLoad(IPropertyBasedFileTypeLoadContext context)
            {
                return DdsReader.Load(context, this.Services);
            }
        }
    }
}
