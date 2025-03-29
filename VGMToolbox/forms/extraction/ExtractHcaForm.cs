using System;
using System.Configuration;
using System.Windows.Forms;
using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class ExtractHcaForm : AVgmtForm
    {
        public ExtractHcaForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "CRI HCA提取器";
            this.tbOutput.Text = "提取CRI HCA数据" + Environment.NewLine;

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
            return new ExtractHcaWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取CRI HCA...已取消";
        }
        protected override string getCompleteMessage()
        {
            return "提取CRI HCA...完成";
        }
        protected override string getBeginMessage()
        {
            return "提取CRI HCA...开始";
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExtractHcaWorker.ExtractHcaStruct bwStruct = new ExtractHcaWorker.ExtractHcaStruct();
            bwStruct.SourcePaths = s;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
