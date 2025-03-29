using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Drawing;
using System.Windows.Forms;
using System;
using VGMToolbox.forms;

namespace YourNamespace // 请替换为实际的命名空间
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(AboutForm));
            LinkLabel linkLabelHomePage = new LinkLabel();
            Button okButton = new Button();
            TextBox tbMain = new TextBox();
            LinkLabel linkLabelSupport = new LinkLabel();
            Label label1 = new Label();
            Label label2 = new Label();
            LinkLabel linkLabelGithub = new LinkLabel();

            SuspendLayout();

            // linkLabelHomePage 属性设置
            linkLabelHomePage.AutoSize = true;
            linkLabelHomePage.Location = new Point(14, 220);
            linkLabelHomePage.Margin = new Padding(4, 0, 4, 0);
            linkLabelHomePage.Name = "linkLabelHomePage";
            linkLabelHomePage.Size = new Size(89, 18);
            linkLabelHomePage.TabIndex = 2;
            linkLabelHomePage.TabStop = true;
            linkLabelHomePage.Text = "主页";
            linkLabelHomePage.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel_LinkClicked);

            // okButton 属性设置
            okButton.Location = new Point(153, 327);
            okButton.Margin = new Padding(4);
            okButton.Name = "okButton";
            okButton.Size = new Size(112, 32);
            okButton.TabIndex = 0;
            okButton.Text = "关闭";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += new EventHandler(okButton_Click);

            // tbMain 属性设置
            tbMain.AcceptsReturn = true;
            tbMain.BackColor = SystemColors.ControlLightLight;
            tbMain.Location = new Point(18, 17);
            tbMain.Margin = new Padding(4);
            tbMain.Multiline = true;
            tbMain.Name = "tbMain";
            tbMain.ReadOnly = true;
            tbMain.ScrollBars = ScrollBars.Vertical;
            tbMain.Size = new Size(428, 155);
            tbMain.TabIndex = 1;
            tbMain.Text = manager.GetString("tbMain.Text");

            // linkLabelSupport 属性设置
            linkLabelSupport.AutoSize = true;
            linkLabelSupport.Location = new Point(14, 245);
            linkLabelSupport.Margin = new Padding(4, 0, 4, 0);
            linkLabelSupport.Name = "linkLabelSupport";
            linkLabelSupport.Size = new Size(161, 18);
            linkLabelSupport.TabIndex = 3;
            linkLabelSupport.TabStop = true;
            linkLabelSupport.Text = "支持/问题咨询";
            linkLabelSupport.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel_LinkClicked);

            // label1 属性设置
            label1.Location = new Point(14, 177);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(390, 43);
            label1.TabIndex = 4;
            label1.Text = "VGMToolbox 是免费软件,采用 MIT 许可证授权.";

            // label2 属性设置
            label2.AutoSize = true;
            label2.Location = new Point(14, 270);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(251, 18);
            label2.TabIndex = 5;
            label2.Text = "邮箱: vgmtoolbox@gmail.com";

            // linkLabelGithub 属性设置
            linkLabelGithub.AutoSize = true;
            linkLabelGithub.Location = new Point(14, 307);
            linkLabelGithub.Name = "linkLabelGithub";
            linkLabelGithub.Size = new Size(197, 18);
            linkLabelGithub.TabIndex = 6;
            linkLabelGithub.TabStop = true;
            linkLabelGithub.Text = "在 Github 上查看版本更新";
            linkLabelGithub.LinkClicked += new LinkLabelLinkClickedEventHandler(linkLabel_LinkClicked);

            AutoScaleDimensions = new SizeF(9f, 18f);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new Size(464, 371);
            Controls.Add(linkLabelGithub);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(linkLabelSupport);
            Controls.Add(tbMain);
            Controls.Add(okButton);
            Controls.Add(linkLabelHomePage);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new Padding(4);
            Name = "AboutForm";
            Text = "关于 VGMToolbox";
            ResumeLayout(false);
            PerformLayout();
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel link = (LinkLabel)sender;
            System.Diagnostics.Process.Start(link.Text);
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}