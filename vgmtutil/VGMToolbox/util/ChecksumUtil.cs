using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

using ICSharpCode.SharpZipLib.Checksum;
using Ionic.Zlib;

namespace VGMToolbox.util
{
    public sealed class ChecksumUtil
    {
        private ChecksumUtil() { }

        public static string GetCrc32OfFullFile(FileStream stream)
        {
            long initialStreamPosition = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);

            Crc32 crc32 = new Crc32();
            byte[] buffer = new byte[4096];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                crc32.Update(new ArraySegment<byte>(buffer, 0, read));
            }

            stream.Position = initialStreamPosition;
            return ((int)crc32.Value).ToString("X8", CultureInfo.InvariantCulture);
        }

        public static string GetMd5OfFullFile(FileStream stream)
        {
            MD5CryptoServiceProvider hashMd5 = new MD5CryptoServiceProvider();
            stream.Seek(0, SeekOrigin.Begin);
            hashMd5.ComputeHash(stream);
            return ParseFile.ByteArrayToString(hashMd5.Hash);
        }

        public static byte[] GetSha1(byte[] dataBlock)
        {
            SHA1CryptoServiceProvider sha1Hash = new SHA1CryptoServiceProvider();
            sha1Hash.ComputeHash(dataBlock);
            return sha1Hash.Hash;
        }

        public static string GetSha1OfFullFile(FileStream stream)
        {
            SHA1CryptoServiceProvider sha1Hash = new SHA1CryptoServiceProvider();
            stream.Seek(0, SeekOrigin.Begin);
            sha1Hash.ComputeHash(stream);
            return ParseFile.ByteArrayToString(sha1Hash.Hash);
        }

        public static string GetSha512OfFullFile(FileStream stream)
        {
            SHA512CryptoServiceProvider sha512 = new SHA512CryptoServiceProvider();
            stream.Seek(0, SeekOrigin.Begin);
            sha512.ComputeHash(stream);
            return ParseFile.ByteArrayToString(sha512.Hash);
        }

        public static void AddChunkToChecksum(Stream stream, int startingOffset, int length, ref Crc32 checksumGenerator)
        {
            int remaining = length;
            byte[] data = new byte[4096];
            int read;
            int offset = startingOffset;

            stream.Seek((long)startingOffset, SeekOrigin.Begin);

            while (remaining > 0)
            {
                read = stream.Read(data, 0, Math.Min(4096, remaining));
                if (read <= 0)
                {
                    throw new EndOfStreamException(
                        String.Format(CultureInfo.CurrentCulture, "流结束,还剩{0}个字节要读取", remaining));
                }

                checksumGenerator.Update(new ArraySegment<byte>(data, 0, read));
                remaining -= read;
                offset += read;
            }
        }

        public static void AddChunkToChecksum(
            Stream sourceStream,
            int startingOffset,
            int length,
            ref Crc32 checksumGeneratorCrc32,
            ref CryptoStream checksumStreamMd5,
            ref CryptoStream checksumStreamSha1)
        {
            int remaining = length;
            byte[] data = new byte[4096];
            int read;
            int offset = startingOffset;

            sourceStream.Seek((long)startingOffset, SeekOrigin.Begin);

            while (remaining > 0)
            {
                read = sourceStream.Read(data, 0, Math.Min(4096, remaining));
                if (read <= 0)
                {
                    throw new EndOfStreamException(
                        String.Format(CultureInfo.CurrentCulture, "流结束,还剩{0}个字节要读取", remaining));
                }

                checksumGeneratorCrc32.Update(new ArraySegment<byte>(data, 0, read));
                checksumStreamMd5.Write(data, 0, read);
                checksumStreamSha1.Write(data, 0, read);
                remaining -= read;
                offset += read;
            }
        }
    }
}
