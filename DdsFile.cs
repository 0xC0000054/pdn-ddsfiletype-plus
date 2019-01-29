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
using System.Drawing;
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

                for (int y = 0; y < surface.Height; ++y)
                {
                    byte* src = scan0 + (y * stride);
                    ColorBgra* dst = surface.GetRowAddressUnchecked(y);

                    for (int x = 0; x < surface.Width; ++x)
                    {
                        dst->R = src[0];
                        dst->G = src[1];
                        dst->B = src[2];
                        dst->A = src[3];

                        src += 4;
                        ++dst;
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
            ResamplingAlgorithm sampling,
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

            int width = scratchSurface.Width;
            int height = scratchSurface.Height;
            int arraySize = 1;
            Size? cubeMapFaceSize = null;

            if (cubeMap && IsCrossedCubeMapSize(scratchSurface))
            {
                if (width > height)
                {
                    width /= 4;
                    height /= 3;
                }
                else
                {
                    width /= 3;
                    height /= 4;
                }
                arraySize = 6;
                cubeMapFaceSize = new Size(width, height);
            }

            int mipLevels = generateMipmaps ? GetMipCount(width, height) : 1;

            DdsNative.DDSSaveInfo info = new DdsNative.DDSSaveInfo
            {
                width = width,
                height = height,
                arraySize = arraySize,
                mipLevels = mipLevels,
                format = format,
                errorMetric = errorMetric,
                compressionMode = compressionMode,
                cubeMap = cubeMapFaceSize.HasValue
            };

            using (TextureCollection textures = GetTextures(scratchSurface, cubeMapFaceSize, mipLevels, sampling))
            {
                DdsNative.Save(info, textures, output, ddsProgress);
            }
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

        private static int GetMipCount(int width, int height)
        {
            int mipCount = 1;

            while (width > 1 || height > 1)
            {
                ++mipCount;

                if (width > 1)
                {
                    width /= 2;
                }
                if (height > 1)
                {
                    height /= 2;
                }
            }

            return mipCount;
        }

        private static TextureCollection GetTextures(Surface scratchSurface, Size? cubeMapFaceSize, int mipLevels, ResamplingAlgorithm algorithm)
        {
            TextureCollection textures = null;
            TextureCollection tempTextures = null;

            try
            {
                tempTextures = new TextureCollection(mipLevels);

                if (cubeMapFaceSize.HasValue)
                {
                    // DirectX 10+ requires DDS cube maps to have all 6 faces.
                    tempTextures.Capacity *= 6;

                    Size faceSize = cubeMapFaceSize.Value;
                    Point[] cubeMapOffsets = new Point[6];

                    // Split the crossed image into the individual cube map faces.
                    // The cube map faces in a DDS file are always ordered: +X, -X, +Y, -Y, +Z, -Z.

                    if (scratchSurface.Width > scratchSurface.Height)
                    {
                        // A horizontal crossed image uses the following layout:
                        //
                        //		  [ +Y ]
                        //	[ -X ][ +Z ][ +X ][ -Z ]
                        //		  [ -Y ]
                        //
                        cubeMapOffsets[0] = new Point(faceSize.Width * 2, faceSize.Height);  // +X
                        cubeMapOffsets[1] = new Point(0, faceSize.Height);                   // -X
                        cubeMapOffsets[2] = new Point(faceSize.Width, 0);                    // +Y
                        cubeMapOffsets[3] = new Point(faceSize.Width, faceSize.Height * 2);  // -Y
                        cubeMapOffsets[4] = new Point(faceSize.Width, faceSize.Height);      // +Z
                        cubeMapOffsets[5] = new Point(faceSize.Width * 3, faceSize.Height);  // -Z
                    }
                    else
                    {
                        // A vertical crossed image uses the following layout:
                        //
                        //		  [ +Y ]
                        //	[ -X ][ +Z ][ +X ]
                        //		  [ -Y ]
                        //		  [ -Z ]
                        //
                        cubeMapOffsets[0] = new Point(faceSize.Width * 2, faceSize.Height);  // +X
                        cubeMapOffsets[1] = new Point(0, faceSize.Height);                   // -X
                        cubeMapOffsets[2] = new Point(faceSize.Width, 0);                    // +Y
                        cubeMapOffsets[3] = new Point(faceSize.Width, faceSize.Height * 2);  // -Y
                        cubeMapOffsets[4] = new Point(faceSize.Width, faceSize.Height);      // +Z
                        cubeMapOffsets[5] = new Point(faceSize.Width, faceSize.Height * 3);  // -Z
                    }

                    for (int i = 0; i < 6; ++i)
                    {
                        Point srcStartOffset = cubeMapOffsets[i];

                        tempTextures.Add(new Texture(scratchSurface.CreateWindow(srcStartOffset.X, srcStartOffset.Y, faceSize.Width, faceSize.Height), true));

                        if (mipLevels > 1)
                        {
                            Surface cubeMapSurface = tempTextures[tempTextures.Count - 1].Surface;

                            for (int j = 1; j < mipLevels; ++j)
                            {
                                int mipWidth = Math.Max(1, cubeMapSurface.Width >> j);
                                int mipHeight = Math.Max(1, cubeMapSurface.Height >> j);

                                tempTextures.Add(CreateMipSurface(cubeMapSurface, mipWidth, mipHeight, algorithm));
                            }
                        }
                    }
                }
                else
                {
                    tempTextures.Add(new Texture(scratchSurface, false));

                    if (mipLevels > 1)
                    {
                        for (int j = 1; j < mipLevels; ++j)
                        {
                            int mipWidth = Math.Max(1, scratchSurface.Width >> j);
                            int mipHeight = Math.Max(1, scratchSurface.Height >> j);

                            tempTextures.Add(CreateMipSurface(scratchSurface, mipWidth, mipHeight, algorithm));
                        }
                    }
                }

                textures = tempTextures;
                tempTextures = null;
            }
            finally
            {
                if (tempTextures != null)
                {
                    tempTextures.Dispose();
                }
            }

            return textures;
        }

        private static unsafe Texture CreateMipSurface(Surface fullSize, int mipWidth, int mipHeight, ResamplingAlgorithm algorithm)
        {
            Texture mipTexture = null;
            Surface mipSurface = null;

            try
            {
                mipSurface = new Surface(mipWidth, mipHeight);

                if (HasTransparency(fullSize))
                {
                    // Downscaling images with transparency is done in a way that allows the completely transparent areas
                    // to retain their RGB color values, this behavior is required by some programs that use DDS files.

                    using (Surface color = new Surface(mipWidth, mipHeight))
                    using (Surface colorAndAlpha = new Surface(mipWidth, mipHeight))
                    {
                        using (Surface opaqueClone = fullSize.Clone())
                        {
                            // Set the alpha channel to fully opaque to prevent Windows Imaging Component
                            // from discarding the color information of completely transparent pixels.
                            new UnaryPixelOps.SetAlphaChannelTo255().Apply(opaqueClone, opaqueClone.Bounds);
                            color.FitSurface(algorithm, opaqueClone);
                        }

                        colorAndAlpha.FitSurface(algorithm, fullSize);

                        for (int y = 0; y < mipHeight; ++y)
                        {
                            ColorBgra* colorPtr = color.GetRowAddressUnchecked(y);
                            ColorBgra* alphaPtr = colorAndAlpha.GetRowAddressUnchecked(y);
                            ColorBgra* destPtr = mipSurface.GetRowAddressUnchecked(y);

                            for (int x = 0; x < mipWidth; ++x)
                            {
                                destPtr->B = colorPtr->B;
                                destPtr->G = colorPtr->G;
                                destPtr->R = colorPtr->R;
                                destPtr->A = alphaPtr->A;

                                ++colorPtr;
                                ++alphaPtr;
                                ++destPtr;
                            }
                        }
                    }
                }
                else
                {
                    mipSurface.FitSurface(algorithm, fullSize);
                }

                mipTexture = new Texture(mipSurface, true);
                mipSurface = null;
            }
            finally
            {
                if (mipSurface != null)
                {
                    mipSurface.Dispose();
                }
            }

            return mipTexture;
        }

        private static unsafe bool HasTransparency(Surface surface)
        {
            for (int y = 0; y < surface.Height; ++y)
            {
                ColorBgra* ptr = surface.GetRowAddressUnchecked(y);
                ColorBgra* ptrEnd = ptr + surface.Width;

                while (ptr < ptrEnd)
                {
                    if (ptr->A < 255)
                    {
                        return true;
                    }

                    ++ptr;
                }
            }

            return false;
        }
    }
}
