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

using PaintDotNet;
using PaintDotNet.Imaging;
using System;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    internal sealed class DdsImage : Disposable
    {
        private DDSLoadInfo info;

        public int Width => this.info.width;

        public int Height => this.info.height;

        internal DdsImage(DDSLoadInfo info)
        {
            this.info = info;
        }

        public unsafe RegionPtr<ColorBgra32> AsRegionPtr()
        {
            return new RegionPtr<ColorBgra32>((ColorBgra32*)this.info.scan0,
                                              this.info.width,
                                              this.info.height,
                                              this.info.stride);
        }

        protected override void Dispose(bool disposing)
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                DdsIO_x64.FreeLoadInfo(this.info);
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                DdsIO_ARM64.FreeLoadInfo(this.info);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            base.Dispose(disposing);
        }
    }
}
