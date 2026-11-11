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
using PaintDotNet.Imaging;
using PaintDotNet.Rendering;
using System;

namespace DdsFileTypePlus.Interop
{
    // Wraps an IBitmap+IBitmapLock around a RegionPtr. Note that this is inherently unsafe:
    // this type does not and cannot guarantee that the memory backed by the RegionPtr remains
    // valid for as long as the IBitmapSource is in use. It is the caller's responsibility to
    // ensure that the RegionPtr's backing store stays alive and is not freed or reused for
    // the entire lifetime of the IBitmapSource. In the context of a Paint.NET FileType plugin,
    // this requirement is typically satisfied by only using the IBitmapSource within the
    // lifetime of the corresponding Save or Load operation.
    internal unsafe sealed class RegionPtrBitmap<TPixel>
        : BitmapSourceBase<TPixel>,
          IBitmap<TPixel>,
          IBitmapLock<TPixel>
          where TPixel : unmanaged, INaturalPixelInfo
    {
        private readonly RegionPtr<TPixel> region;

        public RegionPtrBitmap(RegionPtr<TPixel> region)
            : base(region.Size)
        {
            this.region = region;
        }

        public TPixel* Buffer => this.region.Ptr;

        void* IBitmapLock.Buffer => this.region.Ptr;

        public int BufferStride => this.region.Stride;

        public nuint BufferSize => checked((nuint)this.region.BufferSize);

        public IBitmapLock<TPixel> Lock(RectInt32 rect, BitmapLockOptions lockOptions)
        {
            return new RegionPtrBitmap<TPixel>(this.region.Slice(rect));
        }

        IBitmapLock IBitmap.Lock(RectInt32 rect, BitmapLockOptions lockOptions)
        {
            return Lock(rect, lockOptions);
        }

        protected override void OnCopyPixels(RegionPtr<TPixel> dst, Point2Int32 srcOffset)
        {
            this.region.Slice(srcOffset, dst.Size).CopyTo(dst);
        }

        Vector2Double IBitmap.Resolution
        {
            get => base.Resolution;
            set => throw new NotSupportedException();
        }

        void IBitmap.SetPalette(IPalette palette)
        {
            throw new NotSupportedException();
        }
    }
}
