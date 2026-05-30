using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using VGMToolbox.util;

namespace VGMToolbox.format
{
    public class CriAfs2File
    {
        public ushort CueId { set; get; }
        public long FileOffsetRaw { set; get; }
        public long FileOffsetByteAligned { set; get; }
        public long FileLength { set; get; }

        // public string FileName { set; get; } // comes from ACB for now, maybe included in archive later?
    }

    public class CriAfs2Archive
    {
        public static readonly byte[] SIGNATURE = new byte[] { 0x41, 0x46, 0x53, 0x32 };
        public const string EXTRACTION_FOLDER_FORMAT = "{0}_Extract";

        public string SourceFile { set; get; }
        public byte[] MagicBytes { set; get; }
        public byte[] Version { set; get; }
        public uint FileCount { set; get; }
        public uint ByteAlignment { set; get; }
        public Dictionary<ushort, CriAfs2File> Files { set; get; }

        public CriAfs2Archive(FileStream fs, long offset)
        {
            ushort previousCueId = ushort.MaxValue;

            if (IsCriAfs2Archive(fs, offset))
            {
                this.SourceFile = fs.Name;
                long afs2FileSize = fs.Length;

                this.MagicBytes = ParseFile.ParseSimpleOffset(fs, offset, SIGNATURE.Length);
                this.Version = ParseFile.ParseSimpleOffset(fs, offset + 4, 4);
                this.FileCount = ParseFile.ReadUintLE(fs, offset + 8);

                // setup offset field size
                int offsetFieldSize = this.Version[1]; // known values: 2 and 4.  4 is most common.  I've only seen 2 in 'se_enemy_gurdon_galaga_bee.acb' from Sonic Lost World.
                uint offsetMask = 0;

                for (int j = 0; j < offsetFieldSize; j++)
                {
                    offsetMask |= (uint)((byte)0xFF << (j * 8));
                }

                if (this.FileCount > ushort.MaxValue)
                {
                    throw new FormatException(String.Format("错误,文件计数超过ushort的最大值,请在官方反馈论坛上报告此事(查看'其他'菜单项).", fs.Name));
                }

                this.ByteAlignment = ParseFile.ReadUshortLE(fs, offset + 0xC);
                // maybe ushort? 0x172e0020 in "Yurucamp Have a nice day" and 0x2a760020 "Summertime Render Another Horizon", others 0x20

                this.Files = new Dictionary<ushort, CriAfs2File>((int)this.FileCount);

                CriAfs2File dummy;

                for (ushort i = 0; i < this.FileCount; i++)
                {
                    dummy = new CriAfs2File();

                    //CueID is ushort if byte in 0x06 is 02, uint if 04
                    switch (this.Version[2])
                    {
                        case 4:
                            dummy.CueId = (ushort)ParseFile.ReadUintLE(fs, offset + (0x10 + (this.Version[2] * i)));
                            break;
                        case 2:
                        default:
                            dummy.CueId = ParseFile.ReadUshortLE(fs, offset + (0x10 + (this.Version[2] * i)));
                            break;
                    }
                    dummy.FileOffsetRaw = ParseFile.ReadUintLE(fs, offset + (0x10 + (this.FileCount * this.Version[2]) + (offsetFieldSize * i)));

                    // mask off unneeded info
                    dummy.FileOffsetRaw &= offsetMask;

                    // add offset
                    dummy.FileOffsetRaw += offset;  // for AFS2 files inside of other files (ACB, etc.)

                    // set file offset to byte alignment
                    if ((dummy.FileOffsetRaw % this.ByteAlignment) != 0)
                    {
                        dummy.FileOffsetByteAligned = MathUtil.RoundUpToByteAlignment(dummy.FileOffsetRaw, this.ByteAlignment);
                    }
                    else
                    {
                        dummy.FileOffsetByteAligned = dummy.FileOffsetRaw;
                    }

                    //---------------
                    // set file size
                    //---------------
                    // last file will use final offset entry
                    if (i == this.FileCount - 1)
                    {
                        dummy.FileLength = (ParseFile.ReadUintLE(fs, offset + (0x10 + (this.FileCount * this.Version[2]) + ((offsetFieldSize) * i)) + offsetFieldSize) + offset) - dummy.FileOffsetByteAligned;
                    }

                    // else set length for previous cue id
                    if (previousCueId != ushort.MaxValue)
                    {
                        this.Files[previousCueId].FileLength = dummy.FileOffsetRaw - this.Files[previousCueId].FileOffsetByteAligned;
                    }

                    this.Files.Add(dummy.CueId, dummy);
                    previousCueId = dummy.CueId;
                } // for (ushort i = 0; i < this.FileCount; i++)

            }
            else
            {
                throw new FormatException(String.Format("在偏移处找不到AFS2魔数:0x{0}.", offset.ToString("X8")));
            }
        }

