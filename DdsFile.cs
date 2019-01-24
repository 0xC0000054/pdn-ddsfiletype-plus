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
using System;
using System.IO;

namespace DdsFileTypePlus
{
    internal static class DdsFile
    {
        public static unsafe Document Load(Stream input)
        {
            Document doc = null;

            using (DdsNative.DdsImage image = DdsNative.Load(input))
            {
                doc = new Document(image.Width, image.Height);

                BitmapLayer layer = Layer.CreateBackgroundLayer(image.Width, image.Height);

                Surface surface = layer.Surface;

                byte* scan0 = (byte*)image.Scan0;
                int stride = image.Stride;

                for (int y = 0; y < surface.Height; y++)
                {
                    byte* src = scan0 + (y * stride);
                    ColorBgra* dst = surface.GetRowAddressUnchecked(y);

                    for (int x = 0; x < surface.Width; x++)
                    {
                        dst->R = src[0];
                        dst->G = src[1];
                        dst->B = src[2];
                        dst->A = src[3];

                        src += 4;
                        dst++;
                    }
                }

                doc.Layers.Add(layer);
            }

            return doc;
        }

        public static void Save(
            Document input,
            Stream output,
            DdsFileFormat format,
            DdsErrorMetric errorMetric,
            BC7CompressionMode compressionMode,
            bool cubeMap,
            bool generateMipmaps,
            MipMapSampling sampling,
            Surface scratchSurface,
            ProgressEventHandler progressCallback)
        {
            using (RenderArgs args = new RenderArgs(scratchSurface))
            {
                input.Render(args, true);
            }

            DdsNative.DdsProgressCallback ddsProgress = null;
            if (progressCallback != null)
            {
                ddsProgress = (UIntPtr done, UIntPtr total) =>
                {
                    double progress = (double)done.ToUInt64() / (double)total.ToUInt64();
                    progressCallback(null, new ProgressEventArgs(progress * 100.0, true));
                };
            }

            DdsNative.DDSSaveInfo info = new DdsNative.DDSSaveInfo
            {
                scan0 = scratchSurface.Scan0.Pointer,
                width = scratchSurface.Width,
                height = scratchSurface.Height,
                stride = scratchSurface.Stride,
                format = format,
                errorMetric = errorMetric,
                compressionMode = compressionMode,
                cubeMap = cubeMap && IsCrossedCubeMapSize(scratchSurface),
                generateMipmaps = generateMipmaps,
                mipmapSampling = sampling
            };

            DdsNative.Save(info, output, ddsProgress);
        }

        private static bool IsCrossedCubeMapSize(Surface surface)
        {
            // A crossed image cube map must have a 4:3 aspect ratio for horizontal cube maps
            // or a 3:4 aspect ratio for vertical cube maps, with the cube map images being square.
            //
            // For example, a horizontal crossed image with 256 x 256 pixel cube maps
            // would have a width of 1024 and a height of 768.

            if (surface.Width > surface.Height)
            {
                return (surface.Width / 4) == (surface.Height / 3);
            }
            else if (surface.Height > surface.Width)
            {
                return (surface.Width / 3) == (surface.Height / 4);
            }

            return false;
        }
    }
}
