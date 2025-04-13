////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2025 Nicholas Hayes
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
                    if (info.IsTextureArray)
                    {
                        // Reject files containing a texture array because loading only the first item
                        // poses a data loss risk when saving.
                        throw new FormatException("DDS files containing a texture array are not supported.");
                    }
                    else if (info.VolumeMap && info.Depth > 1)
                    {
                        // Reject files containing a volume map with multiple slices because loading
                        // only the first item poses a data loss risk when saving.
                        throw new FormatException("DDS files containing a volume map are not supported.");
                    }

                    int documentWidth = checked((int)info.Width);
                    int documentHeight = checked((int)info.Height);

                    if (info.CubeMap)
                    {
                        // Cube maps are flattened using the horizontal cross layout.
                        documentWidth = checked(documentWidth * 4);
                        documentHeight = checked(documentHeight * 3);
                    }

                    doc = new Document(documentWidth, documentHeight);

                    BitmapLayer layer = Layer.CreateBackgroundLayer(documentWidth, documentHeight);

                    RegionPtr<ColorBgra32> destination = layer.Surface.AsRegionPtr().Cast<ColorBgra32>();

                    if (info.CubeMap)
                    {
                        // The cube map faces in a DDS file are always ordered: +X, -X, +Y, -Y, +Z, -Z.
                        // Setup the offsets used to convert the cube map faces to a horizontal crossed image.
                        // A horizontal crossed image uses the following layout:
                        //
                        //		  [ +Y ]
                        //	[ -X ][ +Z ][ +X ][ -Z ]
                        //		  [ -Y ]
                        //
                        int cubeMapWidth = (int)info.Width;
                        int cubeMapHeight = (int)info.Height;

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

                        for (int i = 0; i < 6; ++i)
                        {
                            DirectXTexScratchImageData data = image.GetImageData(0, (uint)i, 0);
                            Point2Int32 offset = cubeMapOffsets[i];

                            RegionPtr<ColorRgba32> source = data.AsRegionPtr<ColorRgba32>();
                            RegionPtr<ColorBgra32> target = destination.Slice(offset.X, offset.Y, cubeMapWidth, cubeMapHeight);

                            RenderDdsImage(source, target, info);
                        }
                    }
                    else
                    {
                        // For images other than cube maps we only load the first image in the file.
                        DirectXTexScratchImageData data = image.GetImageData(0, 0, 0);

                        RenderDdsImage(data.AsRegionPtr<ColorRgba32>(), destination, info);
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
                                           DDSLoadInfo loadInfo)
        {
            if (loadInfo.SwizzledImageFormat != SwizzledImageFormat.Unknown)
            {
                RenderFromSwizzledImage(source, destination, loadInfo.SwizzledImageFormat);
            }
            else
            {
                PixelKernels.ConvertRgba32ToBgra32(destination, source);
            }

            if (loadInfo.PremultipliedAlpha)
            {
                PixelKernels.ConvertPbgra32ToBgra32(destination);
            }
        }

        private static unsafe void RenderFromSwizzledImage(RegionPtr<ColorRgba32> source,
                                                           RegionPtr<ColorBgra32> destination,
                                                           SwizzledImageFormat format)
        {
            int width = source.Width;
            int height = source.Height;
            RegionRowPtrCollection<ColorRgba32> sourceRows = source.Rows;
            RegionRowPtrCollection<ColorBgra32> destinationRows = destination.Rows;

            for (int y = 0; y < height; ++y)
            {
                ColorRgba32* src = sourceRows[y].Ptr;
                ColorBgra32* dest = destinationRows[y].Ptr;

                for (int x = 0; x < width; ++x)
                {
                    // None of the formats appear to have transparency data in the swapped alpha channel, it is
                    // always set to 0 or some other low value.
                    //
                    // This makes sense as the swizzled formats are optimized for storing the data that is used in
                    // a normal map, and take advantage of the fact that BC3/DXT5 compresses the alpha channel
                    // separately to improve the quality for one or more of those components.
                    // Most of the formats store 3 channels (X, Y and Z), but some only store 2 channels (X and Y).
                    //
                    // Compressonator appears to always set the alpha channel to 255 when it reads an image that is
                    // stored in one of these formats.

                    switch (format)
                    {
                        case SwizzledImageFormat.Rxbg:
                            dest->R = src->R;
                            dest->G = src->A;
                            dest->B = src->B;
                            dest->A = 255;
                            break;
                        case SwizzledImageFormat.Rgxb:
                            dest->R = src->R;
                            dest->G = src->G;
                            dest->B = src->A;
                            dest->A = 255;
                            break;
                        case SwizzledImageFormat.Rbxg:
                            dest->R = src->R;
                            dest->G = src->A;
                            dest->B = src->G;
                            dest->A = 255;
                            break;
                        case SwizzledImageFormat.Xgbr:
                            dest->R = src->A;
                            dest->G = src->G;
                            dest->B = src->B;
                            dest->A = 255;
                            break;
                        case SwizzledImageFormat.Xrbg:
                            dest->R = src->G;
                            dest->G = src->A;
                            dest->B = src->B;
                            dest->A = 255;
                            break;
                        case SwizzledImageFormat.Xgxr:
                            dest->R = src->A;
                            dest->G = src->G;
                            dest->B = src->B;
                            dest->A = 255;
                            break;
                        case SwizzledImageFormat.Unknown:
                        default:
                            throw new InvalidOperationException($"Unsupported {nameof(SwizzledImageFormat)}: {format}.");
                    }

                    ++src;
                    ++dest;
                }
            }
        }
    }
}
