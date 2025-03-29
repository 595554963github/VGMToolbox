using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.gbs;

namespace VGMToolbox.forms.nsf
{
    public partial class NsfToM3uForm : AVgmtForm
    {
        public NsfToM3uForm(TreeNode pTreeNode) : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = "简易NSF .m3u创建";
            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            InitializeComponent();

            this.grpSource.AllowDrop = true;

            this.grpSource.Text =
                ConfigurationManager.AppSettings["Form_Global_DropSourceFiles"];
            this.grpOptions.Text = "选项";
            this.cbOneM3uPerTrack.Text =
                ConfigurationManager.AppSettings["Form_GbsM3u_CheckBoxOneM3uPerTrack"];
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new GbsM3uBuilderWorker();
        }
        protected override string getCancelMessage()
        {
            return "NSF .M3U 创建… 已取消";
        }
        protected override string getCompleteMessage()
        {
            return "NSF .M3U 创建… 完成";
        }
        protected override string getBeginMessage()
        {
            return "NSF .M3U 创建… 开始";
        }

        private void grpSource_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            GbsM3uBuilderWorker.GbsM3uWorkerStruct gbStruct = new GbsM3uBuilderWorker.GbsM3uWorkerStruct();
            gbStruct.SourcePaths = s;
            gbStruct.UseKnurekFormatParsing = this.cbUseKnurekFormat.Checked;
            gbStruct.onePlaylistPerFile = cbOneM3uPerTrack.Checked;

            base.backgroundWorker_Execute(gbStruct);
        }
    }
}
