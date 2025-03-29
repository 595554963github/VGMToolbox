using System;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.xsf;

namespace VGMToolbox.forms.xsf
{
    public partial class PspSeqDataFinderForm : AVgmtForm
    {
        public PspSeqDataFinderForm(TreeNode pTreeNode) :
            base(pTreeNode)
        {
            InitializeComponent();

            this.btnDoTask.Hide();
            this.grpSource.AllowDrop = true;

            this.lblTitle.Text = "PSP序列数据查找器";
            this.tbOutput.Text = "- 从文件中提取MID/PHD/PBD数据." + Environment.NewLine;
            this.tbOutput.Text += "- PHD/PBD应始终正确配对." + Environment.NewLine;
            this.tbOutput.Text += "- 注意：PBD检测可能非常慢,请耐心等待..." + Environment.NewLine;
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new PspSeqDataFinderWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取PSP数据… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "提取PSP数据… 完成";
        }
        protected override string getBeginMessage()
        {
            return "提取PSP数据… 开始";
        }

        private void grpSource_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            PspSeqDataFinderWorker.PspSeqDataFinderStruct bwStruct = new PspSeqDataFinderWorker.PspSeqDataFinderStruct();
            bwStruct.SourcePaths = s;
            bwStruct.ReorderMidFiles = cbReorderMidFiles.Checked;
            bwStruct.UseZeroOffsetForPbd = cb00ByteAligned.Checked;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
