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
    public partial class UnpackNintendoWadForm : AVgmtForm
    {
        public UnpackNintendoWadForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "任天堂WAD提取器";
            this.tbOutput.Text = "解压任天堂WII平台WAD文件.";

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
            return new UnpackNintendoWadWorker();
        }
        protected override string getCancelMessage()
        {
            return "提取WAD文件...已取消.";
        }
        protected override string getCompleteMessage()
        {
            return "提取WAD文件...已完成.";
        }
        protected override string getBeginMessage()
        {
            return "提取WAD文件...开始.";
        }

        private void UnpackNintendoWadForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            UnpackNintendoWadWorker.WadUnpackerStruct taskStruct = new UnpackNintendoWadWorker.WadUnpackerStruct();
            taskStruct.SourcePaths = s;
            taskStruct.UnpackExtractedU8Files = this.cbUnpackU8Archives.Checked;
            taskStruct.ExtractAllFiles = this.rbExtractAllSections.Checked;


            base.backgroundWorker_Execute(taskStruct);
        }
    }
}
