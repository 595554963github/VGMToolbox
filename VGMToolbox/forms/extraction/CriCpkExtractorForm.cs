using System;
using System.Configuration;
using System.Windows.Forms;
using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class CriCpkExtractorForm : AVgmtForm
    {
        public CriCpkExtractorForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "CRI CPK档案提取器";
            this.tbOutput.Text = "从CRI CPK档案中提取文件" + Environment.NewLine;
            this.tbOutput.Text += "目前支持使用TOC和ITOC结构的加密和未加密CPK文件." + Environment.NewLine;

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
            return new ExtractCriCpkWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取cpk文件… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "提取cpk文件… 已完成";
        }
        protected override string getBeginMessage()
        {
            return "提取cpk文件… 开始";
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExtractCriCpkWorker.ExtractCriCpkStruct bwStruct = new ExtractCriCpkWorker.ExtractCriCpkStruct();
            bwStruct.SourcePaths = s;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
