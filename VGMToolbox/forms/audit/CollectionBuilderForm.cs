using System.IO;
using System.Windows.Forms;

using VGMToolbox.dbutil;
using VGMToolbox.plugin;

namespace VGMToolbox.forms.audit
{
    public partial class CollectionBuilderForm : VgmtForm
    {
        private static readonly string DB_PATH =
            Path.Combine(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "db"), "collection.s3db");

        public CollectionBuilderForm(TreeNode pTreeNode)
            : base(pTreeNode)
        {
            this.lblTitle.Text = "收藏生成器";

            InitializeComponent();

            loadSystemList();
        }

        private void loadSystemList()
        {
            this.comboBox1.DataSource = SqlLiteUtil.GetSimpleDataTable(DB_PATH, "系统", "系统名称");
            this.comboBox1.DisplayMember = "系统名称";
            this.comboBox1.ValueMember = "系统ID";
        }
    }
}
