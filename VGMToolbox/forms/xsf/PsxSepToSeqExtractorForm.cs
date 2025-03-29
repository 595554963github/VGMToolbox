using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.xsf;

namespace VGMToolbox.forms.xsf
{
    public partial class PsxSepToSeqExtractorForm : AVgmtForm
    {
        public PsxSepToSeqExtractorForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            InitializeComponent();

            this.grpSource.AllowDrop = true;

            // this.grpSource.Text
            this.lblTitle.Text = "从SEP文件中提取音序";
            this.tbOutput.Text = "将PSX SEP格式文件拆分为音序文件.";
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new PsxSepToSeqExtractorWorker();
        }
        protected override string getCancelMessage()
        {
            return "从SEP中提取SEQ… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "从SEP中提取SEQ… 完成";
        }
        protected override string getBeginMessage()
        {
            return "从SEP中提取SEQ… 开始";
        }

        private void grpSource_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            PsxSepToSeqExtractorWorker.PsxSepToSeqExtractorStruct bwStruct = new PsxSepToSeqExtractorWorker.PsxSepToSeqExtractorStruct();
            bwStruct.SourcePaths = s;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
