using System;
using System.IO;
using VGMToolbox.util;

namespace VGMToolbox.format
{
    public class SofdecStream : Mpeg1Stream
    {
        new public const string DefaultVideoExtension = ".m2v";

        public const string AdxAudioExtension = ".adx";
        public const string AixAudioExtension = ".aix";
        public const string Ac3AudioExtension = ".ac3";

        public static readonly byte[] AixSignatureBytes = new byte[] { 0x41, 0x49, 0x58, 0x46 };
        public static readonly byte[] Ac3SignatureBytes = new byte[] { 0x0B, 0x77 };
        private const byte FirstAudioStreamId = 0xC0;

        public SofdecStream(string path): base(path)
        {
            this.UsesSameIdForMultipleAudioTracks = true;
            this.FileExtensionAudio = AdxAudioExtension;
            this.FileExtensionVideo = DefaultVideoExtension;
        }

        protected override byte GetStreamId(Stream readStream, long currentOffset)
        {
            byte audioStreamId = ParseFile.ParseSimpleOffset(readStream, currentOffset + 3, 1)[0];

            return (byte)(audioStreamId - FirstAudioStreamId);
        }

        protected override int GetAudioPacketHeaderSize(Stream readStream, long currentOffset)
        {
            return GetPesPayloadDataOffset(readStream, currentOffset, 7);
        }

        protected override int GetVideoPacketHeaderSize(Stream readStream, long currentOffset)
        {
            return GetPesPayloadDataOffset(readStream, currentOffset, 0xC);
        }
        private static int GetPesPayloadDataOffset(Stream readStream, long currentOffset, int fallbackHeaderSize)
        {
            // skip stuffing bytes
            int stuffingByteCount = 0;
            readStream.Position = currentOffset + 6;

            while (readStream.ReadByte() == 0xFF)
            {
                stuffingByteCount++;
            }

            // parse the PES optional header after the stuffing bytes
            byte[] pesBytes = ParseFile.ParseSimpleOffset(readStream, currentOffset + 6 + stuffingByteCount, 12);
            int cursor = 0;

            if ((pesBytes.Length >= 2) && ((pesBytes[0] & 0xC0) == 0x40))
            {
                cursor = 2;

                if (cursor < pesBytes.Length)
                {
                    byte marker = pesBytes[cursor];

                    if ((marker & 0xF0) == 0x20)
                    {
                        cursor += 5;   // PTS only
                    }
                    else if ((marker & 0xF0) == 0x30)
                    {
                        cursor += 10;  // PTS and DTS
                    }
                    else if (marker == 0x0F)
                    {
                        cursor += 1;
                    }
                    else if ((marker & 0xC0) == 0x80)
                    {
                        if ((cursor + 3) <= pesBytes.Length)
                        {
                            cursor += 3 + pesBytes[cursor + 2];
                        }
                    }
                }
            }

            // truncated packet: keep the legacy fixed size
            if ((currentOffset + 6 + stuffingByteCount + cursor) > readStream.Length)
            {
                return stuffingByteCount + fallbackHeaderSize;
            }

            return stuffingByteCount + cursor;
        }

        protected override string GetAudioFileExtension(Stream readStream, long currentOffset)
        {
            string fileExtension;
            byte[] checkBytes, checkBytesAc3;

            int headerSize = this.GetAudioPacketHeaderSize(readStream, currentOffset);
            checkBytes = ParseFile.ParseSimpleOffset(readStream, (currentOffset + 6 + headerSize), 4);

            if (ParseFile.CompareSegment(checkBytes, 0, AixSignatureBytes))
            {
                fileExtension = AixAudioExtension;
            }
            else if (checkBytes[0] == 0x80)
            {
                fileExtension = AdxAudioExtension;
            }
            else
            {
                checkBytesAc3 = ParseFile.ParseSimpleOffset(readStream, (currentOffset + 6 + headerSize), 2);

                if (ParseFile.CompareSegment(checkBytesAc3, 0, Ac3SignatureBytes))
                {
                    fileExtension = Ac3AudioExtension;
                }
                else
                {
                    fileExtension = ".bin";
                }
            }

            return fileExtension;
        }
    }
}