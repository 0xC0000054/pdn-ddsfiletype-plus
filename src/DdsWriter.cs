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
using PaintDotNet.AppModel;
using PaintDotNet.Imaging;
using PaintDotNet.Interop;
using PaintDotNet.Rendering;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace DdsFileTypePlus
{
    internal static class DdsWriter
    {
        public static void Save(
            IServiceProvider services,
            Document input,
            Stream output,
            DdsFileFormat format,
            bool errorDiffusionDithering,
            BC7CompressionSpeed compressionSpeed,
            DdsErrorMetric errorMetric,
            bool cubeMapFromCrossedImage,
            bool generateMipmaps,
            ResamplingAlgorithm sampling,
            bool useGammaCorrection,
            Surface scratchSurface,
            ProgressEventHandler progressCallback)
        {
            scratchSurface.Clear();
            input.CreateRenderer().Render(scratchSurface);

            int width = scratchSurface.Width;
            int height = scratchSurface.Height;
            int arraySize = 1;

            if (cubeMapFromCrossedImage && IsCrossedCubeMapSize(scratchSurface))
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
            }
            else
            {
                cubeMapFromCrossedImage = false;
            }

            int mipLevels = generateMipmaps ? GetMipCount(width, height) : 1;

            using (DirectXTexScratchImage textures = GetTextures(scratchSurface,
                                                                 cubeMapFromCrossedImage,
                                                                 width,
                                                                 height,
                                                                 arraySize,
                                                                 mipLevels,
                                                                 format,
                                                                 sampling,
                                                                 useGammaCorrection))
            {
                if (format == DdsFileFormat.R8G8B8X8 || format == DdsFileFormat.B8G8R8)
                {
                    new DX9DdsWriter(width, height, arraySize, mipLevels, format).Save(textures, output, progressCallback);
                }
                else
                {
                    DdsProgressCallback ddsProgress = null;
                    if (progressCallback != null)
                    {
                        ddsProgress = (double progressPercentage) =>
                        {
                            try
                            {
                                progressCallback(null, new ProgressEventArgs(progressPercentage, true));
                                return true;
                            }
                            catch (OperationCanceledException)
                            {
                                return false;
                            }
                        };
                    }

                    (DXGI_FORMAT saveFormat, DdsFileOptions fileOptions) = GetSaveFormat(format);

                    DDSSaveInfo info = new()
                    {
                        Format = saveFormat,
                        FileOptions = fileOptions,
                        ErrorMetric = errorMetric,
                        CompressionSpeed = compressionSpeed,
                        ErrorDiffusionDithering = errorDiffusionDithering
                    };

                    if (format == DdsFileFormat.BC6HUnsigned || format == DdsFileFormat.BC7 || format == DdsFileFormat.BC7Srgb)
                    {
                        // Try to get the DXGI adapter that Paint.NET uses for rendering.
                        // This device will be used by the DirectCompute-based BC6H/BC7 compressor, if it supports DirectCompute.
                        // If the specified device does not support DirectCompute, either WARP or the CPU compressor will be used.
                        IDxgiAdapterService dxgiAdapterService = services.GetService<IDxgiAdapterService>();

                        using (SafeComObject dxgiAdapterComObject = dxgiAdapterService.GetRenderingAdapter())
                        {
                            bool addRefSuccess = false;

                            try
                            {
                                dxgiAdapterComObject.DangerousAddRef(ref addRefSuccess);

                                IntPtr directComputeAdapter = IntPtr.Zero;

                                if (addRefSuccess)
                                {
                                    directComputeAdapter = dxgiAdapterComObject.DangerousGetHandle();
                                }

                                DdsNative.Save(info, textures, output, directComputeAdapter, ddsProgress);
                            }
                            finally
                            {
                                if (addRefSuccess)
                                {
                                    dxgiAdapterComObject.DangerousRelease();
                                }
                            }
                        }
                    }
                    else
                    {
                        DdsNative.Save(info, textures, output, directComputeAdapter: IntPtr.Zero, ddsProgress);
                    }
                }
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

        private static (DXGI_FORMAT, DdsFileOptions) GetSaveFormat(DdsFileFormat format)
        {
            DXGI_FORMAT dxgiFormat;
            DdsFileOptions options = DdsFileOptions.None;

            switch (format)
            {
                case DdsFileFormat.BC1:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM;
                    break;
                case DdsFileFormat.BC1Srgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM_SRGB;
                    break;
                case DdsFileFormat.BC2:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM;
                    break;
                case DdsFileFormat.BC2Srgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM_SRGB;
                    break;
                case DdsFileFormat.BC3:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM;
                    break;
                case DdsFileFormat.BC3Srgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM_SRGB;
                    break;
                case DdsFileFormat.BC4Unsigned:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM;
                    break;
                case DdsFileFormat.BC5Unsigned:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM;
                    break;
                case DdsFileFormat.BC5Signed:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC5_SNORM;
                    break;
                case DdsFileFormat.BC6HUnsigned:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC6H_UF16;
                    break;
                case DdsFileFormat.BC7:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM;
                    break;
                case DdsFileFormat.BC7Srgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM_SRGB;
                    break;
                case DdsFileFormat.B8G8R8A8:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM;
                    break;
                case DdsFileFormat.B8G8R8A8Srgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
                    break;
                case DdsFileFormat.B8G8R8X8:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM;
                    break;
                case DdsFileFormat.B8G8R8X8Srgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM_SRGB;
                    break;
                case DdsFileFormat.R8G8B8A8:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;
                    break;
                case DdsFileFormat.R8G8B8A8Srgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
                    break;
                case DdsFileFormat.B5G5R5A1:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM;
                    break;
                case DdsFileFormat.B4G4R4A4:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B4G4R4A4_UNORM;
                    break;
                case DdsFileFormat.B5G6R5:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B5G6R5_UNORM;
                    break;
                case DdsFileFormat.R8Unsigned:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R8_UNORM;
                    break;
                case DdsFileFormat.R8G8Unsigned:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8_UNORM;
                    break;
                case DdsFileFormat.R8G8Signed:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8_SNORM;
                    break;
                case DdsFileFormat.R32Float:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT;
                    break;
                case DdsFileFormat.BC4Ati1:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM;
                    // DirectXTex normally uses the BC4U four-character-code when saving DX9 compatible
                    // DDS files, ForceLegacyDX9Formats makes it use the ATI1 four-character-code.
                    options = DdsFileOptions.ForceLegacyDX9Formats;
                    break;
                case DdsFileFormat.BC5Ati2:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM;
                    // DirectXTex normally uses the BC5U four-character-code when saving DX9 compatible
                    // DDS files, ForceLegacyDX9Formats makes it use the ATI2 four-character-code.
                    options = DdsFileOptions.ForceLegacyDX9Formats;
                    break;
                case DdsFileFormat.BC3Rxgb:
                    dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM;
                    options = DdsFileOptions.ForceBC3ToRXGB;
                    break;
                case DdsFileFormat.R8G8B8X8:
                case DdsFileFormat.B8G8R8:
                default:
                    throw new InvalidOperationException($"{nameof(DdsFileFormat)}.{format} does not map to a DXGI format.");

            }

            return (dxgiFormat, options);
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

        private static DirectXTexScratchImage GetTextures(Surface scratchSurface,
                                                          bool cubeMapFromCrossedImage,
                                                          int width,
                                                          int height,
                                                          int arraySize,
                                                          int mipLevels,
                                                          DdsFileFormat format,
                                                          ResamplingAlgorithm algorithm,
                                                          bool useGammaCorrection)
        {
            DirectXTexScratchImage image = null;
            DirectXTexScratchImage tempImage = null;

            try
            {
                DXGI_FORMAT dxgiFormat;
#pragma warning disable IDE0066 // Convert switch statement to expression
                switch (format)
                {
                    case DdsFileFormat.B8G8R8X8Srgb:
                    case DdsFileFormat.BC1Srgb:
                    case DdsFileFormat.BC2Srgb:
                    case DdsFileFormat.BC3Srgb:
                    case DdsFileFormat.BC7Srgb:
                    case DdsFileFormat.R8G8B8A8Srgb:
                        dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
                        break;
                    case DdsFileFormat.B8G8R8A8:
                    // R8G8B8X8 and B8G8R8 are legacy DirectX 9 formats that DXGI does not support.
                    // See DX9DdsWriter for the writer implementation.
                    case DdsFileFormat.R8G8B8X8:
                    case DdsFileFormat.B8G8R8:
                        dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM;
                        break;
                    case DdsFileFormat.B8G8R8A8Srgb:
                        dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
                        break;
                    case DdsFileFormat.BC1:
                    case DdsFileFormat.BC2:
                    case DdsFileFormat.BC3:
                    case DdsFileFormat.BC4Unsigned:
                    case DdsFileFormat.BC5Unsigned:
                    case DdsFileFormat.BC5Signed:
                    case DdsFileFormat.BC6HUnsigned:
                    case DdsFileFormat.BC7:
                    case DdsFileFormat.B8G8R8X8:
                    case DdsFileFormat.R8G8B8A8:
                    case DdsFileFormat.B5G5R5A1:
                    case DdsFileFormat.B4G4R4A4:
                    case DdsFileFormat.B5G6R5:
                    case DdsFileFormat.R8Unsigned:
                    case DdsFileFormat.R8G8Unsigned:
                    case DdsFileFormat.R8G8Signed:
                    case DdsFileFormat.R32Float:
                    case DdsFileFormat.BC3Rxgb:
                    case DdsFileFormat.BC4Ati1:
                    case DdsFileFormat.BC5Ati2:
                        dxgiFormat = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;
                        break;
                    default:
                        throw new InvalidOperationException($"{nameof(DdsFileFormat)}.{format} does not map to a DXGI format.");
                }
#pragma warning restore IDE0066 // Convert switch statement to expression

                tempImage = new DirectXTexScratchImage(width,
                                                       height,
                                                       arraySize,
                                                       mipLevels,
                                                       dxgiFormat,
                                                       cubeMapFromCrossedImage);

                if (cubeMapFromCrossedImage)
                {
                    Point[] cubeMapOffsets = new Point[6];

                    // Split the crossed image into the individual cube map faces.
                    //
                    // The crossed image uses the same layout as the Intel® Texture Works DDS plug-in for Adobe Photoshop®
                    // (https://github.com/GameTechDev/Intel-Texture-Works-Plugin)
                    //
                    // The DirectXTex texassemble utility and Unity® both use different layouts, so there does not appear
                    // to be any common standard for a crossed image.
                    //
                    // The cube map faces in a DDS file are always ordered: +X, -X, +Y, -Y, +Z, -Z.

                    if (scratchSurface.Width > scratchSurface.Height)
                    {
                        // A horizontal crossed image uses the following layout:
                        //
                        //		  [ +Y ]
                        //	[ -X ][ +Z ][ +X ][ -Z ]
                        //		  [ -Y ]
                        //
                        cubeMapOffsets[0] = new Point(width * 2, height);  // +X
                        cubeMapOffsets[1] = new Point(0, height);          // -X
                        cubeMapOffsets[2] = new Point(width, 0);           // +Y
                        cubeMapOffsets[3] = new Point(width, height * 2);  // -Y
                        cubeMapOffsets[4] = new Point(width, height);      // +Z
                        cubeMapOffsets[5] = new Point(width * 3, height);  // -Z
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
                        cubeMapOffsets[0] = new Point(width * 2, height);  // +X
                        cubeMapOffsets[1] = new Point(0, height);          // -X
                        cubeMapOffsets[2] = new Point(width, 0);           // +Y
                        cubeMapOffsets[3] = new Point(width, height * 2);  // -Y
                        cubeMapOffsets[4] = new Point(width, height);      // +Z
                        cubeMapOffsets[5] = new Point(width, height * 3);  // -Z
                    }

                    for (int i = 0; i < 6; ++i)
                    {
                        Point srcStartOffset = cubeMapOffsets[i];

                        using (Surface cubeMapSurface = scratchSurface.CreateWindow(srcStartOffset.X,
                                                                                    srcStartOffset.Y,
                                                                                    width,
                                                                                    height))
                        {
                            uint item = (uint)i;

                            RenderToDirectXTexScratchImage(cubeMapSurface, tempImage.GetImageData(0, item, 0), format);

                            if (mipLevels > 1)
                            {
                                using (MipSourceSurface mipSource = new(cubeMapSurface))
                                {
                                    for (int mip = 1; mip < mipLevels; ++mip)
                                    {
                                        RenderMipMap(mipSource,
                                                     tempImage.GetImageData((uint)mip, item, 0),
                                                     algorithm,
                                                     useGammaCorrection,
                                                     format);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    RenderToDirectXTexScratchImage(scratchSurface, tempImage.GetImageData(0, 0, 0), format);

                    if (mipLevels > 1)
                    {
                        using (MipSourceSurface mipSource = new(scratchSurface))
                        {
                            for (int mip = 1; mip < mipLevels; ++mip)
                            {
                                RenderMipMap(mipSource,
                                             tempImage.GetImageData((uint)mip, 0, 0),
                                             algorithm,
                                             useGammaCorrection,
                                             format);
                            }
                        }
                    }
                }

                image = tempImage;
                tempImage = null;
            }
            finally
            {
                tempImage?.Dispose();
            }

            return image;
        }

        private static unsafe void RenderMipMap(MipSourceSurface source,
                                                DirectXTexScratchImageData mipData,
                                                ResamplingAlgorithm algorithm,
                                                bool useGammaCorrection,
                                                DdsFileFormat format)
        {
            FitSurfaceOptions options = useGammaCorrection ? FitSurfaceOptions.UseGammaCorrection : FitSurfaceOptions.Default;

            using (Surface mipSurface = new((int)mipData.Width, (int)mipData.Height))
            {
                mipSurface.FitSurface(algorithm, source.Surface, options);

                if (source.HasTransparency)
                {
                    // Downscaling images with transparency is done in a way that allows the completely transparent areas
                    // to retain their RGB color values, this behavior is required by some programs that use DDS files.
                    using (Surface color = new(mipSurface.Width, mipSurface.Height))
                    {
                        // An opaque copy of the source surface is used to prevent Windows Imaging Component
                        // from discarding the color information of completely transparent pixels.
                        color.FitSurface(algorithm, source.OpaqueSurface, options);

                        for (int y = 0; y < mipSurface.Height; ++y)
                        {
                            ColorBgra* colorPtr = color.GetRowPointerUnchecked(y);
                            ColorBgra* destPtr = mipSurface.GetRowPointerUnchecked(y);

                            for (int x = 0; x < mipSurface.Width; ++x)
                            {
                                // Copy the color data from the opaque image to create a merged
                                // image with the transparent pixels retaining their original values.
                                destPtr->B = colorPtr->B;
                                destPtr->G = colorPtr->G;
                                destPtr->R = colorPtr->R;

                                ++colorPtr;
                                ++destPtr;
                            }
                        }
                    }
                }

                RenderToDirectXTexScratchImage(mipSurface, mipData, format);
            }
        }

        private static unsafe void RenderToDirectXTexScratchImage(Surface surface, DirectXTexScratchImageData scratchImage, DdsFileFormat format)
        {
            RegionPtr<ColorBgra32> source = surface.AsRegionPtr().Cast<ColorBgra32>();

            switch (scratchImage.Format)
            {
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
                    source.CopyTo(scratchImage.AsRegionPtr<ColorBgra32>(checkPixelType: false));
                    break;
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM:
                case DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
                    if (TryGetSwizzledImageFormat(format, out SwizzledImageFormat swizzledImageFormat))
                    {
                        RenderToSwizzledImage(source, scratchImage.AsRegionPtr<ColorRgba32>(checkPixelType: false), swizzledImageFormat);
                    }
                    else
                    {
                        PixelKernels.ConvertBgra32ToRgba32(scratchImage.AsRegionPtr<ColorRgba32>(checkPixelType: false), source);
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported {nameof(DXGI_FORMAT)} value: {scratchImage.Format}.");
            }

            static bool TryGetSwizzledImageFormat(DdsFileFormat format, out SwizzledImageFormat swizzledImageFormat)
            {
                if (format == DdsFileFormat.BC3Rxgb)
                {
                    swizzledImageFormat = SwizzledImageFormat.Xgbr;
                    return true;
                }

                swizzledImageFormat = SwizzledImageFormat.Unknown;
                return false;
            }
        }

        private static unsafe void RenderToSwizzledImage(RegionPtr<ColorBgra32> source,
                                                         RegionPtr<ColorRgba32> destination,
                                                         SwizzledImageFormat format)
        {
            int width = source.Width;
            int height = source.Height;
            RegionRowPtrCollection<ColorBgra32> sourceRows = source.Rows;
            RegionRowPtrCollection<ColorRgba32> destinationRows = destination.Rows;

            for (int y = 0; y < height; ++y)
            {
                ColorBgra32* src = sourceRows[y].Ptr;
                ColorRgba32* dest = destinationRows[y].Ptr;

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
                            dest->A = src->G;
                            dest->B = src->B;
                            dest->G = 0;
                            break;
                        case SwizzledImageFormat.Rgxb:
                            dest->R = src->R;
                            dest->G = src->G;
                            dest->A = src->B;
                            dest->B = 0;
                            break;
                        case SwizzledImageFormat.Rbxg:
                            dest->R = src->R;
                            dest->A = src->G;
                            dest->G = src->B;
                            dest->B = 0;
                            break;
                        case SwizzledImageFormat.Xgbr:
                            dest->A = src->R;
                            dest->G = src->G;
                            dest->B = src->B;
                            dest->R = 0;
                            break;
                        case SwizzledImageFormat.Xrbg:
                            dest->G = src->R;
                            dest->A = src->G;
                            dest->B = src->B;
                            dest->R = 0;
                            break;
                        case SwizzledImageFormat.Xgxr:
                            dest->A = src->R;
                            dest->G = src->G;
                            dest->B = 0;
                            dest->R = 0;
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

        private sealed class MipSourceSurface : Disposable
        {
            private readonly Lazy<bool> hasTransparency;
            private Surface opaqueSurface;

            public MipSourceSurface(Surface surface)
            {
                this.Surface = surface;
                this.opaqueSurface = null;
                this.hasTransparency = new Lazy<bool>(SurfaceHasTransparency);
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public bool HasTransparency => this.hasTransparency.Value;

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            public Surface OpaqueSurface
            {
                get
                {
                    if (this.IsDisposed)
                    {
                        ExceptionUtil.ThrowObjectDisposedException(nameof(MipSourceSurface));
                    }

                    this.opaqueSurface ??= CreateOpaqueSurface();

                    return this.opaqueSurface;
                }
            }

            public Surface Surface { get; }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (this.opaqueSurface != null)
                    {
                        this.opaqueSurface.Dispose();
                        this.opaqueSurface = null;
                    }
                }

                base.Dispose(disposing);
            }

            private Surface CreateOpaqueSurface()
            {
                Surface opaqueClone = this.Surface.Clone();

                PixelKernels.SetAlphaChannel(opaqueClone.AsRegionPtr().Cast<ColorBgra32>(), ColorAlpha8.Opaque);

                return opaqueClone;
            }

            private unsafe bool SurfaceHasTransparency()
            {
                Surface surface = this.Surface;

                for (int y = 0; y < surface.Height; ++y)
                {
                    ColorBgra* ptr = surface.GetRowPointerUnchecked(y);
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
}
