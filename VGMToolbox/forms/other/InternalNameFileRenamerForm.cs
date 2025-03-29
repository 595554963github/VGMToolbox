using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.other;

namespace VGMToolbox.forms.other
{
    public partial class InternalNameFileRenamerForm : AVgmtForm
    {
        public InternalNameFileRenamerForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "内部名称文件重命名器";
            this.tbOutput.Text = "根据文件头中包含的名称重命名文件.";
            this.btnDoTask.Hide();

            InitializeComponent();

            this.rbNameLength.Checked = true;
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        private void InternalNameFileRenamerForm_DragDrop(object sender, DragEventArgs e)
        {
            if (this.validateAll())
            {
                InternalNameFileRenamerWorker.InternalNameFileRenamerWorkerStruct taskStruct =
                    new InternalNameFileRenamerWorker.InternalNameFileRenamerWorkerStruct();

                // paths
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                taskStruct.SourcePaths = s;

                // OUTPUT
                //if (!String.IsNullOrEmpty(this.tbOutputFolder.Text))
                //{
                //    taskStruct.OutputFolder = this.tbOutputFolder.Text.Trim();
                //}

                taskStruct.Offset = this.tbNameOffset.Text;
                taskStruct.NameLength = this.tbNameLength.Text;

                if (String.IsNullOrEmpty(this.tbTerminatorBytes.Text.Trim()))
                {
                    taskStruct.TerminatorBytes = null;
                }
                else
                {
                    taskStruct.TerminatorBytes = this.tbTerminatorBytes.Text;
                }

                taskStruct.MaintainOriginalExtension = this.cbMaintainExtension.Checked;

                base.backgroundWorker_Execute(taskStruct);
            }
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new InternalNameFileRenamerWorker();
        }
        protected override string getCancelMessage()
        {
            return "重命名文件… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "重命名文件… 完成";
        }
        protected override string getBeginMessage()
        {
            return "重命名文件… 开始";
        }

        private void doLengthRadioButtons()
        {
            this.tbNameLength.Enabled = this.rbNameLength.Checked;
            this.tbTerminatorBytes.Enabled = this.rbTerminatorBytes.Checked;
        }
        private void rbNameLength_CheckedChanged(object sender, EventArgs e)
        {
            this.doLengthRadioButtons();
        }
        private void rbTerminatorBytes_CheckedChanged(object sender, EventArgs e)
        {
            this.doLengthRadioButtons();
        }

        private bool validateAll()
        {
            bool isValid = true;

            isValid &= AVgmtForm.checkTextBox(this.tbNameOffset.Text, this.lblNameOffset.Text);

            if (this.rbNameLength.Checked)
            {
                isValid &= AVgmtForm.checkTextBox(this.tbNameLength.Text, this.rbNameLength.Text);
                AVgmtForm.checkIfTextIsParsableAsLong(this.tbNameLength.Text, this.rbNameLength.Text);
            }

            if (this.rbTerminatorBytes.Checked)
            {
                isValid &= AVgmtForm.checkTextBox(this.tbTerminatorBytes.Text, this.rbTerminatorBytes.Text);
                AVgmtForm.checkIfTextIsParsableAsLong("0x" + this.tbTerminatorBytes.Text, this.rbTerminatorBytes.Text);
            }

            return isValid;
        }

    }
}
