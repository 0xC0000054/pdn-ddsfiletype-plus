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
using System;

namespace DdsFileTypePlus
{
    internal sealed class Texture : Disposable
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.surface != null)
                {
                    if (this.ownsSurface)
                    {
                        this.surface.Dispose();
                    }

                    this.surface = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
