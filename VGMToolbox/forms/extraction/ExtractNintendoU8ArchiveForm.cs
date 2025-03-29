using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class ExtractNintendoU8ArchiveForm : AVgmtForm
    {
        public ExtractNintendoU8ArchiveForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "任天堂U8解包器";
            this.tbOutput.Text = "解压任天堂WII平台U8文件.";

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            InitializeComponent();
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new ExtractNintendoU8ArchiveWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取U8文件内容...已取消.";
        }
        protected override string getCompleteMessage()
        {
            return "提取U8文件内容...完成.";
        }
        protected override string getBeginMessage()
        {
            return "提取U8文件内容...开始.";
        }

        private void ExtractNintendoU8ArchiveForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            ExtractNintendoU8ArchiveWorker.U8ArchiveUnpackerStruct taskStruct = new ExtractNintendoU8ArchiveWorker.U8ArchiveUnpackerStruct();
            taskStruct.SourcePaths = s;

            base.backgroundWorker_Execute(taskStruct);
        }
    }
}
