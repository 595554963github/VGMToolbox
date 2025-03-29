using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;

using VGMToolbox.format;
using VGMToolbox.plugin;
using VGMToolbox.tools.xsf;

namespace VGMToolbox.forms.xsf
{
    public partial class Psf2SettingsUpdaterForm : AVgmtForm
    {
        public Psf2SettingsUpdaterForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            InitializeComponent();

            this.grpSourceFiles.AllowDrop = true;
            this.btnDoTask.Hide();

            this.lblTitle.Text = "PSF2设置更新程序";
            this.tbOutput.Text = "在批处理模式下更新PSF2的psf2.ini设置.";
        }

        private void grpSourceFiles_DragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new Psf2SettingsUpdaterWorker();
        }
        protected override string getCancelMessage()
        {
            return "更新PSF2设置… 已取消.";
        }
        protected override string getCompleteMessage()
        {
            return "更新PSF2设置… 完成.";
        }
        protected override string getBeginMessage()
        {
            return "更新PSF2设置… 开始.";
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Psf2SettingsUpdaterWorker.Psf2SettingsUpdaterStruct bwStruct = new Psf2SettingsUpdaterWorker.Psf2SettingsUpdaterStruct();
            Psf2.Psf2IniSqIrxStruct iniValues = new Psf2.Psf2IniSqIrxStruct();

            iniValues.SqFileName = this.tbSqFile.Text;
            iniValues.HdFileName = this.tbHdFile.Text;
            iniValues.BdFileName = this.tbBdFile.Text;

            iniValues.SequenceNumber = this.tbSequenceNumber.Text;
            iniValues.TimerTickInterval = this.tbTickInterval.Text;
            iniValues.Reverb = this.tbReverb.Text;
            iniValues.Depth = this.tbDepth.Text;
            iniValues.Tempo = this.tbTempo.Text;
            iniValues.Volume = this.tbVolume.Text;

            bwStruct.IniSettings = iniValues;
            bwStruct.SourcePaths = s;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
