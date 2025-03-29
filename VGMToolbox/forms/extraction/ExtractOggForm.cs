using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class ExtractOggForm : AVgmtForm
    {
        public ExtractOggForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "Xiph.Org OGG提取器";
            this.tbOutput.Text = "提取Xiph.Org OGG数据" + Environment.NewLine;

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            InitializeComponent();

            //this.grpSourceFiles.AllowDrop = true;
            this.grpSourceFiles.Text = ConfigurationManager.AppSettings["Form_Global_DropSourceFiles"];
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new ExtractOggWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取OGGs...已取消";
        }
        protected override string getCompleteMessage()
        {
            return "提取OGGs...完成";
        }
        protected override string getBeginMessage()
        {
            return "提取OGGs...开始";
        }

        private void ExtractOggForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExtractOggWorker.ExtractOggStruct bwStruct = new ExtractOggWorker.ExtractOggStruct();
            bwStruct.SourcePaths = s;
            bwStruct.StopParsingOnFormatError = cbStopParsingOnError.Checked;

            base.backgroundWorker_Execute(bwStruct);
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExtractOggWorker.ExtractOggStruct bwStruct = new ExtractOggWorker.ExtractOggStruct();
            bwStruct.SourcePaths = s;
            bwStruct.StopParsingOnFormatError = cbStopParsingOnError.Checked;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
