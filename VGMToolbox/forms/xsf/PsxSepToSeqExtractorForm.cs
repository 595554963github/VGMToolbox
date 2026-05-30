using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using VGMToolbox.plugin;
using VGMToolbox.tools.xsf;

namespace VGMToolbox.forms.xsf
{
    public partial class PsxSepToSeqExtractorForm : AVgmtForm
    {
        public PsxSepToSeqExtractorForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            InitializeComponent();

            this.grpSource.AllowDrop = true;

            this.grpSource.Text = ConfigurationManager.AppSettings["Form_Global_DropSourceFiles"];
            this.lblTitle.Text = ConfigurationManager.AppSettings["Form_PsxSepExtractor_Title"];
            this.tbOutput.Text = ConfigurationManager.AppSettings["Form_PsxSepExtractor_IntroText"];
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new PsxSepToSeqExtractorWorker();
        }
        protected override string getCancelMessage()
        {
            return ConfigurationManager.AppSettings["Form_PsxSepExtractor_MessageCancel"];
        }
        protected override string getCompleteMessage()
        {
            return ConfigurationManager.AppSettings["Form_PsxSepExtractor_MessageComplete"];
        }
        protected override string getBeginMessage()
        {
            return ConfigurationManager.AppSettings["Form_PsxSepExtractor_MessageBegin"];
        }

        private void grpSource_DragDrop(object sender, DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            PsxSepToSeqExtractorWorker.PsxSepToSeqExtractorStruct bwStruct = new PsxSepToSeqExtractorWorker.PsxSepToSeqExtractorStruct();
            bwStruct.SourcePaths = s;

            base.backgroundWorker_Execute(bwStruct);
        }
    }
}
