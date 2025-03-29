using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.tools.stream;

using VGMToolbox.plugin;

namespace VGMToolbox.forms.stream
{
    public partial class XmashMashForm : AVgmtForm
    {
        public XmashMashForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            InitializeComponent();

            // set title
            this.lblTitle.Text = "XMAshMash";

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            this.tbOutput.Text = String.Format("- 使用XMAsh和其他工具将XMA转换为WAV.{0}", Environment.NewLine);
            this.tbOutput.Text += String.Format("* 注意：此工具需要hcs的“xmash.exe”（XMAsh v0.6或更高版本）,Xplorer 的“ToWave. exe”,cbagwell和robs的SoX.{0}", Environment.NewLine);
            this.tbOutput.Text += String.Format("  请下载XMAsh和ToWAV文件并将“xmash. exe”和“ToWAv.exe”放在以下目录中:<{0}>{1}", Path.GetDirectoryName(XmashMashWorker.XMASH_FULL_PATH), Environment.NewLine);
            this.tbOutput.Text += String.Format("  请下载所有SoX文件并将“sox.exe”和所有必需的.dls放在以下目录中:<{0}>{1}", XmashMashWorker.SOX_FOLDER, Environment.NewLine);
        }


        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new XmashMashWorker();
        }
        protected override string getCancelMessage()
        {
            return "XMAsh'ing...已取消";
        }
        protected override string getCompleteMessage()
        {
            return "XMAsh'ing...完成";
        }
        protected override string getBeginMessage()
        {
            return "XMAsh'ing...开始";
        }


        private void XmashMashForm_DragEnter(object sender, DragEventArgs e)
        {
            this.doDragEnter(sender, e);
        }

        private void XmashMashForm_DragDrop(object sender, DragEventArgs e)
        {
            XmashMashWorker.XmaMashMashStruct taskStruct = new XmashMashWorker.XmaMashMashStruct();

            // paths
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            taskStruct.SourcePaths = s;

            // options
            taskStruct.IgnoreXmashFailure = this.chkIgnoreXmashFailure.Checked;

            taskStruct.ReinterleaveMultichannel = this.chkInterleaveMultiChannelOutput.Checked;

            base.backgroundWorker_Execute(taskStruct);
        }


    }


}
