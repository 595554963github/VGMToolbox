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
using VGMToolbox.util;

namespace VGMToolbox.forms.compression
{
    public partial class GzipCompressionForm : AVgmtForm
    {
        private static string GetSafeConfigValue(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public GzipCompressionForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            InitializeComponent();

            this.grpSourceFiles.AllowDrop = true;
            this.btnDoTask.Hide();

            this.lblTitle.Text = GetSafeConfigValue("Form_GzipCompress_Title", "gzip压缩/解压缩");

            string introText1 = GetSafeConfigValue("Form_GzipCompress_IntroText1", "解压缩gzip文件将得到扩展名为{0}的文件{1}");
            this.tbOutput.Text = String.Format(introText1,
                            CompressionUtil.GzipDecompressOutputExtension, Environment.NewLine);

            string introText2 = GetSafeConfigValue("Form_GzipCompress_IntroText2", "压缩文件将得到扩展名为{0}的文件{1}");
            this.tbOutput.Text += String.Format(introText2,
                CompressionUtil.GzipCompressOutputExtension, Environment.NewLine);

            this.grpSourceFiles.Text = GetSafeConfigValue("Form_Global_DropSourceFiles", "在此处拖放文件");
            this.grpOptions.Text = GetSafeConfigValue("Form_GzipCompress_GrpOptions", "选项");
            this.rbDecompress.Text = GetSafeConfigValue("Form_GzipCompress_RbDecompress", "解压缩");
            this.rbCompress.Text = GetSafeConfigValue("Form_GzipCompress_RbCompress", "压缩");
            this.lblOffset.Text = GetSafeConfigValue("Form_GzipCompress_LblOffset", "起始偏移量（可选）");
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new GzipExtractorWorker();
        }
        protected override string getCancelMessage()
        {
            return GetSafeConfigValue("Form_GzipCompress_MessageCancel", "操作已取消");
        }
        protected override string getCompleteMessage()
        {
            return GetSafeConfigValue("Form_GzipCompress_MessageComplete", "操作完成");
        }
        protected override string getBeginMessage()
        {
            return GetSafeConfigValue("Form_GzipCompress_MessageBegin", "正在处理...");
        }

        private bool validateInputs()
        {
            bool ret = true;

            // put 0 in Offset if it is empty
            if (String.IsNullOrEmpty(this.tbOffset.Text))
            {
                this.tbOffset.Text = "0";
            }

            try
            {
                long tempval = VGMToolbox.util.ByteConversion.GetLongValueFromString(this.tbOffset.Text);
            }
            catch
            {
                MessageBox.Show(GetSafeConfigValue("Form_GzipCompress_ErrorIntParse", "请输入有效的数字作为偏移量"),
                    GetSafeConfigValue("Form_Global_ErrorWindowTitle", "错误"));
                ret = false;
            }

            return ret;
        }
        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (validateInputs())
            {

                GzipExtractorWorker.GzipExtractorStruct bwStruct = new GzipExtractorWorker.GzipExtractorStruct();
                bwStruct.SourcePaths = s;
                bwStruct.DoDecompress = this.rbDecompress.Checked;
                bwStruct.StartingOffset = VGMToolbox.util.ByteConversion.GetLongValueFromString(this.tbOffset.Text);

                base.backgroundWorker_Execute(bwStruct);
            }
        }
    }
}
