using System;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class CriAcbAwbExtractorForm : AVgmtForm
    {
        public CriAcbAwbExtractorForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "CRI ACB/AWB档案提取器";
            this.tbOutput.Text = "从CRI ACB/AWB档案中提取文件" + Environment.NewLine;
            this.tbOutput.Text += "对于ACB/AWB对,只需要删除ACB." + Environment.NewLine;
            this.tbOutput.Text += "如果您在提取过程中遇到问题,请先尝试使用VGMStream." + Environment.NewLine;

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            InitializeComponent();

            this.grpSourceFiles.AllowDrop = true;
            this.grpSourceFiles.Text = "在此处拖放.acb/.awb 文件.";
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new ExtractCriAcbAwbWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取文件… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "提取文件… 已完成";
        }
        protected override string getBeginMessage()
        {
            return "提取文件… 开始";
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExtractCriAcbAwbWorker.ExtractCriAcbAwbStruct bwStruct = new ExtractCriAcbAwbWorker.ExtractCriAcbAwbStruct();
            bwStruct.SourcePaths = s;
            bwStruct.IncludeCueIdInFileName = this.cbIncludeCueIdInFileName.Checked;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
