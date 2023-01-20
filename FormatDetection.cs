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
using System.IO;
using System.Runtime.CompilerServices;

#nullable enable

namespace DdsFileTypePlus
{
    internal static class FormatDetection
    {
        private static ReadOnlySpan<byte> BmpFileSignature => new byte[] { 0x42, 0x4D };

        private static ReadOnlySpan<byte> Gif87aFileSignature => new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };

        private static ReadOnlySpan<byte> Gif89aFileSignature => new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };

        private static ReadOnlySpan<byte> JpegFileSignature => new byte[] { 0xff, 0xd8, 0xff };

        private static ReadOnlySpan<byte> PngFileSignature => new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

        private static ReadOnlySpan<byte> TiffBigEndianFileSignature => new byte[] { 0x4d, 0x4d, 0x00, 0x2a };

        private static ReadOnlySpan<byte> TiffLittleEndianFileSignature => new byte[] { 0x49, 0x49, 0x2a, 0x00 };

        /// <summary>
        /// Attempts to get an <see cref="IFileTypeInfo"/> from the file signature.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>
        ///   An <see cref="IFileTypeInfo"/> instance if the file has the signature of a recognized image format;
        ///   otherwise, <see langword="null"/>.
        /// </returns>
        internal static IFileTypeInfo? TryGetFileTypeInfo(Stream stream, IServiceProvider serviceProvider)
        {
            string name = TryGetFileTypeName(stream);

            IFileTypeInfo? fileTypeInfo = null;

            if (!string.IsNullOrEmpty(name))
            {
                IFileTypesService? fileTypesService = serviceProvider?.GetService<IFileTypesService>();

                if (fileTypesService != null)
                {
                    foreach (IFileTypeInfo item in fileTypesService.FileTypes)
                    {
                        if (item.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                            && item.Options.SupportsLoading)
                        {
                            fileTypeInfo = item;
                            break;
                        }
                    }
                }
            }

            return fileTypeInfo;
        }

        [SkipLocalsInit]
        private static string TryGetFileTypeName(Stream stream)
        {
            string name = string.Empty;

            Span<byte> bytes = stackalloc byte[8];

            stream.Position = 0;

            stream.ReadExactly(bytes);

            if (FileSignatureMatches(bytes, PngFileSignature))
            {
                name = "PNG";
            }
            else if (FileSignatureMatches(bytes, BmpFileSignature))
            {
                name = "BMP";
            }
            else if (FileSignatureMatches(bytes, JpegFileSignature))
            {
                name = "JPEG";
            }
            else if (IsGifFileSignature(bytes))
            {
                name = "GIF";
            }
            else if (IsTiffFileSignature(bytes))
            {
                name = "TIFF";
            }

            return name;
        }

        private static bool FileSignatureMatches(ReadOnlySpan<byte> data, ReadOnlySpan<byte> signature)
                    => data.Length >= signature.Length && data.Slice(0, signature.Length).SequenceEqual(signature);

        private static bool IsGifFileSignature(ReadOnlySpan<byte> data)
        {
            bool result = false;

            if (data.Length >= Gif87aFileSignature.Length)
            {
                ReadOnlySpan<byte> bytes = data.Slice(0, Gif87aFileSignature.Length);

                result = bytes.SequenceEqual(Gif87aFileSignature)
                      || bytes.SequenceEqual(Gif89aFileSignature);
            }

            return result;
        }

        private static bool IsTiffFileSignature(ReadOnlySpan<byte> data)
        {
            bool result = false;

            if (data.Length >= TiffBigEndianFileSignature.Length)
            {
                ReadOnlySpan<byte> bytes = data.Slice(0, TiffBigEndianFileSignature.Length);

                result = bytes.SequenceEqual(TiffBigEndianFileSignature)
                      || bytes.SequenceEqual(TiffLittleEndianFileSignature);
            }

            return result;
        }
    }
}
