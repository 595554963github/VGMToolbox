namespace VGMToolbox.plugin
{
    partial class AVgmtForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AVgmtForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.pnlLabels = new System.Windows.Forms.Panel();
            this.lblProgressLabel = new System.Windows.Forms.Label();
            this.lblGears = new System.Windows.Forms.Label();
            this.lblTimeElapsed = new System.Windows.Forms.Label();
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnDoTask = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1.SuspendLayout();
            this.pnlLabels.SuspendLayout();
            this.pnlTitle.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 341);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(539, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.AutoSize = false;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(200, 17);
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.AutoSize = false;
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(320, 16);
            // 
            // pnlLabels
            // 
            this.pnlLabels.Controls.Add(this.lblProgressLabel);
            this.pnlLabels.Controls.Add(this.lblGears);
            this.pnlLabels.Controls.Add(this.lblTimeElapsed);
            this.pnlLabels.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLabels.Location = new System.Drawing.Point(0, 323);
            this.pnlLabels.Name = "pnlLabels";
            this.pnlLabels.Size = new System.Drawing.Size(539, 18);
            this.pnlLabels.TabIndex = 1;
            // 
            // lblProgressLabel
            // 
            this.lblProgressLabel.AutoSize = true;
            this.lblProgressLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblProgressLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblProgressLabel.Location = new System.Drawing.Point(75, 0);
            this.lblProgressLabel.MinimumSize = new System.Drawing.Size(456, 0);
            this.lblProgressLabel.Name = "lblProgressLabel";
            this.lblProgressLabel.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblProgressLabel.Size = new System.Drawing.Size(456, 16);
            this.lblProgressLabel.TabIndex = 1;
            // 
            // lblGears
            // 
            this.lblGears.AutoSize = true;
            this.lblGears.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblGears.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblGears.Image = ((System.Drawing.Image)(resources.GetObject("lblGears.Image")));
            this.lblGears.Location = new System.Drawing.Point(55, 0);
            this.lblGears.MinimumSize = new System.Drawing.Size(20, 0);
            this.lblGears.Name = "lblGears";
            this.lblGears.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblGears.Size = new System.Drawing.Size(20, 16);
            this.lblGears.TabIndex = 2;
            // 
            // lblTimeElapsed
            // 
            this.lblTimeElapsed.AutoSize = true;
            this.lblTimeElapsed.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblTimeElapsed.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTimeElapsed.Location = new System.Drawing.Point(0, 0);
            this.lblTimeElapsed.Name = "lblTimeElapsed";
            this.lblTimeElapsed.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.lblTimeElapsed.Size = new System.Drawing.Size(55, 16);
            this.lblTimeElapsed.TabIndex = 0;
            this.lblTimeElapsed.Text = "00:00:00";
            // 
            // pnlTitle
            // 
            this.pnlTitle.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.pnlTitle.Location = new System.Drawing.Point(0, 3);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(539, 18);
            this.pnlTitle.TabIndex = 3;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(87, 15);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "章节标题";
            // 
            // tbOutput
            // 
            this.tbOutput.AcceptsReturn = true;
            this.tbOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tbOutput.Location = new System.Drawing.Point(0, 252);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbOutput.Size = new System.Drawing.Size(539, 71);
            this.tbOutput.TabIndex = 2;
            this.toolTip1.SetToolTip(this.tbOutput, "双击在默认文本编辑器中查看.");
            this.tbOutput.DoubleClick += new System.EventHandler(this.tbOutput_DoubleClick);
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnDoTask);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(0, 234);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(539, 18);
            this.pnlButtons.TabIndex = 4;
            // 
            // btnDoTask
            // 
            this.btnDoTask.AutoSize = true;
            this.btnDoTask.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnDoTask.Location = new System.Drawing.Point(419, 0);
            this.btnDoTask.Name = "btnDoTask";
            this.btnDoTask.Size = new System.Drawing.Size(60, 18);
            this.btnDoTask.TabIndex = 1;
            this.btnDoTask.Text = "执行任务";
            this.btnDoTask.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCancel.Location = new System.Drawing.Point(479, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(60, 18);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.backgroundWorker_Cancel);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // AVgmtForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 363);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.pnlTitle);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.pnlLabels);
            this.Controls.Add(this.statusStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AVgmtForm";
            this.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.pnlLabels.ResumeLayout(false);
            this.pnlLabels.PerformLayout();
            this.pnlTitle.ResumeLayout(false);
            this.pnlTitle.PerformLayout();
            this.pnlButtons.ResumeLayout(false);
            this.pnlButtons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.StatusStrip statusStrip1;
        protected System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        protected System.Windows.Forms.ToolStripProgressBar toolStripProgressBar;
        protected System.Windows.Forms.Panel pnlLabels;
        protected System.Windows.Forms.Panel pnlTitle;
        protected System.Windows.Forms.Label lblTitle;
        protected System.Windows.Forms.Label lblProgressLabel;
        protected System.Windows.Forms.Label lblTimeElapsed;
        protected System.Windows.Forms.TextBox tbOutput;
        protected System.Windows.Forms.Panel pnlButtons;
        protected System.Windows.Forms.Button btnCancel;
        protected System.Windows.Forms.Button btnDoTask;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        protected System.Windows.Forms.ToolTip toolTip1;
        protected System.Windows.Forms.Label lblGears;
    }
}