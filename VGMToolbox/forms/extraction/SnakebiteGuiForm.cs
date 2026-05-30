using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class SnakebiteGuiForm : AVgmtForm
    {
        bool doDrag;

        private static string GetSafeConfigValue(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public SnakebiteGuiForm(TreeNode pTreeNode) : base(pTreeNode)
        {
            InitializeComponent();

            this.grpFiles.AllowDrop = true;
            this.doDrag = false;

            this.lblTitle.Text = GetSafeConfigValue("Form_SnakebiteGUI_Title", "简易切割器");
            this.tbOutput.Text = GetSafeConfigValue("Form_SnakebiteGUI_IntroText", "在此处拖放文件或使用浏览按钮");
            this.btnDoTask.Text = GetSafeConfigValue("Form_SnakebiteGUI_BtnDoTask", "执行任务");

            this.grpFiles.Text = GetSafeConfigValue("Form_SnakebiteGUI_GrpFiles", "文件");
            this.lblSourceFiles.Text = GetSafeConfigValue("Form_SnakebiteGUI_LblSourceFiles", "源文件");
            this.lblDragNDrop.Text = GetSafeConfigValue("Form_SnakebiteGUI_LblDragNDrop", "拖放文件");
            this.groupOutputMode.Text = GetSafeConfigValue("Form_SnakebiteGUI_GroupOutputMode", "输出模式");
            this.rbNameOutput.Text = GetSafeConfigValue("Form_SnakebiteGUI_RbNameOutput", "命名输出");
            this.rbAutoName.Text = GetSafeConfigValue("Form_SnakebiteGUI_RbAutoName", "自动命名");
            this.lblOutputFile.Text = GetSafeConfigValue("Form_SnakebiteGUI_LblOutputFile", "输出文件");
            this.lblFileExtension.Text = GetSafeConfigValue("Form_SnakebiteGUI_LblFileExtension", "文件扩展名");
            this.grpOptions.Text = GetSafeConfigValue("Form_SnakebiteGUI_GrpOptions", "选项");
            this.lblStartAddress.Text = GetSafeConfigValue("Form_SnakebiteGUI_LblStartAddress", "起始地址");
            this.rbEndAddress.Text = GetSafeConfigValue("Form_SnakebiteGUI_RbEndAddress", "结束地址");
            this.rbLength.Text = GetSafeConfigValue("Form_SnakebiteGUI_RbLength", "长度");
            this.rbEndOfFile.Text = GetSafeConfigValue("Form_SnakebiteGUI_RbEndOfFile", "文件结束");

            this.rbEndAddress.Checked = true;
        }

        private void btnBrowseSource_Click(object sender, EventArgs e)
        {
            this.tbSourceFiles.Text = base.browseForFile(sender, e);
        }
        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            this.tbOutputFile.Text = base.browseForFileToSave(sender, e);
        }

        private void setRadioButtons()
        {
            if (rbEndAddress.Checked)
            {
                tbEndAddress.Enabled = true;
                tbEndAddress.ReadOnly = false;

                tbLength.Enabled = false;
                tbLength.ReadOnly = true;
            }
            else if (rbLength.Checked)
            {
                tbEndAddress.Enabled = false;
                tbEndAddress.ReadOnly = true;

                tbLength.Enabled = true;
                tbLength.ReadOnly = false;
            }
            else if (rbEndOfFile.Checked)
            {
                tbEndAddress.Enabled = false;
                tbEndAddress.ReadOnly = true;

                tbLength.Enabled = false;
                tbLength.ReadOnly = true;
            }
        }
        private void rbEndAddress_CheckedChanged(object sender, EventArgs e)
        {
            this.setRadioButtons();
        }
        private void rbLength_CheckedChanged(object sender, EventArgs e)
        {
            this.setRadioButtons();
        }
        private void rbEndOfFile_CheckedChanged(object sender, EventArgs e)
        {
            this.setRadioButtons();
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new SimpleCutterSnakebiteWorker();
        }
        protected override string getCancelMessage()
        {
            return GetSafeConfigValue("Form_SnakebiteGUI_MessageCancel", "操作已取消");
        }
        protected override string getCompleteMessage()
        {
            return GetSafeConfigValue("Form_SnakebiteGUI_MessageComplete", "操作完成");
        }
        protected override string getBeginMessage()
        {
            return GetSafeConfigValue("Form_SnakebiteGUI_MessageBegin", "正在处理...");
        }

        private void tbSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            bool cutFiles = false;
            string warningMessage;

            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (((s.Length > 1) && (!this.rbAutoName.Checked)) ||
                ((s.Length == 1) && (Directory.Exists(s[0]))))
            {
                warningMessage =
                    GetSafeConfigValue("Form_SnakebiteGUI_ErrorSingleFile", "自动命名模式需要单个源文件，或者使用拖放模式");
                MessageBox.Show(warningMessage,
                    GetSafeConfigValue("Form_Global_ErrorWindowTitle", "错误"));
            }
            else
            {
                cutFiles = true;
            }

            if (cutFiles)
            {
                this.doDrag = true;
                this.cutTheFile(s);
            }
        }

        private void cutTheFile(string[] pPaths)
        {
            if (this.validateInputs(!this.doDrag))
            {
                this.doDrag = false;

                SimpleCutterSnakebiteWorker.SimpleCutterSnakebiteStruct snbStruct =
                    new SimpleCutterSnakebiteWorker.SimpleCutterSnakebiteStruct();

                snbStruct.EndAddress = this.tbEndAddress.Text;
                snbStruct.Length = this.tbLength.Text;
                snbStruct.OutputFile = this.tbOutputFile.Text;
                snbStruct.NewFileExtension = this.tbFileExtension.Text;
                snbStruct.SourcePaths = pPaths;
                snbStruct.StartOffset = this.tbStartAddress.Text;
                snbStruct.UseEndAddress = this.rbEndAddress.Checked;
                snbStruct.UseFileEnd = this.rbEndOfFile.Checked;
                snbStruct.UseLength = this.rbLength.Checked;

                base.backgroundWorker_Execute(snbStruct);
            }
        }

        private void btnDoTask_Click(object sender, EventArgs e)
        {
            string[] s = new string[] { this.tbSourceFiles.Text };
            this.doDrag = false;
            this.cutTheFile(s);
        }

        private bool validateInputs()
        {
            return validateInputs(true);
        }
        private bool validateInputs(bool pCheckInputFile)
        {
            bool ret = true;

            if (pCheckInputFile)
            {
                ret &= AVgmtForm.checkFileExists(this.tbSourceFiles.Text, this.lblSourceFiles.Text);
            }
            if (this.rbNameOutput.Checked)
            {
                ret &= AVgmtForm.checkTextBox(this.tbOutputFile.Text, this.rbNameOutput.Text);
            }
            if (this.rbAutoName.Checked)
            {
                ret &= AVgmtForm.checkTextBox(this.tbFileExtension.Text, this.rbAutoName.Text);
            }

            ret &= AVgmtForm.checkTextBox(this.tbStartAddress.Text, this.lblStartAddress.Text);

            if (rbEndAddress.Checked)
            {
                ret &= AVgmtForm.checkTextBox(this.tbEndAddress.Text, this.rbEndAddress.Text);
            }
            if (rbLength.Checked)
            {
                ret &= AVgmtForm.checkTextBox(this.tbLength.Text, this.rbLength.Text);
            }

            if (pCheckInputFile && (this.tbSourceFiles.Text.Equals(this.tbOutputFile.Text)))
            {
                MessageBox.Show(GetSafeConfigValue("Form_SnakebiteGUI_ErrorInputOutputSame", "输入和输出文件不能相同"),
                    GetSafeConfigValue("Form_Global_ErrorWindowTitle", "错误"));
                ret = false;
            }

            return ret;
        }

        private void rbFileNameButtons_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rbNameOutput.Checked)
            {
                this.tbOutputFile.Enabled = true;
                this.tbOutputFile.ReadOnly = false;
                this.btnBrowseOutput.Enabled = true;

                this.tbFileExtension.Enabled = false;
                this.tbFileExtension.ReadOnly = true;
                this.tbFileExtension.Clear();
            }
            else
            {
                this.tbOutputFile.Enabled = false;
                this.tbOutputFile.ReadOnly = true;
                this.tbOutputFile.Clear();
                this.btnBrowseOutput.Enabled = false;

                this.tbFileExtension.Enabled = true;
                this.tbFileExtension.ReadOnly = false;
            }
        }
    }
}
