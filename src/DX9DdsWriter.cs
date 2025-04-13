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
using PaintDotNet.IO;
using System;
using System.IO;

namespace DdsFileTypePlus
{
    internal sealed class DX9DdsWriter
    {
        private readonly int width;
        private readonly int height;
        private readonly int arraySize;
        private readonly int mipLevels;
        private readonly DdsFileFormat format;
        private readonly byte[] pixelBuffer;

        internal DX9DdsWriter(int width, int height, int arraySize, int mipLevels, DdsFileFormat format)
        {
            this.width = width;
            this.height = height;
            this.arraySize = arraySize;
            this.mipLevels = mipLevels;
            this.format = format;
            this.pixelBuffer = format switch
            {
                DdsFileFormat.R8G8B8X8 => new byte[4],
                DdsFileFormat.B8G8R8 => new byte[3],
                _ => throw new InvalidOperationException(GetUnsupportedFormatMessage(format)),
            };
        }

        private static ReadOnlySpan<byte> DdsMagic => "DDS "u8;

        internal void Save(DirectXTexScratchImage images, Stream output, ProgressEventHandler progressCallback)
        {
            DdsHeader header = new(this.width, this.height, this.arraySize, this.mipLevels, this.format);

            output.Write(DdsMagic);
            header.Write(output);

            double progressDone = 0.0;
            double progressTotal = (double)this.arraySize * this.mipLevels;
            double progressDelta = (1.0 / progressTotal) * 100.0;

            for (int item = 0; item < this.arraySize; ++item)
            {
                for (int mip = 0; mip < this.mipLevels; ++mip)
                {
                    DirectXTexScratchImageData imageData = images.GetImageData((uint)mip, (uint)item, 0);

                    WritePixelData(imageData.AsRegionPtr<ColorBgra32>(), output);

                    progressCallback?.Invoke(this, new ProgressEventArgs(progressDone, true));
                    progressDone += progressDelta;
                }
            }
        }

        private unsafe void WritePixelData(RegionPtr<ColorBgra32> source, Stream output)
        {
            foreach (RegionRowPtr<ColorBgra32> row in source.Rows)
            {
                ColorBgra32* ptr = row.Ptr;
                ColorBgra32* ptrEnd = row.EndPtr;

                while (ptr < ptrEnd)
                {
                    switch (this.format)
                    {
                        case DdsFileFormat.R8G8B8X8:
                            this.pixelBuffer[0] = ptr->R;
                            this.pixelBuffer[1] = ptr->G;
                            this.pixelBuffer[2] = ptr->B;
                            break;
                        case DdsFileFormat.B8G8R8:
                            this.pixelBuffer[0] = ptr->B;
                            this.pixelBuffer[1] = ptr->G;
                            this.pixelBuffer[2] = ptr->R;
                            break;
                        default:
                            throw new InvalidOperationException(GetUnsupportedFormatMessage(this.format));
                    }

                    output.Write(this.pixelBuffer, 0, this.pixelBuffer.Length);

                    ptr++;
                }
            }
        }

        private static string GetUnsupportedFormatMessage(DdsFileFormat format)
        {
            return format.ToString() + " is not a supported pixel format.";
        }

        private sealed class DdsHeader
        {
            private readonly uint size;
            private readonly uint flags;
            private readonly uint height;
            private readonly uint width;
            private readonly uint pitchOrLinearSize;
            private readonly uint depth; // only if DDS_HEADER_FLAGS_VOLUME is set in flags
            private readonly uint mipMapCount;
            private readonly uint[] reserved1;
            private readonly DdsPixelFormat ddspf;
            private readonly uint caps;
            private readonly uint caps2;
            private readonly uint caps3;
            private readonly uint caps4;
            private readonly uint reserved2;

            private const uint SizeOf = 124;

