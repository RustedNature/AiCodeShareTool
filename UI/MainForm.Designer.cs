
namespace AiCodeShareTool.UI
{
    partial class MainForm
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
            lblProjectDir = new Label();
            txtProjectDir = new TextBox();
            btnBrowseProject = new Button();
            lblExportPath = new Label();
            txtExportPath = new TextBox();
            btnBrowseExport = new Button();
            lblImportPath = new Label();
            txtImportPath = new TextBox();
            btnBrowseImport = new Button();
            btnExport = new Button();
            btnImport = new Button();
            rtbStatus = new RichTextBox();
            lblStatus = new Label();
            progressBar = new ProgressBar();
            lblLanguageProfile = new Label();
            cmbLanguageProfile = new ComboBox();
            SuspendLayout();
            // 
            // lblProjectDir
            // 
            lblProjectDir.AutoSize = true;
            lblProjectDir.Location = new Point(12, 15);
            lblProjectDir.Name = "lblProjectDir";
            lblProjectDir.Size = new Size(124, 20);
            lblProjectDir.TabIndex = 0;
            lblProjectDir.Text = "Project Directory:";
            // 
            // txtProjectDir
            // 
            txtProjectDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtProjectDir.Location = new Point(142, 12);
            txtProjectDir.Name = "txtProjectDir";
            txtProjectDir.Size = new Size(525, 27);
            txtProjectDir.TabIndex = 1;
            // 
            // btnBrowseProject
            // 
            btnBrowseProject.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseProject.Location = new Point(673, 11);
            btnBrowseProject.Name = "btnBrowseProject";
            btnBrowseProject.Size = new Size(94, 29);
            btnBrowseProject.TabIndex = 2;
            btnBrowseProject.Text = "Browse...";
            btnBrowseProject.UseVisualStyleBackColor = true;
            btnBrowseProject.Click += btnBrowseProject_Click;
            // 
            // lblExportPath
            // 
            lblExportPath.AutoSize = true;
            lblExportPath.Location = new Point(12, 50);
            lblExportPath.Name = "lblExportPath";
            lblExportPath.Size = new Size(116, 20);
            lblExportPath.TabIndex = 3;
            lblExportPath.Text = "Export File Path:";
            // 
            // txtExportPath
            // 
            txtExportPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtExportPath.Location = new Point(142, 47);
            txtExportPath.Name = "txtExportPath";
            txtExportPath.Size = new Size(525, 27);
            txtExportPath.TabIndex = 4;
            // 
            // btnBrowseExport
            // 
            btnBrowseExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseExport.Location = new Point(673, 46);
            btnBrowseExport.Name = "btnBrowseExport";
            btnBrowseExport.Size = new Size(94, 29);
            btnBrowseExport.TabIndex = 5;
            btnBrowseExport.Text = "Browse...";
            btnBrowseExport.UseVisualStyleBackColor = true;
            btnBrowseExport.Click += btnBrowseExport_Click;
            // 
            // lblImportPath
            // 
            lblImportPath.AutoSize = true;
            lblImportPath.Location = new Point(12, 85);
            lblImportPath.Name = "lblImportPath";
            lblImportPath.Size = new Size(117, 20);
            lblImportPath.TabIndex = 6;
            lblImportPath.Text = "Import File Path:";
            // 
            // txtImportPath
            // 
            txtImportPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtImportPath.Location = new Point(142, 82);
            txtImportPath.Name = "txtImportPath";
            txtImportPath.Size = new Size(525, 27);
            txtImportPath.TabIndex = 7;
            // 
            // btnBrowseImport
            // 
            btnBrowseImport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseImport.Location = new Point(673, 81);
            btnBrowseImport.Name = "btnBrowseImport";
            btnBrowseImport.Size = new Size(94, 29);
            btnBrowseImport.TabIndex = 8;
            btnBrowseImport.Text = "Browse...";
            btnBrowseImport.UseVisualStyleBackColor = true;
            btnBrowseImport.Click += btnBrowseImport_Click;
            // 
            // btnExport
            // 
            btnExport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExport.Location = new Point(12, 165);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(180, 35);
            btnExport.TabIndex = 11;
            btnExport.Text = "Export Project";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnImport
            // 
            btnImport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnImport.Location = new Point(198, 165);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(180, 35);
            btnImport.TabIndex = 12;
            btnImport.Text = "Import Code";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // rtbStatus
            // 
            rtbStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbStatus.BackColor = SystemColors.Window;
            rtbStatus.Location = new Point(12, 234);
            rtbStatus.Name = "rtbStatus";
            rtbStatus.ReadOnly = true;
            rtbStatus.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbStatus.Size = new Size(755, 235);
            rtbStatus.TabIndex = 15;
            rtbStatus.Text = "";
            rtbStatus.LinkClicked += (sender, e) => {
                try {
                     if (e.LinkText != null) {
                          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.LinkText) { UseShellExecute = true });
                     }
                } catch (Exception ex) { /* Handle exceptions if needed */ MessageBox.Show($"Could not open link: {ex.Message}"); }
            };
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 211);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(52, 20);
            lblStatus.TabIndex = 14;
            lblStatus.Text = "Status:";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(384, 169);
            progressBar.MarqueeAnimationSpeed = 50;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(383, 28);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 13;
            progressBar.Visible = false;
            // 
            // lblLanguageProfile
            // 
            lblLanguageProfile.AutoSize = true;
            lblLanguageProfile.Location = new Point(12, 124);
            lblLanguageProfile.Name = "lblLanguageProfile";
            lblLanguageProfile.Size = new Size(122, 20);
            lblLanguageProfile.TabIndex = 9;
            lblLanguageProfile.Text = "Language Profile:";
            // 
            // cmbLanguageProfile
            // 
            cmbLanguageProfile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLanguageProfile.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguageProfile.FormattingEnabled = true;
            cmbLanguageProfile.Location = new Point(142, 121);
            cmbLanguageProfile.Name = "cmbLanguageProfile";
            cmbLanguageProfile.Size = new Size(625, 28);
            cmbLanguageProfile.TabIndex = 10;
            cmbLanguageProfile.SelectedIndexChanged += cmbLanguageProfile_SelectedIndexChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(779, 481);
            Controls.Add(cmbLanguageProfile);
            Controls.Add(lblLanguageProfile);
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            Controls.Add(rtbStatus);
            Controls.Add(btnImport);
            Controls.Add(btnExport);
            Controls.Add(btnBrowseImport);
            Controls.Add(txtImportPath);
            Controls.Add(lblImportPath);
            Controls.Add(btnBrowseExport);
            Controls.Add(txtExportPath);
            Controls.Add(lblExportPath);
            Controls.Add(btnBrowseProject);
            Controls.Add(txtProjectDir);
            Controls.Add(lblProjectDir);
            MinimumSize = new Size(500, 450);
            Name = "MainForm";
            Text = "AI Code Share Tool";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblProjectDir;
        private TextBox txtProjectDir;
        private Button btnBrowseProject;
        private Label lblExportPath;
        private TextBox txtExportPath;
        private Button btnBrowseExport;
        private Label lblImportPath;
        private TextBox txtImportPath;
        private Button btnBrowseImport;
        private Button btnExport;
        private Button btnImport;
        private RichTextBox rtbStatus;
        private Label lblStatus;
        private ProgressBar progressBar;
        private Label lblLanguageProfile;
        private ComboBox cmbLanguageProfile;
    }
}