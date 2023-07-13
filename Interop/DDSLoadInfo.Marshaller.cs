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

using System.Runtime.InteropServices.Marshalling;

namespace DdsFileTypePlus.Interop
{
    internal sealed partial class DDSLoadInfo
    {
        [CustomMarshaller(typeof(DDSLoadInfo), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
        public static class Marshaller
        {
            public unsafe struct Native
            {
                public nuint width;
                public nuint height;
                public byte cubeMap;
                public byte premultipliedAlpha;
            }

            public static DDSLoadInfo ConvertToManaged(Native unmanaged)
            {
                return new DDSLoadInfo()
                {
                    width = unmanaged.width,
                    height = unmanaged.height,
                    cubeMap = unmanaged.cubeMap != 0,
                    premultipliedAlpha = unmanaged.premultipliedAlpha != 0
                };
            }
        }
    }
}