            internal DdsHeader(int width, int height, int arraySize, int mipCount, DdsFileFormat format)
            {
                this.size = SizeOf;
                this.flags = HeaderFlags.Texture;
                this.height = (uint)height;
                this.width = (uint)width;
                switch (format)
                {
                    case DdsFileFormat.R8G8B8X8:
                        this.flags |= HeaderFlags.Pitch;
                        this.pitchOrLinearSize = (((uint)width * 32) + 7) / 8;
                        break;
                    case DdsFileFormat.B8G8R8:
                        this.flags |= HeaderFlags.Pitch;
                        this.pitchOrLinearSize = (((uint)width * 24) + 7) / 8;
                        break;
                    default:
                        throw new InvalidOperationException(GetUnsupportedFormatMessage(format));
                }
                this.depth = 1;
                if (mipCount > 1)
                {
                    this.flags |= HeaderFlags.Mipmap;
                    this.mipMapCount = (uint)mipCount;
                    this.caps |= SurfaceFlags.Mipmap;
                }
                else
                {
                    this.mipMapCount = 1;
                }
                this.reserved1 = new uint[11];
                this.ddspf = new DdsPixelFormat(format);
                this.caps |= SurfaceFlags.Texture;
                if (arraySize == 6)
                {
                    this.caps |= SurfaceFlags.Cubemap;
                    this.caps2 |= CubemapFaces.All;
                }
                this.caps3 = 0;
                this.caps4 = 0;
                this.reserved2 = 0;
            }

            internal void Write(Stream output)
            {
                output.WriteUInt32(this.size);
                output.WriteUInt32(this.flags);
                output.WriteUInt32(this.height);
                output.WriteUInt32(this.width);
                output.WriteUInt32(this.pitchOrLinearSize);
                output.WriteUInt32(this.depth);
                output.WriteUInt32(this.mipMapCount);
                for (int i = 0; i < this.reserved1.Length; ++i)
                {
                    output.WriteUInt32(this.reserved1[i]);
                }
                this.ddspf.Write(output);
                output.WriteUInt32(this.caps);
                output.WriteUInt32(this.caps2);
                output.WriteUInt32(this.caps3);
                output.WriteUInt32(this.caps4);
                output.WriteUInt32(this.reserved2);
            }

            private static class HeaderFlags
            {
                internal const uint Texture = 0x00001007; // DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT
                internal const uint Mipmap = 0x00020000; // DDSD_MIPMAPCOUNT
                internal const uint Pitch = 0x00000008; // DDSD_PITCH
            }

            private static class SurfaceFlags
            {
                internal const uint Texture = 0x00001000; // DDSCAPS_TEXTURE
                internal const uint Mipmap = 0x00400008; // DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
                internal const uint Cubemap = 0x00000008; // DDSCAPS_COMPLEX
            }

            private static class CubemapFaces
            {
                internal const uint PositiveX = 0x00000600; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
                internal const uint NegativeX = 0x00000a00; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
                internal const uint PositiveY = 0x00001200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
                internal const uint NegativeY = 0x00002200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
                internal const uint PositiveZ = 0x00004200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
                internal const uint NegativeZ = 0x00008200; // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ

                internal const uint All = PositiveX | NegativeX |
                                          PositiveY | NegativeY |
                                          PositiveZ | NegativeZ;
            }
        }

        private sealed class DdsPixelFormat
        {
            private readonly uint size;
            private readonly uint flags;
            private readonly uint fourCC;
            private readonly uint RGBBitCount;
            private readonly uint RBitMask;
            private readonly uint GBitMask;
            private readonly uint BBitMask;
            private readonly uint ABitMask;

            private const int SizeOf = 32;

            internal DdsPixelFormat(DdsFileFormat format)
            {
                this.size = SizeOf;
                switch (format)
                {
                    case DdsFileFormat.R8G8B8X8:
                        this.flags = PixelFormatFlags.Rgb;
                        this.fourCC = 0;
                        this.RGBBitCount = 32;
                        this.RBitMask = 0x000000ff;
                        this.GBitMask = 0x0000ff00;
                        this.BBitMask = 0x00ff0000;
                        this.ABitMask = 0x00000000;
                        break;
                    case DdsFileFormat.B8G8R8:
                        this.flags = PixelFormatFlags.Rgb;
                        this.fourCC = 0;
                        this.RGBBitCount = 24;
                        this.RBitMask = 0x00ff0000;
                        this.GBitMask = 0x0000ff00;
                        this.BBitMask = 0x000000ff;
                        this.ABitMask = 0x00000000;
                        break;
                    default:
                        throw new InvalidOperationException(GetUnsupportedFormatMessage(format));
                }
            }

            internal void Write(Stream output)
            {
                output.WriteUInt32(this.size);
                output.WriteUInt32(this.flags);
                output.WriteUInt32(this.fourCC);
                output.WriteUInt32(this.RGBBitCount);
                output.WriteUInt32(this.RBitMask);
                output.WriteUInt32(this.GBitMask);
                output.WriteUInt32(this.BBitMask);
                output.WriteUInt32(this.ABitMask);
            }

            private static class PixelFormatFlags
            {
                internal const uint Rgb = 0x00000040;
            }
        }
    }
}
