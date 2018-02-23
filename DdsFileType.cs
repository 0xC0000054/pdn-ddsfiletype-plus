////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017, 2018 Nicholas Hayes
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
        public DdsFileType() : base("DirectDraw Surface (DDS)", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving | FileTypeFlags.SavesWithProgress, new string[] { ".dds2" })
        {
        }

        public override PropertyCollection OnCreateSavePropertyCollection()
        {
            List<Property> props = new List<Property>
            {
                StaticListChoiceProperty.CreateForEnum(PropertyNames.FileFormat, DdsFileFormat.BC1, false),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.BC7CompressionMode, BC7CompressionMode.Normal, false),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.ErrorMetric, DdsErrorMetric.Perceptual, false),
                new BooleanProperty(PropertyNames.GenerateMipMaps, false),
                StaticListChoiceProperty.CreateForEnum(PropertyNames.MipMapResamplingAlgorithm, MipMapSampling.Fant, false)
            };

            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>
            {
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.BC7CompressionMode, PropertyNames.FileFormat, new object[] { DdsFileFormat.BC6H, DdsFileFormat.BC7, DdsFileFormat.BC7Srgb }, true),
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.ErrorMetric, PropertyNames.FileFormat, new object[] { DdsFileFormat.B8G8R8A8, DdsFileFormat.B8G8R8X8, DdsFileFormat.R8G8B8A8, DdsFileFormat.B5G5R5A1, DdsFileFormat.B4G4R4A4, DdsFileFormat.B5G6R5 }, false),
                new ReadOnlyBoundToBooleanRule(PropertyNames.MipMapResamplingAlgorithm, PropertyNames.GenerateMipMaps, true)
            };

            return new PropertyCollection(props, rules);
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultSaveConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.FileFormat, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC1, "BC1 (Linear, DXT1)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC1Srgb, "BC1 (sRGB, DX 10+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC2, "BC2 (Linear, DXT3)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC2Srgb, "BC2 (sRGB, DX 10+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC3, "BC3 (Linear, DXT5)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC3Srgb, "BC3 (sRGB, DX 10+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC4, "BC4 (Linear, DX 10+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC5, "BC5 (Linear, DX 10+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC6H, "BC6H (Linear, DX 11+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC7, "BC7 (Linear, DX 11+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC7Srgb, "BC7 (sRGB, DX 11+)");

            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.B8G8R8A8, "B8G8R8A8 (Linear, A8R8G8B8)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.B8G8R8X8, "B8G8R8X8 (Linear, X8R8G8B8)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.R8G8B8A8, "R8G8B8A8 (Linear, A8B8G8R8)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.B5G5R5A1, "B5G5R5A1 (Linear, A1R5G5B5)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.B4G4R4A4, "B4G4R4A4 (Linear, A4R4G4B4)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.B5G6R5, "B5G6R5 (Linear, R5G6B5)");
            configUI.SetPropertyControlValue(PropertyNames.BC7CompressionMode, ControlInfoPropertyNames.DisplayName, "BC7 Compression Mode");
            configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode).SetValueDisplayName(BC7CompressionMode.Fast, "Fast");
            configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode).SetValueDisplayName(BC7CompressionMode.Normal, "Normal");
            configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode).SetValueDisplayName(BC7CompressionMode.Slow, "Slow");
            configUI.SetPropertyControlValue(PropertyNames.ErrorMetric, ControlInfoPropertyNames.DisplayName, "Error Metric");
            configUI.SetPropertyControlType(PropertyNames.ErrorMetric, PropertyControlType.RadioButton);
            configUI.FindControlForPropertyName(PropertyNames.ErrorMetric).SetValueDisplayName(DdsErrorMetric.Perceptual, "Perceptual");
            configUI.FindControlForPropertyName(PropertyNames.ErrorMetric).SetValueDisplayName(DdsErrorMetric.Uniform, "Uniform");
            configUI.SetPropertyControlValue(PropertyNames.GenerateMipMaps, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.SetPropertyControlValue(PropertyNames.GenerateMipMaps, ControlInfoPropertyNames.Description, "Generate MipMaps");
            configUI.SetPropertyControlValue(PropertyNames.MipMapResamplingAlgorithm, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(MipMapSampling.NearestNeighbor, "Nearest Neighbor");
            configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(MipMapSampling.Bicubic, "Bicubic");
            configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(MipMapSampling.Bilinear, "Bilinear");
            configUI.FindControlForPropertyName(PropertyNames.MipMapResamplingAlgorithm).SetValueDisplayName(MipMapSampling.Fant, "Fant");

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
            bool generateMipmaps = token.GetProperty<BooleanProperty>(PropertyNames.GenerateMipMaps).Value;
            MipMapSampling mipSampling = (MipMapSampling)token.GetProperty(PropertyNames.MipMapResamplingAlgorithm).Value;

            DdsFile.Save(input, output, fileFormat, errorMetric, compressionMode, generateMipmaps, mipSampling, scratchSurface, progressCallback);
        }

        protected override bool IsReflexive(PropertyBasedSaveConfigToken token)
        {
            DdsFileFormat format = (DdsFileFormat)token.GetProperty(PropertyNames.FileFormat).Value;

            return (format == DdsFileFormat.B8G8R8A8 || format == DdsFileFormat.R8G8B8A8);
        }

        public enum PropertyNames
        {
            FileFormat,
            BC7CompressionMode,
            ErrorMetric,
            GenerateMipMaps,
            MipMapResamplingAlgorithm
        }
    }
}
