using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using VGMToolbox.format.sdat;
using VGMToolbox.format.util;
using VGMToolbox.plugin;
using VGMToolbox.util;

namespace VGMToolbox.tools.xsf
{
    class Mk2sfWorker : BackgroundWorker, IVgmtBackgroundWorker
    {
        public const string TESTPACK_PATH = "external\\2sf\\testpack.nds";
        public const string TESTPACK_CRC32 = "FB16DF0E";
        public readonly string TWOSFTOOL_PATH =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "external\\2sf\\2sftool.exe");
        public readonly string TESTPACK_FULL_PATH =
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            TESTPACK_PATH);

        VGMToolbox.util.ProgressStruct progressStruct = new VGMToolbox.util.ProgressStruct();        
        
        public struct Mk2sfStruct
        {
            public ArrayList AllowedSequences;
            public ArrayList UnAllowedSequences;

            public string DestinationFolder;
            public string SourcePath;
            public string GameSerial;

            public string TagArtist;
            public string TagCopyright;
            public string TagYear;
            public string TagGame;

            public VolumeChangeStruct[] VolumeChangeList;
        }

        public struct VolumeChangeStruct
        {
            public int oldValue;
            public int newValue;
        }

        public Mk2sfWorker() 
        {
            this.progressStruct = new VGMToolbox.util.ProgressStruct();

            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;        
        }

        private void Make2sfFiles(Mk2sfStruct pMk2sfStruct)
        {
            string sdatDestinationPath;
            string TwoSFDestinationPath;
            string sdatPrefix;
            string testpackDestinationPath;
            string unallowedDestinationPath;
            string strmDestinationPath;
            Sdat sdat;

            // Build Paths
            if (String.IsNullOrEmpty(pMk2sfStruct.GameSerial))
            {
                sdatDestinationPath =
                    Path.Combine(pMk2sfStruct.DestinationFolder, Path.GetFileName(pMk2sfStruct.SourcePath));
            }
            else
            {
                sdatDestinationPath =
                    Path.Combine(pMk2sfStruct.DestinationFolder, pMk2sfStruct.GameSerial + Path.GetExtension(pMk2sfStruct.SourcePath));            
            }
            TwoSFDestinationPath =
                Path.Combine(pMk2sfStruct.DestinationFolder, Path.GetFileNameWithoutExtension(sdatDestinationPath));
            sdatPrefix = Path.GetFileNameWithoutExtension(sdatDestinationPath);
            testpackDestinationPath = Path.Combine(pMk2sfStruct.DestinationFolder, 
                Path.GetFileName(TESTPACK_FULL_PATH));
            unallowedDestinationPath = Path.Combine(TwoSFDestinationPath, "禁止序列");
            strmDestinationPath = TwoSFDestinationPath;

            // Copy SDAT to destination folder
            try
            {
                File.Copy(pMk2sfStruct.SourcePath, sdatDestinationPath, false);
            }
            catch (Exception sdatException)
            {
                throw new IOException(String.Format("错误：无法复制SDAT<{0}>到目标目录:{1}.", sdatDestinationPath, sdatException.Message));
            }
            
            // Copy STRMs
            this.progressStruct.Clear();
            this.progressStruct.GenericMessage = "正在复制STRM文件" + Environment.NewLine;
            ReportProgress(Constants.ProgressMessageOnly, progressStruct);
            
            using (FileStream sdatStream = File.OpenRead(sdatDestinationPath))
            {
                sdat = new Sdat();
                sdat.Initialize(sdatStream, sdatDestinationPath);
                sdat.ExtractStrms(sdatStream, strmDestinationPath);
            }

            // Update Volume
            for (int i = 0; i < pMk2sfStruct.VolumeChangeList.Length; i++)
            {
                if (pMk2sfStruct.VolumeChangeList[i].newValue != pMk2sfStruct.VolumeChangeList[i].oldValue)
                {
                    sdat.UpdateSseqVolume(i, pMk2sfStruct.VolumeChangeList[i].newValue);
                }
            }


            // Optimize SDAT
            this.progressStruct.Clear();
            this.progressStruct.GenericMessage = "优化SDAT文件" + Environment.NewLine;
            ReportProgress(Constants.ProgressMessageOnly, progressStruct);

            using (FileStream sdatStream = File.OpenRead(sdatDestinationPath))
            {
                sdat = new Sdat();
                sdat.Initialize(sdatStream, sdatDestinationPath);                
            }
            sdat.OptimizeForZlib(pMk2sfStruct.AllowedSequences);

            // Copy testpack.nds
            File.Copy(TESTPACK_FULL_PATH, testpackDestinationPath, true);

            // Create 2SF output path
            if (!Directory.Exists(TwoSFDestinationPath))
            {
                Directory.CreateDirectory(TwoSFDestinationPath);
            }

            // Build 2SFs
            this.progressStruct.Clear();
            this.progressStruct.GenericMessage = "创建2SFs" + Environment.NewLine;
            ReportProgress(Constants.ProgressMessageOnly, progressStruct);

            XsfUtil.Make2sfSet(testpackDestinationPath, sdatDestinationPath,
                GetMinAllowedSseq(pMk2sfStruct.AllowedSequences),
                GetMaxAllowedSseq(pMk2sfStruct.AllowedSequences), TwoSFDestinationPath);

            // Move unallowed Sequences
            string unallowedFileName;
            string unallowedFilePath;
            foreach (int unallowedSequenceNumber in pMk2sfStruct.UnAllowedSequences)
            {
                unallowedFileName = String.Format("{0}-{1}.mini2sf", sdatPrefix, unallowedSequenceNumber.ToString("x4"));
                unallowedFilePath = Path.Combine(TwoSFDestinationPath, unallowedFileName);

                if (!Directory.Exists(unallowedDestinationPath))
                {
                    Directory.CreateDirectory(unallowedDestinationPath);
                }

                if (File.Exists(unallowedFilePath))
                {
                    File.Copy(unallowedFilePath, Path.Combine(unallowedDestinationPath, unallowedFileName), true);
                    File.Delete(unallowedFilePath);
                }                
            }

            // Add Tags            
            this.progressStruct.Clear();
            this.progressStruct.GenericMessage = "标记输出T" + Environment.NewLine;
            ReportProgress(Constants.ProgressMessageOnly, progressStruct);
            
            XsfBasicTaggingStruct tagStruct = new XsfBasicTaggingStruct();
            tagStruct.TagArtist = pMk2sfStruct.TagArtist;
            tagStruct.TagCopyright = pMk2sfStruct.TagCopyright;
            tagStruct.TagYear = pMk2sfStruct.TagYear;
            tagStruct.TagGame = pMk2sfStruct.TagGame;
            tagStruct.TagComment = "使用Ys的遗产：Caitsith2破解的Book II驱动程序";            
            tagStruct.TagXsfByTagName = "-2sfby";
            tagStruct.TagXsfByTagValue = "VGMToolbox";

            string taggingBatchPath = XsfUtil.BuildBasicTaggingBatch(TwoSFDestinationPath, tagStruct, "*.mini2sf");
            XsfUtil.ExecutePsfPointBatchScript(taggingBatchPath, true);

            // Time 2SFs
            this.progressStruct.Clear();
            this.progressStruct.GenericMessage = "定时输出" + Environment.NewLine;
            ReportProgress(Constants.ProgressMessageOnly, progressStruct);
            
            string outputTimerMessages;
            
            Time2sfStruct timerStruct = new Time2sfStruct();
            timerStruct.DoSingleLoop = false;
            timerStruct.Mini2sfDirectory = TwoSFDestinationPath;
            timerStruct.SdatPath = pMk2sfStruct.SourcePath;

            XsfUtil.Time2sfFolder(timerStruct, out outputTimerMessages);

            // Delete Files
            this.progressStruct.Clear();
            this.progressStruct.GenericMessage = "清理" + Environment.NewLine;
            ReportProgress(Constants.ProgressMessageOnly, progressStruct);
            
            if (File.Exists(sdatDestinationPath))
            {
                File.Delete(sdatDestinationPath);
            }
            if (File.Exists(testpackDestinationPath))
            {
                File.Delete(testpackDestinationPath);
            }
        }

        private int GetMinAllowedSseq(ArrayList pAllowedSequences)
        {
            int ret = int.MaxValue;
            int checkVal;

            foreach (object o in pAllowedSequences)
            { 
                checkVal = (int) o;

                if (checkVal < ret)
                {
                    ret = checkVal;
                }
            }
            return ret;
        }
        private int GetMaxAllowedSseq(ArrayList pAllowedSequences)
        {
            int ret = int.MinValue;
            int checkVal;

            foreach (object o in pAllowedSequences)
            {
                checkVal = (int)o;

                if (checkVal > ret)
                {
                    ret = checkVal;
                }
            }
            return ret;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            Mk2sfStruct mk2sfStruct = (Mk2sfStruct)e.Argument;

            try
            {
                Make2sfFiles(mk2sfStruct);
            }
            catch (Exception _ex)
            {
                this.progressStruct.Clear();
                this.progressStruct.ErrorMessage = String.Format("创建2sf文件时出错:{0}{1}", _ex.Message, Environment.NewLine);
                ReportProgress(0, this.progressStruct);
            }
        }
    }
}
