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
using VGMToolbox.tools.xsf;
using VGMToolbox.util;

namespace VGMToolbox.forms.xsf
{
    public partial class NdsTo2sfForm : AVgmtForm
    {
        private static string GetSafeConfigValue(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public NdsTo2sfForm(TreeNode pTreeNode) :
            base(pTreeNode)
        {
            string testpackPath = Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath),
                NdsTo2sfWorker.TESTPACK_PATH);

            InitializeComponent();

            this.grpSourceFiles.AllowDrop = true;
            this.btnDoTask.Hide();

            this.lblTitle.Text = GetSafeConfigValue("Form_NdsTo2sf_Title", "NDS转2SF");
            this.tbOutput.Text = GetSafeConfigValue("Form_NdsTo2sf_IntroText", "在此处拖放NDS文件") + Environment.NewLine;
            this.tbOutput.Text += GetSafeConfigValue("Form_Make2sf_IntroText1", "请确保你有必要的测试包文件") + Environment.NewLine;

            string introText2 = GetSafeConfigValue("Form_Make2sf_IntroText2", "测试包应该位于: {0}");
            this.tbOutput.Text += String.Format(introText2, Path.GetDirectoryName(testpackPath)) + Environment.NewLine;

            this.grpSourceFiles.Text = GetSafeConfigValue("Form_Global_DropSourceFiles", "在此处拖放文件");
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }
        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new NdsTo2sfWorker();
        }
        protected override string getCancelMessage()
        {
            return GetSafeConfigValue("Form_NdsTo2sf_MessageCancel", "操作已取消");
        }
        protected override string getCompleteMessage()
        {
            return GetSafeConfigValue("Form_NdsTo2sf_MessageComplete", "操作完成");
        }
        protected override string getBeginMessage()
        {
            return GetSafeConfigValue("Form_NdsTo2sf_MessageBegin", "正在处理...");
        }

        private static bool CheckForTestPackNds()
        {
            bool ret = true;
            string testpackPath =
                Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), NdsTo2sfWorker.TESTPACK_PATH);

            if (!File.Exists(testpackPath))
            {
                ret = false;
                string msg = GetSafeConfigValue("Form_Make2sf_ErrorMessageTestpackMissing", "测试包文件 {0} 未找到！请将其放在 {1} 目录中。");
                string header = GetSafeConfigValue("Form_Make2sf_ErrorMessageTestpackMissingHeader", "缺少测试包 {0}");
                MessageBox.Show(String.Format(msg, Path.GetFileName(testpackPath), Path.GetDirectoryName(testpackPath)),
                    String.Format(header, Path.GetFileName(testpackPath)));
            }
            else
            {
                using (FileStream fs = File.OpenRead(testpackPath))
                {
                    if (!ChecksumUtil.GetCrc32OfFullFile(fs).Equals(Mk2sfWorker.TESTPACK_CRC32))
                    {
                        ret = false;
                        string msg = GetSafeConfigValue("Form_Make2sf_ErrorMessageTestpackCrc32", "测试包文件 {0} 具有不正确的CRC32校验和！请确保你有正确的文件。预期的CRC32: {2}");
                        string header = GetSafeConfigValue("Form_Make2sf_ErrorMessageTestpackCrc32Header", "测试包 {0} 的CRC32不匹配");
                        MessageBox.Show(String.Format(msg, Path.GetFileName(testpackPath), Path.GetDirectoryName(testpackPath), NdsTo2sfWorker.TESTPACK_CRC32),
                            String.Format(header, Path.GetFileName(testpackPath)));
                    }
                }
            }

            return ret;
        }

        private void grpSourceFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            if (CheckForTestPackNds())
            {
                NdsTo2sfWorker.NdsTo2sfStruct bwStruct = new NdsTo2sfWorker.NdsTo2sfStruct();
                bwStruct.SourcePaths = s;
                bwStruct.UseSmapNames = this.cbUseSmapNames.Checked;

                base.backgroundWorker_Execute(bwStruct);
            }

        }
    }
}
