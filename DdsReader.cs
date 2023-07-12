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

using DdsFileTypePlus.Interop;
using PaintDotNet;
using PaintDotNet.Imaging;
using PaintDotNet.Rendering;
using System;
using System.IO;

namespace DdsFileTypePlus
{
    internal static class DdsReader
    {
        public static unsafe Document Load(Stream input, IServiceProvider services)
        {
            Document doc = null;

            try
            {
                using (DirectXTexScratchImage image = DdsNative.Load(input, out DDSLoadInfo info))
                {
                    int documentWidth = checked((int)info.width);
                    int documentHeight = checked((int)info.height);

                    if (info.cubeMap)
                    {
                        // Cube maps are flattened using the horizontal cross layout.
                        documentWidth = checked(documentWidth * 4);
                        documentHeight = checked(documentHeight * 3);
                    }

                    doc = new Document(documentWidth, documentHeight);

                    BitmapLayer layer = Layer.CreateBackgroundLayer(documentWidth, documentHeight);

                    RegionPtr<ColorBgra32> destination = layer.Surface.AsRegionPtr().Cast<ColorBgra32>();

                    if (info.cubeMap)
                    {
                        // The cube map faces in a DDS file are always ordered: +X, -X, +Y, -Y, +Z, -Z.
                        // Setup the offsets used to convert the cube map faces to a horizontal crossed image.
                        // A horizontal crossed image uses the following layout:
                        //
                        //		  [ +Y ]
                        //	[ -X ][ +Z ][ +X ][ -Z ]
                        //		  [ -Y ]
                        //
                        int cubeMapWidth = (int)info.width;
                        int cubeMapHeight = (int)info.height;

                        Point2Int32[] cubeMapOffsets = new Point2Int32[6]
                        {
                            new Point2Int32(cubeMapWidth * 2, cubeMapHeight), // +X
                            new Point2Int32(0, cubeMapHeight),			      // -X
                            new Point2Int32(cubeMapWidth, 0),			      // +Y
                            new Point2Int32(cubeMapWidth, cubeMapHeight * 2), // -Y
                            new Point2Int32(cubeMapWidth, cubeMapHeight),	  // +Z
                            new Point2Int32(cubeMapWidth * 3, cubeMapHeight)  // -Z
                        };

                        // Initialize the layer as completely transparent.
                        destination.Clear();

                        for (int i = 0; i < 6; i++)
                        {
                            DirectXTexScratchImageData data = image.GetImageData(0, (uint)i, 0);
                            Point2Int32 offset = cubeMapOffsets[i];

                            RegionPtr<ColorRgba32> source = data.AsRegionPtr<ColorRgba32>();
                            RegionPtr<ColorBgra32> target = destination.Slice(offset.X, offset.Y, cubeMapWidth, cubeMapHeight);

                            RenderDdsImage(source, target, info.premultipliedAlpha);
                        }
                    }
                    else
                    {
                        // For images other than cube maps we only load the first image in the file.
                        DirectXTexScratchImageData data = image.GetImageData(0, 0, 0);

                        RenderDdsImage(data.AsRegionPtr<ColorRgba32>(), destination, info.premultipliedAlpha);
                    }

                    doc.Layers.Add(layer);
                }
            }
            catch (FormatException ex) when (ex.HResult == HResult.InvalidDdsFileSignature)
            {
                IFileTypeInfo fileTypeInfo = FormatDetection.TryGetFileTypeInfo(input, services);

                if (fileTypeInfo != null)
                {
                    FileType fileType = fileTypeInfo.GetInstance();

                    input.Position = 0;

                    doc = fileType.Load(input);
                }
                else
                {
                    throw;
                }
            }

            return doc;
        }

        private static void RenderDdsImage(RegionPtr<ColorRgba32> source,
                                           RegionPtr<ColorBgra32> destination,
                                           bool premultipliedAlpha)
        {
            PixelKernels.ConvertRgba32ToBgra32(destination, source);

            if (premultipliedAlpha)
            {
                PixelKernels.ConvertPbgra32ToBgra32(destination);
            }
        }
    }
}
