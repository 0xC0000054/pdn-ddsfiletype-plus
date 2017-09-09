////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017 Nicholas Hayes
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
        public DdsFileType() : base("DirectDraw Surface", FileTypeFlags.SupportsLoading | FileTypeFlags.SupportsSaving | FileTypeFlags.SavesWithProgress, new string[] { ".dds2" })
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
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.BC7CompressionMode, PropertyNames.FileFormat, new object[] { DdsFileFormat.BC6H, DdsFileFormat.BC7 }, true),
                new ReadOnlyBoundToValueRule<object, StaticListChoiceProperty>(PropertyNames.ErrorMetric, PropertyNames.FileFormat, new object[] { DdsFileFormat.A1R5G5B5, DdsFileFormat.A8R8G8B8, DdsFileFormat.R5G6B5, DdsFileFormat.X8R8G8B8 }, false),
                new ReadOnlyBoundToBooleanRule(PropertyNames.MipMapResamplingAlgorithm, PropertyNames.GenerateMipMaps, true)
            };

            return new PropertyCollection(props, rules);
        }

        public override ControlInfo OnCreateSaveConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultSaveConfigUI(props);

            configUI.SetPropertyControlValue(PropertyNames.FileFormat, ControlInfoPropertyNames.DisplayName, string.Empty);
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC1, "BC1 (DXT1)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC2, "BC2 (DXT3)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC3, "BC3 (DXT5)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC4, "BC4 (DX 10+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC5, "BC5 (DX 10+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC6H, "BC6H (DX 11+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.BC7, "BC7 (DX 11+)");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.A8R8G8B8, "A8B8G8R8");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.X8R8G8B8, "X8B8G8R8");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.A1R5G5B5, "A1R5G5B5");
            configUI.FindControlForPropertyName(PropertyNames.FileFormat).SetValueDisplayName(DdsFileFormat.R5G6B5, "R5G6B5");
            configUI.SetPropertyControlValue(PropertyNames.BC7CompressionMode, ControlInfoPropertyNames.DisplayName, "BC7 Compression Mode");
            configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode).SetValueDisplayName(BC7CompressionMode.Quick, "Quick");
            configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode).SetValueDisplayName(BC7CompressionMode.Normal, "Normal");
            configUI.FindControlForPropertyName(PropertyNames.BC7CompressionMode).SetValueDisplayName(BC7CompressionMode.Max, "Max");
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
