using System;
using System.Windows.Forms;

using VGMToolbox.format;
using VGMToolbox.plugin;
using VGMToolbox.tools.stream;

namespace VGMToolbox.forms.stream
{
    public partial class MpegDemuxForm : AVgmtForm
    {
        public MpegDemuxForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "视频解复用器";

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            InitializeComponent();

            this.tbOutput.Text = "从游戏视频中分解流媒体." + Environment.NewLine;
            this.tbOutput.Text += "- 当前支持的格式: BIK, DSI (PS2), DVD 视频, MO (仅Wii), MPEG1, MPEG2, PAM, PMF, PSS, SFD, THP, USM, XMV" + Environment.NewLine;
            this.tbOutput.Text += "- 如果MPEG不适用于您的文件,一定要试试DVD视频,因为它可以处理特殊的音频类型." + Environment.NewLine;
            this.tbOutput.Text += "- Bink音频文件并不总是使用RAD视频工具正确播放(binkplay.exe),但将使用RAD Video Tools“转换文件”正确转换为WAV(binkconv.exe)." + Environment.NewLine;
            this.tbOutput.Text += "- ASF/WMV解复用尚未重建到ASF流以进行输出." + Environment.NewLine;
            this.tbOutput.Text += "- MKVMerge可用于将原始.264数据添加到容器文件中以供播放." + Environment.NewLine;
            this.tbOutput.Text += "- 以下音频输出格式未知且不可测试：MO（A3 类型）." + Environment.NewLine;
            this.tbOutput.Text += "- 以下视频输出格式未知且不可测试：MO、XMV." + Environment.NewLine;


            this.initializeFormatList();
        }

        private void initializeFormatList()
        {
            this.comboFormat.Items.Clear();
            this.comboFormat.Items.Add("ASF (MS高级系统格式)");
            this.comboFormat.Items.Add("BIK (Bink视频容器)");
            this.comboFormat.Items.Add("DSI (Racjin/Racdym PS2视频)");
            this.comboFormat.Items.Add("DVD (VOB视频)");
            this.comboFormat.Items.Add("VP6 (On2 Technologies开发的VP6)");
            this.comboFormat.Items.Add("MPC (Gabest开发的MPC)");
            //this.comboFormat.Items.Add("H4M (Hudson GameCube Video)");
            this.comboFormat.Items.Add("MO (Actimagine Corp Mobiclip)");
            this.comboFormat.Items.Add("MPEG");
            this.comboFormat.Items.Add("MPS (PSP UMD电影)");
            this.comboFormat.Items.Add("PAM (PlayStation高级电影)");
            this.comboFormat.Items.Add("PMF (PSP电影格式)");
            this.comboFormat.Items.Add("PSS (PlayStation流媒体)");
            this.comboFormat.Items.Add("SFD (CRI Sofdec视频)");
            this.comboFormat.Items.Add("THP (Expansive Worlds开发)");
            this.comboFormat.Items.Add("USM (CRI Movie 2)");
            this.comboFormat.Items.Add("WMV (MS高级系统格式)");
            this.comboFormat.Items.Add("XMV (Xbox媒体视频)");

            this.comboFormat.SelectedItem = "ASF (MS高级系统格式)";
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new MpegDemuxWorker();
        }
        protected override string getCancelMessage()
        {
            return "解复用流… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "解复用流… 完成";
        }
        protected override string getBeginMessage()
        {
            return "解复用流… 开始";
        }

        private void MpegDemuxForm_DragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        private void MpegDemuxForm_DragDrop(object sender, DragEventArgs e)
        {
            MpegDemuxWorker.MpegDemuxStruct taskStruct = new MpegDemuxWorker.MpegDemuxStruct();

            // paths
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            taskStruct.SourcePaths = s;

            // format
            taskStruct.SourceFormat = this.comboFormat.SelectedItem.ToString();

            // options
            taskStruct.AddHeader = this.cbAddHeader.Checked;
            taskStruct.SplitAudioTracks = this.cbSplitAudioTracks.Checked;
            taskStruct.ExtractAudio = (this.rbExtractAudioAndVideo.Checked ||
                                       this.rbExtractAudioOnly.Checked);
            taskStruct.ExtractVideo = (this.rbExtractAudioAndVideo.Checked ||
                                       this.rbExtractVideoOnly.Checked);

            base.backgroundWorker_Execute(taskStruct);
        }

        private void comboFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboFormat.SelectedItem.ToString())
            {
                case "BIK (Bink视频容器)":
                    this.cbSplitAudioTracks.Enabled = true;
                    this.cbSplitAudioTracks.Checked = false;
                    this.cbAddHeader.Enabled = true;
                    this.cbAddHeader.Checked = true;
                    break;
                //case "ASF (MS Advanced Systems Format)":
                case "DSI (Racjin/Racdym PS2视频)":
                case "H4M (Hudson GameCube Video)":
                case "MO (Mobiclip)":
                case "MPS (PSP UMD视频)":
                case "PAM (PlayStation高级电影)":
                case "PMF (PSP电影格式)":
                //case "WMV (MS Advanced Systems Format)":
                case "XMV (Xbox媒体视频)":
                    this.cbSplitAudioTracks.Enabled = false;
                    this.cbSplitAudioTracks.Checked = false;
                    this.cbAddHeader.Enabled = true;
                    this.cbAddHeader.Checked = true;
                    break;
                default:
                    this.cbSplitAudioTracks.Enabled = false;
                    this.cbSplitAudioTracks.Checked = false;
                    this.cbAddHeader.Checked = false;
                    this.cbAddHeader.Enabled = false;
                    break;
            }
        }
    }
}