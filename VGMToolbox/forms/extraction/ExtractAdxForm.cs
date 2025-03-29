using System;
using System.Configuration;
using System.Windows.Forms;
using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class ExtractAdxForm : AVgmtForm
    {
        public ExtractAdxForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "CRI ADX提取器";
            this.tbOutput.Text = "提取CRI ADX数据" + Environment.NewLine;

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            InitializeComponent();

            this.grpSourceFiles.AllowDrop = true;
            this.grpSourceFiles.Text = ConfigurationManager.AppSettings["Form_Global_DropSourceFiles"];
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new ExtractAdxWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取 CRI ADX… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "提取 CRI ADX… 已完成";
        }
        protected override string getBeginMessage()
        {
            return "提取 CRI ADX… 开始";
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExtractAdxWorker.ExtractAdxStruct bwStruct = new ExtractAdxWorker.ExtractAdxStruct();
            bwStruct.SourcePaths = s;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
