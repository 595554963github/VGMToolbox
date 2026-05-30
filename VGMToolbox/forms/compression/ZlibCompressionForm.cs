using System;
using System.Configuration;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.extract;
using VGMToolbox.util;

namespace VGMToolbox.forms.compression
{
    public partial class ZlibCompressionForm : AVgmtForm
    {
        private static string GetSafeConfigValue(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public ZlibCompressionForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            InitializeComponent();

            this.grpSourceFiles.AllowDrop = true;
            this.btnDoTask.Hide();

            this.lblTitle.Text = GetSafeConfigValue("Form_ZlibCompress_Title", "zlib压缩/解压缩");

            string introText1 = GetSafeConfigValue("Form_ZlibCompress_IntroText1", "解压缩zlib文件将得到扩展名为{0}的文件{1}");
            this.tbOutput.Text = String.Format(introText1,
                            CompressionUtil.ZlibDecompressOutputExtension, Environment.NewLine);

            string introText2 = GetSafeConfigValue("Form_ZlibCompress_IntroText2", "压缩文件将得到扩展名为{0}的文件{1}");
            this.tbOutput.Text += String.Format(introText2,
                CompressionUtil.ZlibCompressOutputExtension, Environment.NewLine);

            this.grpSourceFiles.Text = GetSafeConfigValue("Form_Global_DropSourceFiles", "在此处拖放文件");
            this.grpOptions.Text = GetSafeConfigValue("Form_ZlibCompress_GrpOptions", "选项");
            this.rbDecompress.Text = GetSafeConfigValue("Form_ZlibCompress_RbDecompress", "解压缩");
            this.rbCompress.Text = GetSafeConfigValue("Form_ZlibCompress_RbCompress", "压缩");
            this.lblOffset.Text = GetSafeConfigValue("Form_ZlibCompress_LblOffset", "起始偏移量（可选）");
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new ZlibExtractorWorker();
        }
        protected override string getCancelMessage()
        {
            return GetSafeConfigValue("Form_ZlibCompress_MessageCancel", "操作已取消");
        }
        protected override string getCompleteMessage()
        {
            return GetSafeConfigValue("Form_ZlibCompress_MessageComplete", "操作完成");
        }
        protected override string getBeginMessage()
        {
            return GetSafeConfigValue("Form_ZlibCompress_MessageBegin", "正在处理...");
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (validateInputs())
            {

                ZlibExtractorWorker.ZlibExtractorStruct zlStruct = new ZlibExtractorWorker.ZlibExtractorStruct();
                zlStruct.SourcePaths = s;
                zlStruct.DoDecompress = this.rbDecompress.Checked;
                zlStruct.StartingOffset = VGMToolbox.util.ByteConversion.GetLongValueFromString(this.tbOffset.Text);

                base.backgroundWorker_Execute(zlStruct);
            }
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
                MessageBox.Show("无法解析偏移量,请输入整数.确保在十六进制值前加上0x",
                    GetSafeConfigValue("Form_Global_ErrorWindowTitle", "错误"));
                ret = false;
            }

            return ret;
        }
    }
}
