using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

using VGMToolbox.format;
using VGMToolbox.plugin;
using VGMToolbox.util;

namespace VGMToolbox.tools.extract
{
    class ExtractHcaWorker : AVgmtDragAndDropWorker, IVgmtBackgroundWorker
    {
        static readonly byte[] HCA_SIG_BYTES = new byte[] { 0x48, 0x43, 0x41, 0x00 };
        static readonly byte[] FMT_CHUNK_BYTES = new byte[] { 0x66, 0x6D, 0x74, 0x00 };
        static readonly byte[] DEC_CHUNK_BYTES = new byte[] { 0x64, 0x65, 0x63, 0x00 };
        static readonly byte[] COMP_CHUNK_BYTES = new byte[] { 0x63, 0x6F, 0x6D, 0x70 };
        static readonly byte[] MASK_BYTES = new byte[] { 0x7F, 0x7F, 0x7F, 0x7F };

        const long MAX_HEADER_SIZE = 0x20000; // just a guess, never seen more than 0x1000;

        public struct ExtractHcaStruct : IVgmtWorkerStruct
        {
            public string[] SourcePaths { set; get; }
        }

        public ExtractHcaWorker() :
            base()
        { }

        protected override void DoTaskForFile(string pPath, IVgmtWorkerStruct pExtractAdxStruct, DoWorkEventArgs e)
        {
            ExtractHcaStruct extractAdxStruct = (ExtractHcaStruct)pExtractAdxStruct;

            long offset = 0;

            byte revisionMajor;
            byte revisionMinor;
            ushort dataOffset;

            long fmtChunkOffset;

            uint blockCount;
            ushort blockSize;

            long fileSize;

            int fileCount = 1;
            string sourceFileName = Path.GetFileNameWithoutExtension(pPath);
            string outputPath = Path.Combine(Path.GetDirectoryName(pPath), sourceFileName);
            string outputFileName;
            string outputFilePath;

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            using (FileStream fs = File.Open(pPath, FileMode.Open, FileAccess.Read))
            {
                while ((offset = ParseFile.GetNextOffsetMasked(fs, offset, HCA_SIG_BYTES, MASK_BYTES)) > -1)
                {
                    if (this.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (offset % 0x10000 == 0 && this.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    // get version
                    revisionMajor = ParseFile.ReadByte(fs, offset + 4);
                    revisionMinor = ParseFile.ReadByte(fs, offset + 5);

                    // get data offset
                    dataOffset = ParseFile.ReadUshortBE(fs, offset + 6);

                    // get 'fmt' chunk offset
                    fmtChunkOffset = ParseFile.GetNextOffsetMasked(fs, offset, FMT_CHUNK_BYTES, MASK_BYTES);

                    if (fmtChunkOffset > -1)
                    {
                        // get block count
                        blockCount = ParseFile.ReadUintBE(fs, fmtChunkOffset + 8);

                        // get block size
                        blockSize = this.getBlockSize(fs, offset);

                        // calculate file size
                        fileSize = dataOffset + (blockCount * blockSize);

                        if (offset + fileSize > fs.Length)
                        {
                            fileSize = fs.Length - offset;
                        }

                        outputFileName = $"{sourceFileName}_{fileCount}.hca";
                        outputFilePath = Path.Combine(outputPath, outputFileName);

                        while (File.Exists(outputFilePath))
                        {
                            fileCount++;
                            outputFileName = $"{sourceFileName}_{fileCount}.hca";
                            outputFilePath = Path.Combine(outputPath, outputFileName);
                        }

                        ExtractChunkToFileFast(fs, offset, fileSize, outputFilePath);

                        // increment counter
                        fileCount++;

                        // move pointer
                        offset += fileSize;
                    }
                    else
                    {
                        offset += 4;
                    }
                }
            }
        }

        private ushort getBlockSize(Stream inStream, long hcaOffset)
        {
            ushort blockSize = 0;

            long decChunkOffset;
            long compChunkOffset;

            //----------------
            // 'dec ' offset
            //----------------

            // get 'dec' chunk offset, if exists (v1.3, maybe others?)
            decChunkOffset = ParseFile.GetNextOffsetWithLimitMasked(inStream, hcaOffset,
                hcaOffset + MAX_HEADER_SIZE, DEC_CHUNK_BYTES, MASK_BYTES, true);

            if (decChunkOffset > -1)
            {
                blockSize = ParseFile.ReadUshortBE(inStream, decChunkOffset + 4);
            }
            else
            {
                //----------------
                // 'comp' offset
                //----------------

                // get 'comp' chunk offset, if exists (v1.3, maybe others?)
                compChunkOffset = ParseFile.GetNextOffsetWithLimitMasked(inStream, hcaOffset,
                    hcaOffset + MAX_HEADER_SIZE, COMP_CHUNK_BYTES, MASK_BYTES, true);

                if (compChunkOffset > -1)
                {
                    blockSize = ParseFile.ReadUshortBE(inStream, compChunkOffset + 4);
                }
                else
                {

                    blockSize = 0x400;
                }
            }

            return blockSize;
        }

        private void ExtractChunkToFileFast(FileStream sourceStream, long sourceOffset, long chunkSize, string outputFilePath)
        {
            const int BUFFER_SIZE = 64 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];
            long bytesRemaining = chunkSize;

            using (FileStream outputStream = File.Create(outputFilePath))
            {
                sourceStream.Seek(sourceOffset, SeekOrigin.Begin);

                while (bytesRemaining > 0)
                {
                    if (this.CancellationPending)
                        break;

                    int bytesToRead = (int)Math.Min(BUFFER_SIZE, bytesRemaining);
                    int bytesRead = sourceStream.Read(buffer, 0, bytesToRead);

                    if (bytesRead == 0)
                        break;

                    outputStream.Write(buffer, 0, bytesRead);
                    bytesRemaining -= bytesRead;
                }
            }
        }
    }
}
