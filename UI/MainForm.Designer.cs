
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
            components = new System.ComponentModel.Container();
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
            btnPreviewExport = new Button();
            toolTipMain = new ToolTip(components);
            btnCopyToClipboard = new Button();
            btnClearStatus = new Button();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            fileReloadProfilesMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            fileExitMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            helpAboutMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lblProjectDir
            // 
            lblProjectDir.AutoSize = true;
            lblProjectDir.Location = new Point(12, 45);
            lblProjectDir.Name = "lblProjectDir";
            lblProjectDir.Size = new Size(124, 20);
            lblProjectDir.TabIndex = 1;
            lblProjectDir.Text = "Project Directory:";
            // 
            // txtProjectDir
            // 
            txtProjectDir.AllowDrop = true;
            txtProjectDir.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtProjectDir.Location = new Point(142, 42);
            txtProjectDir.Name = "txtProjectDir";
            txtProjectDir.Size = new Size(525, 27);
            txtProjectDir.TabIndex = 2;
            // 
            // btnBrowseProject
            // 
            btnBrowseProject.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseProject.Location = new Point(673, 41);
            btnBrowseProject.Name = "btnBrowseProject";
            btnBrowseProject.Size = new Size(94, 29);
            btnBrowseProject.TabIndex = 3;
            btnBrowseProject.Text = "Browse...";
            btnBrowseProject.UseVisualStyleBackColor = true;
            btnBrowseProject.Click += btnBrowseProject_Click;
            // 
            // lblExportPath
            // 
            lblExportPath.AutoSize = true;
            lblExportPath.Location = new Point(12, 80);
            lblExportPath.Name = "lblExportPath";
            lblExportPath.Size = new Size(116, 20);
            lblExportPath.TabIndex = 4;
            lblExportPath.Text = "Export File Path:";
            // 
            // txtExportPath
            // 
            txtExportPath.AllowDrop = true;
            txtExportPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtExportPath.Location = new Point(142, 77);
            txtExportPath.Name = "txtExportPath";
            txtExportPath.Size = new Size(525, 27);
            txtExportPath.TabIndex = 5;
            // 
            // btnBrowseExport
            // 
            btnBrowseExport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseExport.Location = new Point(673, 76);
            btnBrowseExport.Name = "btnBrowseExport";
            btnBrowseExport.Size = new Size(94, 29);
            btnBrowseExport.TabIndex = 6;
            btnBrowseExport.Text = "Browse...";
            btnBrowseExport.UseVisualStyleBackColor = true;
            btnBrowseExport.Click += btnBrowseExport_Click;
            // 
            // lblImportPath
            // 
            lblImportPath.AutoSize = true;
            lblImportPath.Location = new Point(12, 115);
            lblImportPath.Name = "lblImportPath";
            lblImportPath.Size = new Size(117, 20);
            lblImportPath.TabIndex = 7;
            lblImportPath.Text = "Import File Path:";
            // 
            // txtImportPath
            // 
            txtImportPath.AllowDrop = true;
            txtImportPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtImportPath.Location = new Point(142, 112);
            txtImportPath.Name = "txtImportPath";
            txtImportPath.Size = new Size(525, 27);
            txtImportPath.TabIndex = 8;
            // 
            // btnBrowseImport
            // 
            btnBrowseImport.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowseImport.Location = new Point(673, 111);
            btnBrowseImport.Name = "btnBrowseImport";
            btnBrowseImport.Size = new Size(94, 29);
            btnBrowseImport.TabIndex = 9;
            btnBrowseImport.Text = "Browse...";
            btnBrowseImport.UseVisualStyleBackColor = true;
            btnBrowseImport.Click += btnBrowseImport_Click;
            // 
            // btnExport
            // 
            btnExport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnExport.Location = new Point(158, 193);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(140, 35);
            btnExport.TabIndex = 13;
            btnExport.Text = "Export Project";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // btnImport
            // 
            btnImport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnImport.Location = new Point(450, 193);
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(140, 35);
            btnImport.TabIndex = 15;
            btnImport.Text = "Import Code";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += btnImport_Click;
            // 
            // rtbStatus
            // 
            rtbStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbStatus.BackColor = SystemColors.Window;
            rtbStatus.Location = new Point(12, 265);
            rtbStatus.Name = "rtbStatus";
            rtbStatus.ReadOnly = true;
            rtbStatus.ScrollBars = RichTextBoxScrollBars.Vertical;
            rtbStatus.Size = new Size(755, 204);
            rtbStatus.TabIndex = 19;
            rtbStatus.Text = "";
            // 
            // Corrected: Assign lambda directly here
            // 
            rtbStatus.LinkClicked += (sender, e) => {
                try {
                     if (e?.LinkText != null) {
                          System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.LinkText) { UseShellExecute = true });
                     }
                } catch (Exception ex) { /* Handle exceptions if needed */ MessageBox.Show($"Could not open link: {ex.Message}"); }
            };
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 242);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(52, 20);
            lblStatus.TabIndex = 17;
            lblStatus.Text = "Status:";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(596, 197);
            progressBar.MarqueeAnimationSpeed = 50;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(171, 28);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 16;
            progressBar.Visible = false;
            // 
            // lblLanguageProfile
            // 
            lblLanguageProfile.AutoSize = true;
            lblLanguageProfile.Location = new Point(12, 154);
            lblLanguageProfile.Name = "lblLanguageProfile";
            lblLanguageProfile.Size = new Size(122, 20);
            lblLanguageProfile.TabIndex = 10;
            lblLanguageProfile.Text = "Language Profile:";
            // 
            // cmbLanguageProfile
            // 
            cmbLanguageProfile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLanguageProfile.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguageProfile.FormattingEnabled = true;
            cmbLanguageProfile.Location = new Point(142, 151);
            cmbLanguageProfile.Name = "cmbLanguageProfile";
            cmbLanguageProfile.Size = new Size(625, 28);
            cmbLanguageProfile.TabIndex = 11;
            cmbLanguageProfile.SelectedIndexChanged += cmbLanguageProfile_SelectedIndexChanged;
            // 
            // btnPreviewExport
            // 
            btnPreviewExport.Location = new Point(12, 193);
            btnPreviewExport.Name = "btnPreviewExport";
            btnPreviewExport.Size = new Size(140, 35);
            btnPreviewExport.TabIndex = 12;
            btnPreviewExport.Text = "Preview Export";
            btnPreviewExport.UseVisualStyleBackColor = true;
            btnPreviewExport.Click += btnPreviewExport_Click;
            // 
            // btnCopyToClipboard
            // 
            btnCopyToClipboard.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCopyToClipboard.Location = new Point(304, 193);
            btnCopyToClipboard.Name = "btnCopyToClipboard";
            btnCopyToClipboard.Size = new Size(140, 35);
            btnCopyToClipboard.TabIndex = 14;
            btnCopyToClipboard.Text = "Copy to Clipboard";
            btnCopyToClipboard.UseVisualStyleBackColor = true;
            btnCopyToClipboard.Click += btnCopyToClipboard_Click;
            // 
            // btnClearStatus
            // 
            btnClearStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearStatus.Location = new Point(692, 238);
            btnClearStatus.Name = "btnClearStatus";
            btnClearStatus.Size = new Size(75, 29);
            btnClearStatus.TabIndex = 18;
            btnClearStatus.Text = "Clear";
            btnClearStatus.UseVisualStyleBackColor = true;
            btnClearStatus.Click += btnClearStatus_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(779, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { fileReloadProfilesMenuItem, toolStripSeparator1, fileExitMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "&File";
            // 
            // fileReloadProfilesMenuItem
            // 
            fileReloadProfilesMenuItem.Name = "fileReloadProfilesMenuItem";
            fileReloadProfilesMenuItem.Size = new Size(190, 26);
            fileReloadProfilesMenuItem.Text = "&Reload Profiles";
            fileReloadProfilesMenuItem.Click += fileReloadProfilesMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(187, 6);
            // 
            // fileExitMenuItem
            // 
            fileExitMenuItem.Name = "fileExitMenuItem";
            fileExitMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            fileExitMenuItem.Size = new Size(190, 26);
            fileExitMenuItem.Text = "E&xit";
            fileExitMenuItem.Click += fileExitMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { helpAboutMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(55, 24);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // helpAboutMenuItem
            // 
            helpAboutMenuItem.Name = "helpAboutMenuItem";
            helpAboutMenuItem.Size = new Size(133, 26);
            helpAboutMenuItem.Text = "&About";
            helpAboutMenuItem.Click += helpAboutMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(779, 481);
            Controls.Add(btnClearStatus);
            Controls.Add(btnCopyToClipboard);
            Controls.Add(btnPreviewExport);
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
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(600, 500);
            Name = "MainForm";
            Text = "AI Code Share Tool";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
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
        private Button btnPreviewExport;
        private ToolTip toolTipMain;
        private Button btnCopyToClipboard;
        private Button btnClearStatus;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem fileReloadProfilesMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem fileExitMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem helpAboutMenuItem;
    }
}