using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
//using System.Linq;
using System.Text;

using VGMToolbox.format;
using VGMToolbox.plugin;
using VGMToolbox.util;

namespace VGMToolbox.tools.extract
{
    class ExtractCriAcbAwbWorker : AVgmtDragAndDropWorker, IVgmtBackgroundWorker
    {
        public struct ExtractCriAcbAwbStruct : IVgmtWorkerStruct
        {
            public string[] SourcePaths { set; get; }
            public bool IncludeCueIdInFileName { set; get; }
        }

        public ExtractCriAcbAwbWorker() :
            base() { }

        protected override void DoTaskForFile(string pPath, IVgmtWorkerStruct pExtractStruct, DoWorkEventArgs e)
        {
            ExtractCriAcbAwbStruct extractStruct = (ExtractCriAcbAwbStruct)pExtractStruct;
            byte[] magicBytes;
            long awbOffset = 0;

            using (FileStream fs = File.Open(pPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                magicBytes = ParseFile.ParseSimpleOffset(fs, 0, 4);

                this.progressStruct.Clear();

                // ACB
                if (ParseFile.CompareSegment(magicBytes, 0, CriAcbFile.SIGNATURE_BYTES))
                {
                    this.progressStruct.GenericMessage = String.Format("处理ACB文件:'{0}'.{1}", Path.GetFileName(pPath), Environment.NewLine);
                    ReportProgress(Constants.ProgressMessageOnly, this.progressStruct);

                    CriAcbFile acb = new CriAcbFile(fs, 0, extractStruct.IncludeCueIdInFileName);
                    acb.ExtractAll();
                }
                else if (ParseFile.CompareSegment(magicBytes, 0, CriAfs2Archive.SIGNATURE))
                {
                    this.progressStruct.GenericMessage = String.Format("处理AWB文件:'{0}'.{1}", Path.GetFileName(pPath), Environment.NewLine);
                    ReportProgress(Constants.ProgressMessageOnly, this.progressStruct);

                    CriAfs2Archive afs2 = new CriAfs2Archive(fs, 0);
                    afs2.ExtractAll();
                }
                else
                {
                    this.progressStruct.GenericMessage = String.Format("在偏移处找不到ACB/AWB签名 0...扫描AWB签名:'{0}'.{1}", Path.GetFileName(pPath), Environment.NewLine);
                    ReportProgress(Constants.ProgressMessageOnly, this.progressStruct);

                    awbOffset = ParseFile.GetNextOffset(fs, 0, CriAfs2Archive.SIGNATURE);

                    if (awbOffset > 0)
                    {
                        CriAfs2Archive afs2 = new CriAfs2Archive(fs, awbOffset);
                        afs2.ExtractAll();                    
                    }
                    else
                    {
                        this.progressStruct.GenericMessage = String.Format("文件不是ACB或AWB…跳过:'{0}'.{1}", Path.GetFileName(pPath), Environment.NewLine);
                        ReportProgress(Constants.ProgressMessageOnly, this.progressStruct);
                    }
                }
            }            
        }        
    }
}
