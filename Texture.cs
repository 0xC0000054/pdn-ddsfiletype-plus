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

namespace DdsFileTypePlus
{
    internal sealed class Texture : IDisposable
    {
        private Surface surface;
        private readonly bool ownsSurface;

        public Texture(Surface surface, bool ownsSurface)
        {
            this.surface = surface ?? throw new ArgumentNullException(nameof(surface));
            this.ownsSurface = ownsSurface;
        }

        public Surface Surface
        {
            get
            {
                System.Diagnostics.Debug.Assert(this.surface != null, "The texture has been disposed.");

                return this.surface;
            }
        }

        public void Dispose()
        {
            if (this.surface != null && this.ownsSurface)
            {
                this.surface.Dispose();
                this.surface = null;
            }
        }
    }
}