        public static bool IsCriAfs2Archive(FileStream fs, long offset)
        {
            bool ret = false;
            byte[] checkBytes = ParseFile.ParseSimpleOffset(fs, offset, SIGNATURE.Length);

            if (ParseFile.CompareSegment(checkBytes, 0, SIGNATURE))
            {
                ret = true;
            }

            return ret;
        }

        public void ExtractAll()
        {
            string baseExtractionFolder = Path.Combine(Path.GetDirectoryName(this.SourceFile),
                                                       String.Format(EXTRACTION_FOLDER_FORMAT, Path.GetFileNameWithoutExtension(this.SourceFile)));

            this.ExtractAllRaw(baseExtractionFolder);
        }

        public void ExtractAllRaw(string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            string baseFileName = Path.GetFileNameWithoutExtension(this.SourceFile);

            using (FileStream fs = File.Open(this.SourceFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int fileIndex = 1;

                foreach (ushort key in this.Files.Keys)
                {
                    CriAfs2File file = this.Files[key];

                    byte[] fileHeader = ParseFile.ParseSimpleOffset(fs, (long)file.FileOffsetByteAligned, 12);
                    string extension = GetFileExtension(fileHeader);
                    if (extension == "bin")
                    {

                        string fileName = $"{baseFileName}_{fileIndex:D2}.{extension}";
                        string outputPath = Path.Combine(destinationFolder, fileName);

                        ParseFile.ExtractChunkToFile64(fs,
                            (ulong)file.FileOffsetByteAligned,
                            (ulong)file.FileLength,
                            outputPath, false, false);
                    }
                    else
                    {
                        string fileName = $"{baseFileName}_{fileIndex:D2}.{extension}";
                        string outputPath = Path.Combine(destinationFolder, fileName);

                        ParseFile.ExtractChunkToFile64(fs,
                            (ulong)file.FileOffsetByteAligned,
                            (ulong)file.FileLength,
                            outputPath, false, false);
                    }

                    fileIndex++;
                }
            }
        }

        private string GetFileExtension(byte[] fileHeader)
        {
            if (fileHeader.Length >= 8)
            {
                if (fileHeader[0] == 0xC8 && fileHeader[1] == 0xC3 &&
                    fileHeader[2] == 0xC1 && fileHeader[3] == 0x00 &&
                    fileHeader[4] == 0x03 && fileHeader[5] == 0x00 &&
                    fileHeader[6] == 0x00 && fileHeader[7] == 0x60)
                {
                    return "hca";
                }
            }

            if (fileHeader.Length >= 4)
            {
                if (fileHeader[0] == 0x48 && fileHeader[1] == 0x43 && fileHeader[2] == 0x41 && fileHeader[3] == 0x00)
                {
                    return "hca";
                }
                else if (fileHeader[0] == 0x52 && fileHeader[1] == 0x49 && fileHeader[2] == 0x46 && fileHeader[3] == 0x46)
                {
                    return "at3";
                }
                else if (fileHeader.Length >= 10)
                {
                    if (fileHeader[0] == 0x80 && fileHeader[1] == 0x00)
                    {
                        byte[] adxSignature1 = { 0x03, 0x12, 0x04, 0x01, 0x00, 0x00 };
                        byte[] adxSignature2 = { 0x03, 0x12, 0x04, 0x02, 0x00, 0x00 };

                        bool isAdxSignature1 = true;
                        bool isAdxSignature2 = true;

                        for (int i = 0; i < 6; i++)
                        {
                            if (fileHeader[4 + i] != adxSignature1[i])
                                isAdxSignature1 = false;
                            if (fileHeader[4 + i] != adxSignature2[i])
                                isAdxSignature2 = false;
                        }

                        if (isAdxSignature1 || isAdxSignature2)
                        {
                            return "adx";
                        }
                    }
                }
            }

            return "bin";
        }
    }
}
