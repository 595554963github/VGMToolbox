using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using VGMToolbox.util;

namespace mkpsf2fe
{
    public partial class Form1 : Form
    {
        MkPsf2Worker mkPsf2Worker;
        
        public Form1()
        {                        
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            tbOutput.Clear();
            toolStripStatusLabel1.Text = "创建PSF2s...开始";

            MkPsf2Worker.MkPsf2Struct mkStruct = new MkPsf2Worker.MkPsf2Struct();
            mkStruct.sourcePath = tbSourceDirectory.Text;
            mkStruct.modulePath = tbModulesDirectory.Text;

            mkStruct.depth = tbDepth.Text;
            mkStruct.reverb = tbReverb.Text;
            mkStruct.tempo = tbTempo.Text;
            mkStruct.tickInterval = tbTickInterval.Text;
            mkStruct.volume = tbVolume.Text;

            mkPsf2Worker = new MkPsf2Worker();
            mkPsf2Worker.ProgressChanged += backgroundWorker_ReportProgress;
            mkPsf2Worker.RunWorkerCompleted += MkPsf2Worker_WorkComplete;
            mkPsf2Worker.RunWorkerAsync(mkStruct);
        }

        private void MkPsf2Worker_WorkComplete(object sender,
             RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                toolStripStatusLabel1.Text = "创建PSF2s...已取消";
                tbOutput.Text += "行动取消.";
            }
            else
            {
                toolStripStatusLabel1.Text = "创建PSF2s...完成";
            }
        }

        private void backgroundWorker_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != Constants.IgnoreProgress &&
                e.ProgressPercentage != Constants.ProgressMessageOnly)
            {
                toolStripProgressBar.Value = e.ProgressPercentage;
                this.Text = "mkpsf2FE [" + e.ProgressPercentage + "%]";
            }

            if ((e.ProgressPercentage == Constants.ProgressMessageOnly) && e.UserState != null)
            {
                VGMToolbox.util.ProgressStruct vProgressStruct = (VGMToolbox.util.ProgressStruct)e.UserState;
                tbOutput.Text += vProgressStruct.GenericMessage;
            }
            else if (e.UserState != null)
            {
                VGMToolbox.util.ProgressStruct vProgressStruct = (VGMToolbox.util.ProgressStruct)e.UserState;
                tbOutput.Text += vProgressStruct.ErrorMessage ?? String.Empty;
            }
        }

        private void btnSourceDirectoryBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbSourceDirectory.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (mkPsf2Worker != null && mkPsf2Worker.IsBusy)
            {
                mkPsf2Worker.CancelAsync();
            }
        }

        private void btnModulesDirectoryBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                tbModulesDirectory.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
