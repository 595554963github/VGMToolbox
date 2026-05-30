using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using VGMToolbox.dbutil;
using VGMToolbox.plugin;
using VGMToolbox.tools;
using VGMToolbox.tools.extract;

namespace VGMToolbox.forms.extraction
{
    public partial class OffsetFinderForm : AVgmtForm
    {
        private static readonly string DB_PATH =
            Path.Combine(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "db"), "collection.s3db");
        private static readonly string PLUGIN_PATH =
            Path.Combine(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "plugins"), "AdvancedCutter");

        private static string GetSafeConfigValue(string key, string defaultValue = "")
        {
            string value = ConfigurationManager.AppSettings[key];
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }

        public OffsetFinderForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            // set title
            this.lblTitle.Text = GetSafeConfigValue("Form_SimpleCutter_Title", "高级切割器/偏移查找器");

            // hide the DoTask button since this is a drag and drop form
            this.btnDoTask.Hide();

            string introText = GetSafeConfigValue("Form_SimpleCutter_IntroText", "在此处拖放文件 - 预设可从 plugins\\AdvancedCutter 加载");
            this.tbOutput.Text = String.Format(introText, PLUGIN_PATH);

            InitializeComponent();

            this.toolTip1.SetToolTip(this.btnRefresh, "刷新预设");

            this.grpFiles.Text =
                GetSafeConfigValue("Form_SimpleCutter_GroupFiles", "文件");
            this.lblDragNDrop.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblDragNDrop", "拖放文件");
            this.grpCriteria.Text =
                GetSafeConfigValue("Form_SimpleCutter_GroupCriteria", "搜索条件");
            this.cbDoCut.Text =
                GetSafeConfigValue("Form_SimpleCutter_CheckBoxDoCut", "切割文件");
            this.lblStringAtOffset.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblStringAtOffset", "字符串在偏移处");
            this.lblOutputExtension.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblOutputExtension", "输出扩展名");
            this.gbCutSizeOptions.Text =
                GetSafeConfigValue("Form_SimpleCutter_GroupCutSizeOptions", "切割尺寸选项");
            this.rbStaticCutSize.Text =
                GetSafeConfigValue("Form_SimpleCutter_RadioStaticCutSize", "静态切割尺寸");
            this.rbOffsetBasedCutSize.Text =
                GetSafeConfigValue("Form_SimpleCutter_RadioOffsetBasedCutSize", "偏移切割尺寸");
            this.lblHasSize.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblHasSize", "包含尺寸");
            this.lblStoredIn.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblStoredIn", "存储在");
            this.lblInBytes.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblInBytes", "（字节）");
            this.lblFromStart.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblFromStart", "从开始");
            this.lblInBytes2.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblInBytes2", "（字节）");
            this.lblByteOrder.Text =
                GetSafeConfigValue("Form_SimpleCutter_LblByteOrder", "字节顺序");
            this.rbUseTerminator.Text =
                GetSafeConfigValue("Form_SimpleCutter_RadioUseTerminator", "使用终止符");
            this.cbTreatTerminatorAsHex.Text =
                GetSafeConfigValue("Form_SimpleCutter_CheckBoxTreatTerminatorAsHex", "将终止符视为十六进制");
            this.cbIncludeTerminatorInLength.Text =
                GetSafeConfigValue("Form_SimpleCutter_CheckBoxIncludeTerminatorInLength", "在长度中包含终止符");
            this.cbAddExtraBytes.Text =
                GetSafeConfigValue("Form_SimpleCutter_CheckBoxExtracCutSizeBytes", "添加额外字节");

            this.createEndianList();
            this.createOffsetSizeList();
            this.doOffsetModuloSearchStringCheckbox();
            this.resetCutSection();

            this.loadOffsetPlugins();
        }

        private void tbSourcePaths_DragDrop(object sender, DragEventArgs e)
        {
            if (validateInputs())
            {
                string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);

                OffsetFinderWorker.OffsetFinderStruct ofStruct = new OffsetFinderWorker.OffsetFinderStruct();
                ofStruct.SourcePaths = s;
                ofStruct.searchString = tbSearchString.Text;
                ofStruct.startingOffset =
                    String.IsNullOrEmpty(tbStartingOffset.Text) ? "0" : tbStartingOffset.Text;
                ofStruct.treatSearchStringAsHex = cbSearchAsHex.Checked;

                if (cbModOffsetSearchString.Checked)
                {
                    ofStruct.DoSearchStringModulo = true;
                    ofStruct.SearchStringModuloDivisor = this.tbOffsetModuloSearchStringDivisor.Text;
                    ofStruct.SearchStringModuloResult = this.tbOffsetModuloSearchStringResult.Text;
                }

                ofStruct.UseOffsetString = cbUseOffsetString.Checked;
                ofStruct.OffsetString = tbOffsetString.Text;
                ofStruct.UseOffsetBytes = cbUseOffsetBytes.Checked;
                ofStruct.OffsetBytes = tbOffsetBytes.Text;
                ofStruct.OffsetCount = tbOffsetCount.Text;

                if (cbDoCut.Checked)
                {
                    ofStruct.cutFile = true;

                    ofStruct.searchStringOffset = this.tbSearchStringOffset.Text;
                    ofStruct.OutputFolder = this.tbOutputFolder.Text;
                    ofStruct.outputFileExtension = this.tbOutputExtension.Text;
                    ofStruct.MinimumSize = this.tbMinSizeForCut.Text;

                    if (this.rbOffsetBasedCutSize.Checked)
                    {
                        ofStruct.cutSize = this.tbCutSizeOffset.Text;
                        ofStruct.cutSizeOffsetSize = this.cbOffsetSize.Text;
                        ofStruct.isCutSizeAnOffset = true;
                        ofStruct.isLittleEndian = (cbByteOrder.Text.Equals(OffsetFinderWorker.LITTLE_ENDIAN));
                        ofStruct.UseLengthMultiplier = cbUseLengthMultiplier.Checked;
                        ofStruct.LengthMultiplier = this.tbLengthMultiplier.Text;

                    }
                    else if (this.rbUseTerminator.Checked)
                    {
                        ofStruct.useTerminatorForCutsize = true;
                        ofStruct.terminatorString = this.tbTerminatorString.Text;
                        ofStruct.treatTerminatorStringAsHex = this.cbTreatTerminatorAsHex.Checked;
                        ofStruct.includeTerminatorLength = this.cbIncludeTerminatorInLength.Checked;
                        ofStruct.CutToEofIfTerminatorNotFound = this.cbCutToEOFWhenTerminatorNotFound.Checked;

                        if (cbModOffsetTerminator.Checked)
                        {
                            ofStruct.DoTerminatorModulo = true;
                            ofStruct.TerminatorStringModuloDivisor = this.tbOffsetModuloTerminatorDivisor.Text;
                            ofStruct.TerminatorStringModuloResult = this.tbOffsetModuloTerminatorResult.Text;
                        }
                    }
                    else if (this.rbStaticCutSize.Checked)
                    {
                        ofStruct.cutSize = this.tbStaticCutsize.Text;
                    }
                    else
                    {
                        MessageBox.Show("请选择一个单选按钮,指示要使用的切割尺寸选项.");
                        return; // hokey, but oh well
                    }

                    if (cbAddExtraBytes.Checked)
                    {
                        ofStruct.extraCutSizeBytes = tbExtraCutSizeBytes.Text;
                    }
                }

                ofStruct.OutputLogFile = this.cbOutputLogFile.Checked;

                base.backgroundWorker_Execute(ofStruct);
            }
        }

        protected override void doDragEnter(object sender, DragEventArgs e)
        {
            base.doDragEnter(sender, e);
        }

        private void doRadioCheckedChanged(object sender, EventArgs e)
        {
            if (rbStaticCutSize.Checked)
            {
                tbStaticCutsize.ReadOnly = false;
                tbCutSizeOffset.ReadOnly = true;
                cbOffsetSize.Enabled = false;
                cbByteOrder.Enabled = false;
                this.cbUseLengthMultiplier.Checked = false;
                this.cbUseLengthMultiplier.Enabled = false;
                tbTerminatorString.ReadOnly = true;
                cbTreatTerminatorAsHex.Enabled = false;
                cbIncludeTerminatorInLength.Enabled = false;
                this.cbModOffsetTerminator.Enabled = false;
                this.tbOffsetModuloTerminatorDivisor.ReadOnly = true;
                this.tbOffsetModuloTerminatorDivisor.Enabled = false;
                this.tbOffsetModuloTerminatorResult.ReadOnly = true;
                this.tbOffsetModuloTerminatorResult.Enabled = false;
                this.cbCutToEOFWhenTerminatorNotFound.Checked = false;
                this.cbCutToEOFWhenTerminatorNotFound.Enabled = false;
            }
            else if (rbOffsetBasedCutSize.Checked)
            {
                tbStaticCutsize.ReadOnly = true;
                tbCutSizeOffset.ReadOnly = false;
                cbOffsetSize.Enabled = true;
                cbByteOrder.Enabled = true;
                this.cbUseLengthMultiplier.Enabled = true;
                tbTerminatorString.ReadOnly = true;
                cbTreatTerminatorAsHex.Enabled = false;
                cbIncludeTerminatorInLength.Enabled = false;
                this.cbModOffsetTerminator.Enabled = false;
                this.tbOffsetModuloTerminatorDivisor.ReadOnly = true;
                this.tbOffsetModuloTerminatorDivisor.Enabled = false;
                this.tbOffsetModuloTerminatorResult.ReadOnly = true;
                this.tbOffsetModuloTerminatorResult.Enabled = false;
                this.cbCutToEOFWhenTerminatorNotFound.Checked = false;
                this.cbCutToEOFWhenTerminatorNotFound.Enabled = false;
            }
            else
            {
                tbStaticCutsize.ReadOnly = true;
                tbCutSizeOffset.ReadOnly = true;
                cbOffsetSize.Enabled = false;
                cbByteOrder.Enabled = false;
                this.cbUseLengthMultiplier.Enabled = false;
                tbTerminatorString.ReadOnly = false;
                cbTreatTerminatorAsHex.Enabled = true;
                cbIncludeTerminatorInLength.Enabled = true;
                this.cbModOffsetTerminator.Enabled = true;
                this.tbOffsetModuloTerminatorDivisor.ReadOnly = false;
                this.tbOffsetModuloTerminatorDivisor.Enabled = true;
                this.tbOffsetModuloTerminatorResult.ReadOnly = false;
                this.tbOffsetModuloTerminatorResult.Enabled = true;
                this.cbCutToEOFWhenTerminatorNotFound.Enabled = true;
            }

            this.doOffsetModuloTerminatorCheckbox();
            this.doCbUseLengthMultiplier();
        }

        private void createEndianList()
        {
            cbByteOrder.Items.Add(OffsetFinderWorker.BIG_ENDIAN);
            cbByteOrder.Items.Add(OffsetFinderWorker.LITTLE_ENDIAN);
        }
        private void createOffsetSizeList()
        {
            cbOffsetSize.Items.Add("1");
            cbOffsetSize.Items.Add("2");
            cbOffsetSize.Items.Add("4");
        }

        private void cbByteOrder_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void cbByteOrder_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }
        private void cbOffsetSize_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void cbOffsetSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private bool validateInputs()
        {
            bool ret = AVgmtForm.checkTextBox(this.tbSearchString.Text, "搜索字符串");

            if (cbUseOffsetString.Checked)
            {
                ret = ret && AVgmtForm.checkTextBox(this.tbOffsetString.Text, "偏移字符串");
            }
            if (cbUseOffsetBytes.Checked)
            {
                ret = ret && AVgmtForm.checkTextBox(this.tbOffsetBytes.Text, "偏移字节序列");
            }
            if ((cbUseOffsetString.Checked || cbUseOffsetBytes.Checked) &&
                string.IsNullOrEmpty(tbOffsetCount.Text))
            {
                MessageBox.Show("当使用偏移验证时，必须指定偏移数量。", "输入错误");
                return false;
            }

            if (cbDoCut.Checked)
            {
                if (!String.IsNullOrEmpty(this.tbOutputFolder.Text))
                {
                    ret = ret && AVgmtForm.checkFolderExists(this.tbOutputFolder.Text, this.lblOutputFolder.Text);
                }
                ret = ret && AVgmtForm.checkTextBox(this.tbOutputExtension.Text, "输出扩展名");

                if (this.rbStaticCutSize.Checked)
                {
                    ret = ret && AVgmtForm.checkTextBox(this.tbStaticCutsize.Text, "静态切割尺寸");
                }
                else if (rbOffsetBasedCutSize.Checked)
                {
                    ret = ret && AVgmtForm.checkTextBox(this.tbCutSizeOffset.Text, "切割尺寸偏移");
                    ret = ret && AVgmtForm.checkTextBox((string)this.cbOffsetSize.Text, "偏移量大小");
                    ret = ret && AVgmtForm.checkTextBox((string)this.cbByteOrder.Text, "字节顺序");
                }
                else
                {
                    ret = ret && AVgmtForm.checkTextBox(this.tbTerminatorString.Text, "终止字符串");
                }
            }

            return ret;
        }

        private void cbDoCut_CheckedChanged(object sender, EventArgs e)
        {
            this.resetCutSection();
        }

        private void resetCutSection()
        {
            this.cbUseLengthMultiplier.Checked = false;

            if (cbDoCut.Checked)
            {
                this.cbOutputLogFile.Enabled = true;
                this.cbOutputLogFile.Checked = false;
                this.cbOutputLogFile.Show();

                tbSearchStringOffset.ReadOnly = false;
                tbOutputExtension.ReadOnly = false;

                this.lblMinCutSize.Show();
                this.tbMinSizeForCut.ReadOnly = false;
                this.tbMinSizeForCut.Enabled = true;
                this.tbMinSizeForCut.Show();
                this.lblMinCutSizeBytes.Show();

                rbStaticCutSize.Enabled = true;
                rbOffsetBasedCutSize.Enabled = true;

                rbStaticCutSize.Checked = false;
                rbOffsetBasedCutSize.Checked = false;

                tbStaticCutsize.ReadOnly = false;
                tbCutSizeOffset.ReadOnly = false;
                cbOffsetSize.Enabled = true;
                cbByteOrder.Enabled = true;

                tbSearchStringOffset.Show();
                tbOutputExtension.Show();
                rbStaticCutSize.Show();
                rbOffsetBasedCutSize.Show();
                rbStaticCutSize.Show();
                rbOffsetBasedCutSize.Show();
                tbStaticCutsize.Show();
                tbCutSizeOffset.Show();
                cbOffsetSize.Show();
                cbByteOrder.Show();

                cbUseLengthMultiplier.Show();
                tbLengthMultiplier.Show();

                gbCutSizeOptions.Show();
                lblStringAtOffset.Show();
                lblHasSize.Show();
                lblFromStart.Show();
                lblInBytes2.Show();
                lblInBytes.Show();
                lblStoredIn.Show();
                lblByteOrder.Show();
                lblOutputExtension.Show();

                this.rbUseTerminator.Show();
                this.tbTerminatorString.Show();
                this.cbTreatTerminatorAsHex.Show();
                this.cbIncludeTerminatorInLength.Show();

                this.cbAddExtraBytes.Show();
                this.tbExtraCutSizeBytes.Show();

                this.cbModOffsetTerminator.Checked = false;
                this.cbModOffsetTerminator.Show();
                this.tbOffsetModuloTerminatorDivisor.Show();
                this.tbOffsetModuloTerminatorResult.Show();
                this.lblOffsetModuloEquals.Show();

                this.lblOutputFolder.Show();
                this.tbOutputFolder.Show();
                this.btnBrowseOutputFolder.Show();
            }
            else
            {
                this.cbOutputLogFile.Enabled = false;
                this.cbOutputLogFile.Checked = false;
                this.cbOutputLogFile.Hide();

                tbSearchStringOffset.ReadOnly = true;
                tbOutputExtension.ReadOnly = true;

                this.lblMinCutSize.Hide();
                this.tbMinSizeForCut.ReadOnly = true;
                this.tbMinSizeForCut.Enabled = false;
                this.tbMinSizeForCut.Hide();
                this.lblMinCutSizeBytes.Hide();

                rbStaticCutSize.Checked = false;
                rbOffsetBasedCutSize.Checked = false;

                rbStaticCutSize.Enabled = false;
                rbOffsetBasedCutSize.Enabled = false;

                tbStaticCutsize.ReadOnly = true;
                tbCutSizeOffset.ReadOnly = true;
                cbOffsetSize.Enabled = false;
                cbByteOrder.Enabled = false;

                cbUseLengthMultiplier.Show();
                tbLengthMultiplier.Show();

                tbSearchStringOffset.Hide();
                tbOutputExtension.Hide();
                rbStaticCutSize.Hide();
                rbOffsetBasedCutSize.Hide();
                rbStaticCutSize.Hide();
                rbOffsetBasedCutSize.Hide();
                tbStaticCutsize.Hide();
                tbCutSizeOffset.Hide();
                cbOffsetSize.Hide();
                cbByteOrder.Hide();

                gbCutSizeOptions.Hide();
                lblStringAtOffset.Hide();
                lblHasSize.Hide();
                lblFromStart.Hide();
                lblInBytes2.Hide();
                lblInBytes.Hide();
                lblStoredIn.Hide();
                lblByteOrder.Hide();
                lblOutputExtension.Hide();

                this.rbUseTerminator.Hide();
                this.tbTerminatorString.Hide();
                this.cbTreatTerminatorAsHex.Hide();
                this.cbIncludeTerminatorInLength.Hide();

                this.cbAddExtraBytes.Hide();
                this.tbExtraCutSizeBytes.Hide();

                this.cbModOffsetTerminator.Checked = false;
                this.cbModOffsetTerminator.Hide();
                this.tbOffsetModuloTerminatorDivisor.Hide();
                this.tbOffsetModuloTerminatorResult.Hide();
                this.lblOffsetModuloEquals.Hide();

                this.lblOutputFolder.Hide();
                this.tbOutputFolder.Hide();
                this.btnBrowseOutputFolder.Hide();
            }

            this.doCbUseLengthMultiplier();
            this.doOffsetModuloTerminatorCheckbox();
        }

        private void resetCriteriaSection()
        {
            this.tbSearchString.Clear();
            this.cbSearchAsHex.Checked = false;
            this.tbStartingOffset.Text = "0";

            this.cbModOffsetSearchString.Checked = false;
            this.doOffsetModuloSearchStringCheckbox();

            this.cbUseOffsetString.Checked = false;
            this.tbOffsetString.Clear();
            this.cbUseOffsetBytes.Checked = false;
            this.tbOffsetBytes.Clear();
            this.tbOffsetCount.Clear();
        }

        private void cbAddExtraBytes_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAddExtraBytes.Checked)
            {
                tbExtraCutSizeBytes.ReadOnly = false;
            }
            else
            {
                tbExtraCutSizeBytes.ReadOnly = true;
            }
        }

        protected override IVgmtBackgroundWorker getBackgroundWorker()
        {
            return new OffsetFinderWorker();
        }
        protected override string getCancelMessage()
        {
            return GetSafeConfigValue("Form_SimpleCutter_MessageCancel", "操作已取消");
        }
        protected override string getCompleteMessage()
        {
            return GetSafeConfigValue("Form_SimpleCutter_MessageComplete", "操作完成");
        }
        protected override string getBeginMessage()
        {
            return GetSafeConfigValue("Form_SimpleCutter_MessageBegin", "正在处理...");
        }

        private void loadPresetsComboBox()
        {
            this.comboPresets.Items.Add(String.Empty);

            this.comboPresets.DataSource = SqlLiteUtil.GetSimpleDataTable(DB_PATH, "OffsetFinder", "OffsetFinderFormatName");
            this.comboPresets.DisplayMember = "OffsetFinderFormatName";
            this.comboPresets.ValueMember = "OffsetFinderId";
        }

        private void loadSelectedItem()
        {
            OffsetFinderTemplate preset = (OffsetFinderTemplate)this.comboPresets.SelectedItem;

            if (preset != null)
            {
                this.resetCriteriaSection();
                this.resetCutSection();
                this.loadOffsetFinderPreset(preset);

                if (!String.IsNullOrEmpty(preset.NotesOrWarnings))
                {
                    MessageBox.Show(preset.NotesOrWarnings, "注意事项/警告");
                }
            }
        }
        private void comboPresets_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        private void comboPresets_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void btnLoadPreset_Click(object sender, EventArgs e)
        {
            loadSelectedItem();
        }

        private void loadOffsetPlugins()
        {
            comboPresets.Items.Clear();

            foreach (string f in Directory.GetFiles(PLUGIN_PATH, "*.xml", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    OffsetFinderTemplate preset = getPresetFromFile(f);

                    if ((preset != null) && (!String.IsNullOrEmpty(preset.Header.FormatName)))
                    {
                        comboPresets.Items.Add(preset);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(String.Format("加载预设文件时出错<{0}>", Path.GetFileName(f)), "错误");
                }
            }

            comboPresets.Sorted = true;
        }
        private OffsetFinderTemplate getPresetFromFile(string filePath)
        {
            OffsetFinderTemplate preset = null;

            preset = new OffsetFinderTemplate();
            XmlSerializer serializer = new XmlSerializer(preset.GetType());
            using (FileStream xmlFs = File.OpenRead(filePath))
            {
                using (XmlTextReader textReader = new XmlTextReader(xmlFs))
                {
                    preset = (OffsetFinderTemplate)serializer.Deserialize(textReader);
                }
            }

            return preset;
        }
        private void loadOffsetFinderPreset(OffsetFinderTemplate presets)
        {
            this.resetCriteriaSection();
            this.resetCutSection();

            this.cbDoCut.Checked = true;

            // Criteria Section
            this.tbSearchString.Text = presets.SearchParameters.SearchString;
            this.cbSearchAsHex.Checked = presets.SearchParameters.TreatSearchStringAsHex;

            this.tbStartingOffset.Text = string.IsNullOrEmpty(presets.SearchParameters.StartingOffset) ?
                "0" : presets.SearchParameters.StartingOffset;

            this.cbModOffsetSearchString.Checked = false;
            if (presets.SearchParameters.UseModOffsetForSearchStringSpecified &&
                presets.SearchParameters.UseModOffsetForSearchString)
            {
                this.cbModOffsetSearchString.Checked = true;
                this.tbOffsetModuloSearchStringDivisor.Text = presets.SearchParameters.ModOffsetForSearchStringDivisor;
                this.tbOffsetModuloSearchStringResult.Text = presets.SearchParameters.ModOffsetForSearchStringResult;
            }

            this.cbUseOffsetString.Checked = false;
            this.cbUseOffsetBytes.Checked = false;
            this.tbOffsetCount.Clear();

            if (presets.SearchParameters.UseOffsetString)
            {
                this.cbUseOffsetString.Checked = true;
                this.tbOffsetString.Text = presets.SearchParameters.OffsetString;
            }
            if (presets.SearchParameters.UseOffsetBytes)
            {
                this.cbUseOffsetBytes.Checked = true;
                this.tbOffsetBytes.Text = presets.SearchParameters.OffsetBytes;
            }
            if (!string.IsNullOrEmpty(presets.SearchParameters.OffsetCount))
            {
                this.tbOffsetCount.Text = presets.SearchParameters.OffsetCount;
            }

            // Cut Options
            this.tbSearchStringOffset.Text = presets.SearchParameters.SearchStringOffset;
            this.tbOutputExtension.Text = presets.SearchParameters.OutputFileExtension;
            this.tbMinSizeForCut.Text = presets.SearchParameters.MinimumSizeForCutting;

            this.rbStaticCutSize.Checked = false;
            this.rbOffsetBasedCutSize.Checked = false;
            this.rbUseTerminator.Checked = false;

            this.tbStaticCutsize.Clear();
            this.tbCutSizeOffset.Clear();
            this.tbTerminatorString.Clear();
            this.cbUseLengthMultiplier.Checked = false;
            this.tbLengthMultiplier.Clear();
            this.cbModOffsetTerminator.Checked = false;
            this.tbOffsetModuloTerminatorDivisor.Clear();
            this.tbOffsetModuloTerminatorResult.Clear();

            switch (presets.SearchParameters.CutParameters.CutStyle)
            {
                case CutStyle.@static:
                    this.rbStaticCutSize.Checked = true;
                    this.tbStaticCutsize.Text = presets.SearchParameters.CutParameters.StaticCutSize;
                    break;
                case CutStyle.offset:
                    this.rbOffsetBasedCutSize.Checked = true;
                    this.tbCutSizeOffset.Text = presets.SearchParameters.CutParameters.CutSizeAtOffset;
                    this.cbOffsetSize.SelectedItem = presets.SearchParameters.CutParameters.CutSizeOffsetSize;
                    switch (presets.SearchParameters.CutParameters.CutSizeOffsetEndianess)
                    {
                        case Endianness.big:
                            this.cbByteOrder.SelectedItem = "大端序";
                            break;
                        case Endianness.little:
                            this.cbByteOrder.SelectedItem = "小端序";
                            break;
                    }

                    if (!String.IsNullOrEmpty(presets.SearchParameters.CutParameters.CutSizeMultiplier))
                    {
                        this.cbUseLengthMultiplier.Checked = true;
                        this.tbLengthMultiplier.Text = presets.SearchParameters.CutParameters.CutSizeMultiplier;
                    }
                    break;
                case CutStyle.terminator:
                    this.rbUseTerminator.Checked = true;
                    this.tbTerminatorString.Text = presets.SearchParameters.CutParameters.TerminatorString;
                    this.cbTreatTerminatorAsHex.Checked = presets.SearchParameters.CutParameters.TreatTerminatorStringAsHex;
                    this.cbIncludeTerminatorInLength.Checked = presets.SearchParameters.CutParameters.IncludeTerminatorInSize;
                    if (presets.SearchParameters.CutParameters.CutToEofIfTerminatorNotFoundSpecified)
                    {
                        this.cbCutToEOFWhenTerminatorNotFound.Checked = presets.SearchParameters.CutParameters.CutToEofIfTerminatorNotFound;
                    }
                    break;
            }

            this.cbModOffsetTerminator.Checked = false;
            if (presets.SearchParameters.CutParameters.UseModOffsetForTerminatorStringSpecified &&
                presets.SearchParameters.CutParameters.UseModOffsetForTerminatorString)
            {
                this.cbModOffsetTerminator.Checked = true;
                this.tbOffsetModuloTerminatorDivisor.Text = presets.SearchParameters.CutParameters.ModOffsetForTerminatorStringDivisor;
                this.tbOffsetModuloTerminatorResult.Text = presets.SearchParameters.CutParameters.ModOffsetForTerminatorStringResult;
            }

            this.cbAddExtraBytes.Checked = presets.SearchParameters.AddExtraBytes;
            this.tbExtraCutSizeBytes.Text = presets.SearchParameters.AddExtraByteSize;

            this.doOffsetModuloSearchStringCheckbox();
            this.doOffsetModuloTerminatorCheckbox();
            this.UpdateOffsetCountState();
            this.doCbUseLengthMultiplier();
        }

        private OffsetFinderTemplate getPresetForCurrentValues()
        {
            OffsetFinderTemplate preset = new OffsetFinderTemplate();

            // Criteria Section
            preset.SearchParameters.SearchString = this.tbSearchString.Text;
            preset.SearchParameters.TreatSearchStringAsHex = this.cbSearchAsHex.Checked;
            preset.SearchParameters.StartingOffset = this.tbStartingOffset.Text;

            if (this.cbModOffsetSearchString.Checked)
            {
                preset.SearchParameters.UseModOffsetForSearchStringSpecified = true;
                preset.SearchParameters.UseModOffsetForSearchString = true;

                preset.SearchParameters.ModOffsetForSearchStringDivisor = this.tbOffsetModuloSearchStringDivisor.Text;
                preset.SearchParameters.ModOffsetForSearchStringResult = this.tbOffsetModuloSearchStringResult.Text;
            }
            else
            {
                preset.SearchParameters.UseModOffsetForSearchStringSpecified = true;
                preset.SearchParameters.UseModOffsetForSearchString = false;
            }

            preset.SearchParameters.UseOffsetString = this.cbUseOffsetString.Checked;
            preset.SearchParameters.OffsetString = this.tbOffsetString.Text;
            preset.SearchParameters.UseOffsetBytes = this.cbUseOffsetBytes.Checked;
            preset.SearchParameters.OffsetBytes = this.tbOffsetBytes.Text;
            preset.SearchParameters.OffsetCount = this.tbOffsetCount.Text;

            // Cut Options Section
            preset.SearchParameters.SearchStringOffset = this.tbSearchStringOffset.Text;
            preset.SearchParameters.OutputFileExtension = this.tbOutputExtension.Text;
            preset.SearchParameters.MinimumSizeForCutting = this.tbMinSizeForCut.Text;

            // Cut Size Section
            if (this.rbStaticCutSize.Checked)
            {
                preset.SearchParameters.CutParameters.CutStyle = CutStyle.@static;
                preset.SearchParameters.CutParameters.StaticCutSize = this.tbStaticCutsize.Text;

            }
            else if (this.rbOffsetBasedCutSize.Checked)
            {
                preset.SearchParameters.CutParameters.CutStyle = CutStyle.offset;
                preset.SearchParameters.CutParameters.CutSizeAtOffset = this.tbCutSizeOffset.Text;
                preset.SearchParameters.CutParameters.CutSizeOffsetSize = this.cbOffsetSize.SelectedItem.ToString();

                if (this.cbByteOrder.SelectedItem.Equals("小端序"))
                {
                    preset.SearchParameters.CutParameters.CutSizeOffsetEndianessSpecified = true;
                    preset.SearchParameters.CutParameters.CutSizeOffsetEndianess = Endianness.little;
                }
                else if (this.cbByteOrder.SelectedItem.Equals("大端序"))
                {
                    preset.SearchParameters.CutParameters.CutSizeOffsetEndianessSpecified = true;
                    preset.SearchParameters.CutParameters.CutSizeOffsetEndianess = Endianness.big;
                }

                if (!String.IsNullOrEmpty(this.tbLengthMultiplier.Text))
                {
                    preset.SearchParameters.CutParameters.CutSizeMultiplier = this.tbLengthMultiplier.Text;
                }
            }
            else if (this.rbUseTerminator.Checked)
            {
                preset.SearchParameters.CutParameters.CutStyle = CutStyle.terminator;
                preset.SearchParameters.CutParameters.TerminatorString = this.tbTerminatorString.Text;
                preset.SearchParameters.CutParameters.TreatTerminatorStringAsHexSpecified = this.cbTreatTerminatorAsHex.Checked;
                preset.SearchParameters.CutParameters.TreatTerminatorStringAsHex = this.cbTreatTerminatorAsHex.Checked;
                preset.SearchParameters.CutParameters.IncludeTerminatorInSizeSpecified = true;
                preset.SearchParameters.CutParameters.IncludeTerminatorInSize = this.cbIncludeTerminatorInLength.Checked;
                preset.SearchParameters.CutParameters.CutToEofIfTerminatorNotFoundSpecified = true;
                preset.SearchParameters.CutParameters.CutToEofIfTerminatorNotFound = this.cbCutToEOFWhenTerminatorNotFound.Checked;
                if (this.cbModOffsetTerminator.Checked)
                {
                    preset.SearchParameters.CutParameters.UseModOffsetForTerminatorStringSpecified = true;
                    preset.SearchParameters.CutParameters.UseModOffsetForTerminatorString = true;

                    preset.SearchParameters.CutParameters.ModOffsetForTerminatorStringDivisor = this.tbOffsetModuloTerminatorDivisor.Text;
                    preset.SearchParameters.CutParameters.ModOffsetForTerminatorStringResult = this.tbOffsetModuloTerminatorResult.Text;

                }
            }

            preset.SearchParameters.AddExtraBytes = this.cbAddExtraBytes.Checked;
            preset.SearchParameters.AddExtraByteSize = this.tbExtraCutSizeBytes.Text;

            return preset;
        }
        private void btnSavePreset_Click(object sender, EventArgs e)
        {
            OffsetFinderTemplate preset = getPresetForCurrentValues();

            if (preset != null)
            {
                SavePresetForm saveForm = new SavePresetForm(preset);
                saveForm.Show();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.loadOffsetPlugins();
        }

        private void cbModOffsetTerminator_CheckedChanged(object sender, EventArgs e)
        {
            doOffsetModuloTerminatorCheckbox();
        }

        private void doOffsetModuloTerminatorCheckbox()
        {
            if (!cbModOffsetTerminator.Checked)
            {
                this.tbOffsetModuloTerminatorDivisor.ReadOnly = true;
                this.tbOffsetModuloTerminatorDivisor.Enabled = false;
                this.tbOffsetModuloTerminatorDivisor.Clear();

                this.tbOffsetModuloTerminatorResult.ReadOnly = true;
                this.tbOffsetModuloTerminatorResult.Enabled = false;
                this.tbOffsetModuloTerminatorResult.Clear();
            }
            else
            {
                this.tbOffsetModuloTerminatorDivisor.ReadOnly = false;
                this.tbOffsetModuloTerminatorDivisor.Enabled = true;
                this.tbOffsetModuloTerminatorDivisor.Clear();

                this.tbOffsetModuloTerminatorResult.ReadOnly = false;
                this.tbOffsetModuloTerminatorResult.Enabled = true;
                this.tbOffsetModuloTerminatorResult.Clear();
            }
        }

        private void doOffsetModuloSearchStringCheckbox()
        {
            if (!cbModOffsetSearchString.Checked)
            {
                this.tbOffsetModuloSearchStringDivisor.ReadOnly = true;
                this.tbOffsetModuloSearchStringDivisor.Enabled = false;
                this.tbOffsetModuloSearchStringDivisor.Clear();

                this.tbOffsetModuloSearchStringResult.ReadOnly = true;
                this.tbOffsetModuloSearchStringResult.Enabled = false;
                this.tbOffsetModuloSearchStringResult.Clear();
            }
            else
            {
                this.tbOffsetModuloSearchStringDivisor.ReadOnly = false;
                this.tbOffsetModuloSearchStringDivisor.Enabled = true;
                this.tbOffsetModuloSearchStringDivisor.Clear();

                this.tbOffsetModuloSearchStringResult.ReadOnly = false;
                this.tbOffsetModuloSearchStringResult.Enabled = true;
                this.tbOffsetModuloSearchStringResult.Clear();
            }
        }

        private void cbModOffsetSearchString_CheckedChanged(object sender, EventArgs e)
        {
            this.doOffsetModuloSearchStringCheckbox();
        }

        private void btnBrowseOutputFolder_Click(object sender, EventArgs e)
        {
            this.tbOutputFolder.Text = base.browseForFolder(sender, e);
        }

        private void doCbUseLengthMultiplier()
        {
            if (this.cbUseLengthMultiplier.Checked)
            {
                this.tbLengthMultiplier.Enabled = true;
                this.tbLengthMultiplier.ReadOnly = false;
            }
            else
            {
                this.tbLengthMultiplier.Clear();
                this.tbLengthMultiplier.Enabled = false;
                this.tbLengthMultiplier.ReadOnly = true;
            }

        }
        private void cbUseLengthMultiplier_CheckedChanged(object sender, EventArgs e)
        {
            this.doCbUseLengthMultiplier();
        }
        private void cbUseOffsetString_CheckedChanged(object sender, EventArgs e)
        {
            tbOffsetString.Enabled = cbUseOffsetString.Checked;
            UpdateOffsetCountState();
        }

        private void cbUseOffsetBytes_CheckedChanged(object sender, EventArgs e)
        {
            tbOffsetBytes.Enabled = cbUseOffsetBytes.Checked;
            UpdateOffsetCountState();
        }

        private void UpdateOffsetCountState()
        {
            bool enableCount = cbUseOffsetString.Checked || cbUseOffsetBytes.Checked;
            lblOffsetCount.Enabled = enableCount;
            tbOffsetCount.Enabled = enableCount;
        }
    }
}
