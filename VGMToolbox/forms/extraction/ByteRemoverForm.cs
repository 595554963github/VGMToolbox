using System;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class ByteRemoverForm : AVgmtForm
    {
        public ByteRemoverForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            InitializeComponent();

            this.lblTitle.Text = "字节移除器";
            this.tbOutput.Text = "从文件中删除字节.";

            this.rbEndAddress.Checked = true;
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new ByteRemoverWorker();
        }
        protected override string getCancelMessage()
        {
            return "删除字节… 已取消.";
        }
        protected override string getCompleteMessage()
        {
            return "删除字节… 已完成.";
        }
        protected override string getBeginMessage()
        {
            return "删除字节… 开始.";
        }

        private void doEndAddressRadios()
        {
            this.tbEndAddress.Enabled = this.rbEndAddress.Checked;
            this.tbLength.Enabled = this.rbLength.Checked;
        }
        private void rbEndAddress_CheckedChanged(object sender, EventArgs e)
        {
            doEndAddressRadios();
        }
        private void rbLength_CheckedChanged(object sender, EventArgs e)
        {
            doEndAddressRadios();
        }

        private void ByteRemoverForm_DragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }
        private void ByteRemoverForm_DragDrop(object sender, DragEventArgs e)
        {
            if (this.areInputsValid())
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                ByteRemoverWorker.ByteRemoverStruct taskStruct = new ByteRemoverWorker.ByteRemoverStruct();

                taskStruct.UseEndAddress = this.rbEndAddress.Checked;
                taskStruct.UseLength = this.rbLength.Checked;
                taskStruct.UseFileEnd = this.rbCutToEof.Checked;
                taskStruct.StartOffset = this.tbStartAddress.Text;
                taskStruct.EndOffset = this.tbEndAddress.Text;
                taskStruct.Length = this.tbLength.Text;
                taskStruct.SourcePaths = s;

                base.backgroundWorker_Execute(taskStruct);
            }
        }

        private bool areInputsValid()
        {
            bool isValid = true;

            // start address
            isValid &= AVgmtForm.checkTextBox(this.tbStartAddress.Text, this.lblStartAddress.Text);

            if (isValid)
            {
                isValid &= AVgmtForm.checkIfTextIsParsableAsLong(this.tbStartAddress.Text, this.lblStartAddress.Text);
            }

            // end address
            if (this.rbEndAddress.Checked)
            {
                isValid &= AVgmtForm.checkTextBox(this.tbEndAddress.Text, this.rbEndAddress.Text);

                if (isValid)
                {
                    isValid &= AVgmtForm.checkIfTextIsParsableAsLong(this.tbEndAddress.Text, this.rbEndAddress.Text);
                }
            }
            else if (this.rbLength.Checked)
            {
                isValid &= AVgmtForm.checkTextBox(this.tbLength.Text, this.rbLength.Text);

                if (isValid)
                {
                    isValid &= AVgmtForm.checkIfTextIsParsableAsLong(this.tbLength.Text, this.rbLength.Text);
                }
            }

            return isValid;
        }
    }
}
