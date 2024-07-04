////////////////////////////////////////////////////////////////////////
//
// This file is part of pdn-ddsfiletype-plus, a DDS FileType plugin
// for Paint.NET that adds support for the DX10 and later formats.
//
// Copyright (c) 2017-2024 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace DdsFileTypePlus.Interop
{
    internal sealed class SafeDirectXTexScratchImage : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDirectXTexScratchImage() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                DdsIO_x64.DestroyScratchImage(this.handle);
            }
            else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            {
                DdsIO_ARM64.DestroyScratchImage(this.handle);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return true;
        }
    }
}
