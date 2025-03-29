using System;
using System.IO;
using System.Windows.Forms;

using VGMToolbox.format;
using VGMToolbox.util;

namespace VGMToolbox.forms.examine
{
    public partial class SingleTagUpdateForm : Form
    {
        VGMToolbox.util.NodeTagStruct nodeTagInfo;
        ISingleTagFormat vgmData;
        
        public SingleTagUpdateForm(VGMToolbox.util.NodeTagStruct pNts)
        {
            nodeTagInfo = pNts;
            
            InitializeComponent();

            loadCurrentTagInformation();
        }

        private void loadCurrentTagInformation()
        {
            using (FileStream fs =
                File.Open(this.nodeTagInfo.FilePath, FileMode.Open, FileAccess.Read))
            {
                this.vgmData =
                    (ISingleTagFormat)Activator.CreateInstance(Type.GetType(this.nodeTagInfo.ObjectType));
                this.vgmData.Initialize(fs, this.nodeTagInfo.FilePath);

                this.tbTag.Text = this.vgmData.GetTagAsText();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.vgmData.UpdateTag(this.tbTag.Text);

            MessageBox.Show(String.Format("\"{0}\"的标签已更新,在您再次添加文件之前,更改不会显示在树中.", Path.GetFileName(this.vgmData.FilePath)));
            this.Close();
            this.Dispose();
        }
    }
}
