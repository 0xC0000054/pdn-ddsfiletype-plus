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
using PaintDotNet.FileTypes;
using System;
using System.IO;
using System.Runtime.CompilerServices;

#nullable enable

namespace DdsFileTypePlus
{
    internal static class FormatDetection
    {
        private static ReadOnlySpan<byte> BmpFileSignature => "BM"u8;

        private static ReadOnlySpan<byte> Gif87aFileSignature => "GIF87a"u8;

        private static ReadOnlySpan<byte> Gif89aFileSignature => "GIF89a"u8;

        private static ReadOnlySpan<byte> JpegFileSignature => new byte[] { 0xff, 0xd8, 0xff };

        private static ReadOnlySpan<byte> PngFileSignature => new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

        private static ReadOnlySpan<byte> TgaFileSignature => "TRUEVISION-XFILE.\0"u8;

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
            string? ext = TryGetFileTypeExtension(stream);
            if (string.IsNullOrEmpty(ext))
            {
                return null;
            }

            IFileTypesService? fileTypesService = serviceProvider?.GetService<IFileTypesService>();
            if (fileTypesService != null)
            {
                return fileTypesService.FindFileTypeForLoadingExtension(ext);
            }

            return null;
        }

        private static string? TryGetFileTypeExtension(Stream stream)
        {
            string? ext = TryGetExtensionFromImageHeader(stream);

            if (string.IsNullOrEmpty(ext))
            {
                ext = TryGetExtensionFromImageFooter(stream);
            }

            return ext;
        }

        private static string? TryGetExtensionFromImageFooter(Stream stream)
        {
            string? ext = null;

            if (IsTgaFile(stream))
            {
                ext = ".tga";
            }

            return ext;
        }

        [SkipLocalsInit]
        private static string? TryGetExtensionFromImageHeader(Stream stream)
        {
            string? ext = null;

            Span<byte> bytes = stackalloc byte[8];

            stream.Position = 0;

            stream.ReadExactly(bytes);

            if (FileSignatureMatches(bytes, PngFileSignature))
            {
                ext = ".png";
            }
            else if (FileSignatureMatches(bytes, BmpFileSignature))
            {
                ext = ".bmp";
            }
            else if (FileSignatureMatches(bytes, JpegFileSignature))
            {
                ext = ".jpg";
            }
            else if (IsGifFileSignature(bytes))
            {
                ext = ".gif";
            }
            else if (IsTiffFileSignature(bytes))
            {
                ext = ".tif";
            }

            return ext;
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

        [SkipLocalsInit]
        private static bool IsTgaFile(Stream stream)
        {
            // This only detects TGA 2.0 files, TGA versions prior to 2.0 didn't
            // have any signature to identify the format.
            // TGA 2.0 has a footer that includes the 18 byte TRUEVISION-XFILE.\0
            // signature at the end.

            const int TgaSignatureLength = 18;

            bool result = false;

            if (stream.Length > TgaSignatureLength)
            {
                stream.Seek(-TgaSignatureLength, SeekOrigin.End);

                Span<byte> signature = stackalloc byte[TgaSignatureLength];

                stream.ReadExactly(signature);

                result = signature.SequenceEqual(TgaFileSignature);
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
