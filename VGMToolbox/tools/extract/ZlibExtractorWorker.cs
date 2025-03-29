using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

using VGMToolbox.plugin;
using VGMToolbox.util;

namespace VGMToolbox.tools.extract
{
    class ZlibExtractorWorker : AVgmtDragAndDropWorker, IVgmtBackgroundWorker
    {                
        public struct ZlibExtractorStruct : IVgmtWorkerStruct
        {
            public bool DoDecompress;
            public long StartingOffset;
            
            private string[] sourcePaths;
            public string[] SourcePaths
            {
                get { return sourcePaths; }
                set { sourcePaths = value; }
            }
        }

        public ZlibExtractorWorker() : base() { }

        protected override void DoTaskForFile(string pPath,
            IVgmtWorkerStruct pZlibExtractorStruct, DoWorkEventArgs e)
        {
            ZlibExtractorStruct zlibExtractorStruct =
                (ZlibExtractorStruct)pZlibExtractorStruct;

            this.progressStruct.Clear();

            if (zlibExtractorStruct.DoDecompress)
            {
                progressStruct.GenericMessage = String.Format("解压缩 <{0}>{1}", pPath, Environment.NewLine);
            }
            else
            {
                progressStruct.GenericMessage = String.Format("压缩 <{0}>{1}", pPath, Environment.NewLine);
            }
            ReportProgress(this.Progress, progressStruct);

            try
            {
                string outputFileName;
                
                if (zlibExtractorStruct.DoDecompress)
                {
                    outputFileName = Path.ChangeExtension(pPath, CompressionUtil.ZlibDecompressOutputExtension);
                }
                else
                {
                    outputFileName = Path.ChangeExtension(pPath, CompressionUtil.ZlibCompressOutputExtension);
                }
                
                using (FileStream fs = File.OpenRead(pPath))
                {
                    if (zlibExtractorStruct.StartingOffset > fs.Length)
                    {
                        throw new ArgumentOutOfRangeException("偏移量起始处","偏移量不能大于文件大小.");
                    }
                    
                    if (zlibExtractorStruct.DoDecompress)
                    {
                        CompressionUtil.DecompressZlibStreamToFile(fs, outputFileName, zlibExtractorStruct.StartingOffset);
                    }
                    else
                    {
                        CompressionUtil.CompressStreamToZlibFile(fs, outputFileName, zlibExtractorStruct.StartingOffset);
                    }
                }

                this.progressStruct.Clear();
                if (zlibExtractorStruct.DoDecompress)
                {
                    progressStruct.GenericMessage = String.Format("    {0} 已解压.{1}", Path.GetFileName(outputFileName), Environment.NewLine);
                }
                else
                {
                    progressStruct.GenericMessage = String.Format("    {0} 已压缩.{1}", Path.GetFileName(outputFileName), Environment.NewLine);
                }
                                
                ReportProgress(this.Progress, progressStruct);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
