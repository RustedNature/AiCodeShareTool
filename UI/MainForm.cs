
using System.Text; // Required for StringBuilder
using AiCodeShareTool.Configuration;
using AiCodeShareTool.Core;

namespace AiCodeShareTool.UI
{
    public partial class MainForm : Form
    {
        private readonly ApplicationState _appState;
        private readonly IConfigurationService _configService;
        private readonly IUserInterface _ui;
        private readonly IExporter _exporter;
        private readonly IImporter _importer;

        // Constructor now accepts ApplicationState and IConfigurationService
        public MainForm(ApplicationState appState, IConfigurationService configService)
        {
            InitializeComponent();

            // Store dependencies
            _appState = appState ?? throw new ArgumentNullException(nameof(appState));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));

            // Initialize components that depend on the services
            _ui = new WinFormsUI(this, this.rtbStatus);
            // Pass config service to exporter, importer now uses UI only
            _exporter = new FileSystemExporter(_ui, _configService);
            _importer = new FileSystemImporter(_ui);

            // Configure UI elements
            ConfigureDragDrop();
            SetupTooltips(); // Call method to set up tooltips

            // Load initial paths and populate profile dropdown using ApplicationState (already loaded from settings in Program.cs)
            LoadUiFromState();
            PopulateLanguageProfiles();
            SelectInitialProfile(); // Uses appState.ActiveLanguageProfileName
        }


        private void ConfigureDragDrop()
        {
            // Allow dropping onto TextBoxes
            txtProjectDir.AllowDrop = true;
            txtExportPath.AllowDrop = true;
            txtImportPath.AllowDrop = true;

            // Hook up event handlers
            txtProjectDir.DragEnter += TextBox_DragEnter_Folder;
            txtProjectDir.DragDrop += TextBox_DragDrop_Folder;

            txtExportPath.DragEnter += TextBox_DragEnter_File;
            txtExportPath.DragDrop += TextBox_DragDrop_File;

            txtImportPath.DragEnter += TextBox_DragEnter_File;
            txtImportPath.DragDrop += TextBox_DragDrop_File;
        }

        private void SetupTooltips()
        {
            toolTipMain.SetToolTip(txtProjectDir, "Enter or browse/drag the root directory of the project to export from or import into.");
            toolTipMain.SetToolTip(btnBrowseProject, "Browse for the project's root directory.");
            toolTipMain.SetToolTip(txtExportPath, "Enter or browse/drag the path for the combined text file to be created by the export.");
            toolTipMain.SetToolTip(btnBrowseExport, "Browse for the location to save the exported file.");
            toolTipMain.SetToolTip(txtImportPath, "Enter or browse/drag the path of the combined text file to import code from.");
            toolTipMain.SetToolTip(btnBrowseImport, "Browse for the file containing the code to import.");
            toolTipMain.SetToolTip(cmbLanguageProfile, "Select the language/project type to determine which files are included in the export.");
            toolTipMain.SetToolTip(btnPreviewExport, "Show a list of files that will be included in the export based on the current settings.");
            toolTipMain.SetToolTip(btnExport, "Export the selected project files to the specified text file.");
            toolTipMain.SetToolTip(btnCopyToClipboard, "Export the selected project files directly to the system clipboard.");
            toolTipMain.SetToolTip(btnImport, "Import code from the specified text file into the project directory (overwrites existing files!).");
            toolTipMain.SetToolTip(btnClearStatus, "Clear the status messages below.");
            toolTipMain.SetToolTip(rtbStatus, "Displays status messages, warnings, and errors from operations.");
        }


        private void LoadUiFromState()
        {
            txtProjectDir.Text = _appState.CurrentProjectDirectory ?? "";
            txtExportPath.Text = _appState.CurrentExportFilePath ?? "";
            txtImportPath.Text = _appState.CurrentImportFilePath ?? "";
            // Active profile is handled separately by SelectInitialProfile
        }

        private void PopulateLanguageProfiles()
        {
            int selectedIndex = cmbLanguageProfile.SelectedIndex; // Preserve selection if possible
            string? selectedName = (cmbLanguageProfile.SelectedItem as LanguageProfile)?.Name;

            var profiles = _configService.GetAvailableProfiles();
            cmbLanguageProfile.Items.Clear();
            cmbLanguageProfile.DisplayMember = nameof(LanguageProfile.Name); // Show the Name property
            foreach (var profile in profiles.OrderBy(p => p.Name)) // Sort profiles alphabetically
            {
                cmbLanguageProfile.Items.Add(profile);
            }

            // Try to restore selection
            bool restored = false;
            if (!string.IsNullOrEmpty(selectedName))
            {
                for (int i = 0; i < cmbLanguageProfile.Items.Count; i++)
                {
                    if ((cmbLanguageProfile.Items[i] as LanguageProfile)?.Name == selectedName)
                    {
                        cmbLanguageProfile.SelectedIndex = i;
                        restored = true;
                        break;
                    }
                }
            }
            // Fallback if name not found or no previous selection
            if (!restored && cmbLanguageProfile.Items.Count > 0)
            {
                cmbLanguageProfile.SelectedIndex = 0; // Select the first item
                                                      // Ensure the config service reflects this change if the selection actually changed
                if (cmbLanguageProfile.SelectedItem is LanguageProfile firstProfile)
                {
                    _configService.SetActiveProfile(firstProfile.Name);
                    _appState.ActiveLanguageProfileName = firstProfile.Name;
                }
            }
            else if (cmbLanguageProfile.Items.Count == 0)
            {
                _ui.DisplayError("No language profiles loaded. Check language_profiles.json.");
            }
        }

        private void SelectInitialProfile()
        {
            // Profile name already loaded into _appState from Settings in Program.cs
            string profileToSelect = _appState.ActiveLanguageProfileName ?? _configService.DefaultProfileName;

            for (int i = 0; i < cmbLanguageProfile.Items.Count; i++)
            {
                if (cmbLanguageProfile.Items[i] is LanguageProfile profile && profile.Name.Equals(profileToSelect, StringComparison.OrdinalIgnoreCase))
                {
                    cmbLanguageProfile.SelectedIndex = i;
                    // Update the active profile in the service to match the potentially loaded state
                    _configService.SetActiveProfile(profile.Name);
                    // No initial message needed, avoid clutter
                    return;
                }
            }

            // Fallback if loaded name not found (e.g., removed from JSON)
            _ui.DisplayWarning($"Saved profile '{profileToSelect}' not found. Selecting default.");
            profileToSelect = _configService.DefaultProfileName; // Use default
            for (int i = 0; i < cmbLanguageProfile.Items.Count; i++)
            {
                if (cmbLanguageProfile.Items[i] is LanguageProfile profile && profile.Name.Equals(profileToSelect, StringComparison.OrdinalIgnoreCase))
                {
                    cmbLanguageProfile.SelectedIndex = i;
                    _configService.SetActiveProfile(profile.Name);
                    _appState.ActiveLanguageProfileName = profile.Name; // Update state to reflect fallback
                    return;
                }
            }

            // Further fallback: select first item if default also missing
            if (cmbLanguageProfile.Items.Count > 0)
            {
                cmbLanguageProfile.SelectedIndex = 0;
                if (cmbLanguageProfile.SelectedItem is LanguageProfile selectedProfile)
                {
                    _configService.SetActiveProfile(selectedProfile.Name);
                    _appState.ActiveLanguageProfileName = selectedProfile.Name; // Update state
                    _ui.DisplayWarning($"Default profile '{profileToSelect}' also not found. Selecting first available: {selectedProfile.Name}");
                }
            }
            else
            {
                // No profiles available at all!
                _ui.DisplayError("No language profiles available. Please check language_profiles.json.");
                _appState.ActiveLanguageProfileName = null;
            }
        }

        private void cmbLanguageProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLanguageProfile.SelectedItem is LanguageProfile selectedProfile)
            {
                bool success = _configService.SetActiveProfile(selectedProfile.Name);
                _appState.ActiveLanguageProfileName = selectedProfile.Name; // Update state for persistence
                if (success)
                {
                    // _ui.DisplayMessage($"Active language profile changed to: {selectedProfile.Name}"); // Maybe too noisy
                }
                else
                {
                    _ui.DisplayWarning($"Could not activate selected profile: {selectedProfile.Name}");
                }
            }
        }


        private void btnBrowseProject_Click(object sender, EventArgs e)
        {
            string? selectedPath = _ui.GetDirectoryPath("Select Project Directory", _appState.CurrentProjectDirectory);
            if (selectedPath != null)
            {
                UpdateProjectPath(selectedPath);
            }
        }

        private void btnBrowseExport_Click(object sender, EventArgs e)
        {
            string? selectedPath = _ui.GetSaveFilePath("Select Export Output File", "Text Files|*.txt|All Files|*.*", "txt", _appState.CurrentExportFilePath);
            if (selectedPath != null)
            {
                UpdateExportPath(selectedPath);
            }
        }

        private void btnBrowseImport_Click(object sender, EventArgs e)
        {
            string? selectedPath = _ui.GetOpenFilePath("Select Import Code File", "Text Files|*.txt|All Files|*.*", _appState.CurrentImportFilePath);
            if (selectedPath != null)
            {
                UpdateImportPath(selectedPath);
            }
        }

        // --- Helper methods to update state and UI for paths ---
        private void UpdateProjectPath(string path)
        {
            _appState.CurrentProjectDirectory = path;
            txtProjectDir.Text = path;
            _ui.DisplayMessage($"Project Directory set to: {path}");
        }
        private void UpdateExportPath(string path)
        {
            _appState.CurrentExportFilePath = path;
            txtExportPath.Text = path;
            _ui.DisplayMessage($"Export File Path set to: {path}");
            // Convenience: If import path is empty, set it to the same file
            if (string.IsNullOrWhiteSpace(txtImportPath.Text))
            {
                UpdateImportPath(path, false); // Update import path without extra message
                _ui.DisplayMessage($"Import File Path also updated for convenience.");
            }
        }

        private void UpdateImportPath(string path, bool showMessage = true)
        {
            _appState.CurrentImportFilePath = path;
            txtImportPath.Text = path;
            if (showMessage) _ui.DisplayMessage($"Import File Path set to: {path}");
        }


        // --- Drag and Drop Handlers (Corrected Nullability) ---

        private void TextBox_DragEnter_Folder(object? sender, DragEventArgs e) // Changed sender to object?
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
                // Allow drop if exactly one item is dropped and it's a directory
                if (files != null && files.Length == 1 && Directory.Exists(files[0]))
                {
                    e.Effect = DragDropEffects.Copy; // Show copy cursor
                    return;
                }
            }
            e.Effect = DragDropEffects.None; // Otherwise, don't allow drop
        }

        private void TextBox_DragDrop_Folder(object? sender, DragEventArgs e) // Changed sender to object?
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length == 1 && Directory.Exists(files[0]))
                {
                    UpdateProjectPath(files[0]); // Update via helper method
                }
            }
        }

        private void TextBox_DragEnter_File(object? sender, DragEventArgs e) // Changed sender to object?
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
                // Allow drop if exactly one item is dropped and it's a file
                if (files != null && files.Length == 1 && File.Exists(files[0]))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void TextBox_DragDrop_File(object? sender, DragEventArgs e) // Changed sender to object?
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length == 1 && File.Exists(files[0]))
                {
                    // Ensure sender is a TextBox before accessing it
                    if (sender is TextBox tb)
                    {
                        if (tb == txtExportPath)
                        {
                            UpdateExportPath(files[0]);
                        }
                        else if (tb == txtImportPath)
                        {
                            UpdateImportPath(files[0]);
                        }
                    }
                }
            }
        }

        // --- Action Buttons ---

        private async void btnPreviewExport_Click(object sender, EventArgs e)
        {
            _appState.CurrentProjectDirectory = txtProjectDir.Text; // Update state from UI

            if (!ValidateProjectDirectory()) return;
            if (!ValidateProfileSelected()) return;

            SetBusyState(true);
            List<string>? filesToExport = null;
            try
            {
                filesToExport = await Task.Run(() => _exporter.PreviewExport(_appState.CurrentProjectDirectory!)); // Use ! null-forgiving operator after validation
            }
            catch (Exception ex)
            {
                _ui.DisplayError($"An unexpected error occurred during the preview task: {ex.Message}");
            }
            finally { SetBusyState(false); }

            if (filesToExport != null)
            {
                // Show results in a simple message box for now
                // A custom dialog would be better for large lists
                string fileList = string.Join(Environment.NewLine, filesToExport);
                string message = $"Preview: {filesToExport.Count} file(s) would be exported:\n\n{fileList}";

                // Limit message box size
                const int maxDisplayLength = 4000; // Approximate limit
                if (message.Length > maxDisplayLength)
                {
                    message = message.Substring(0, maxDisplayLength) + $"\n\n... (list truncated - {filesToExport.Count} files total)";
                }

                if (filesToExport.Count == 0)
                {
                    MessageBox.Show(this, "Preview: No files found matching the selected profile and criteria.", "Export Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, message, "Export Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            // Errors are handled by the exporter and displayed in the status box already
        }


        private async void btnExport_Click(object sender, EventArgs e)
        {
            _appState.CurrentProjectDirectory = txtProjectDir.Text;
            _appState.CurrentExportFilePath = txtExportPath.Text;

            if (!ValidateProjectDirectory()) return;
            if (!ValidateExportFilePath()) return;
            if (!ValidateProfileSelected()) return;

            SetBusyState(true);
            try
            {
                await Task.Run(() => _exporter.Export(_appState.CurrentProjectDirectory!, _appState.CurrentExportFilePath!));
            }
            catch (Exception ex)
            {
                _ui.DisplayError($"An unexpected error occurred during the export task: {ex.Message}");
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private async void btnCopyToClipboard_Click(object sender, EventArgs e)
        {
            _appState.CurrentProjectDirectory = txtProjectDir.Text;

            if (!ValidateProjectDirectory()) return;
            if (!ValidateProfileSelected()) return;

            SetBusyState(true);
            StringBuilder? exportContent = null;
            long charCount = 0;
            bool exportSuccess = false;

            try
            {
                // ExportToStringBuilder now returns StringBuilder? and has an out param for char count
                exportContent = await Task.Run(() => _exporter.ExportToStringBuilder(_appState.CurrentProjectDirectory!, out charCount));
                // Consider it successful if we got a non-null builder back, even if empty
                exportSuccess = exportContent != null;

                if (exportSuccess) // Check if exportContent is not null
                {
                    if (exportContent!.Length > 0) // Now safe to access Length
                    {
                        // Set clipboard text on UI thread
                        await SetClipboardTextAsync(exportContent.ToString());
                        _ui.DisplaySuccess("Export content copied to clipboard.");
                        _ui.DisplayMessage($"Total characters copied: {charCount:N0}"); // Message already includes count
                    }
                    else
                    {
                        _ui.DisplayWarning("No files found to export, clipboard not modified.");
                    }
                }
                else
                {
                    // Error message should have been displayed by the exporter
                    _ui.DisplayError("Failed to generate export content for clipboard.");
                }
            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException)
                {
                    _ui.DisplayError($"Out of memory trying to build the export string for the clipboard. Try exporting to a file instead. {ex.Message}");
                }
                else
                {
                    _ui.DisplayError($"An unexpected error occurred during the copy to clipboard task: {ex.Message}");
                }
            }
            finally
            {
                SetBusyState(false);
            }
        }

        // Helper to set clipboard text asynchronously on the UI thread
        private Task SetClipboardTextAsync(string text)
        {
            // Check if the form or its handle exists before invoking
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                if (InvokeRequired)
                {
                    // Use BeginInvoke for fire-and-forget, or Invoke if completion needs to be awaited (unlikely for clipboard)
                    return Task.Factory.StartNew(() => Clipboard.SetText(text),
                        CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    try
                    {
                        Clipboard.SetText(text);
                    }
                    catch (System.Runtime.InteropServices.ExternalException clipEx)
                    {
                        _ui.DisplayError($"Failed to set clipboard text: {clipEx.Message}");
                    }
                    return Task.CompletedTask;
                }
            }
            else
            {
                _ui.DisplayWarning("Cannot set clipboard text - form is not ready or disposed.");
                return Task.CompletedTask;
            }
        }


        private async void btnImport_Click(object sender, EventArgs e)
        {
            _appState.CurrentProjectDirectory = txtProjectDir.Text;
            _appState.CurrentImportFilePath = txtImportPath.Text;

            if (!ValidateProjectDirectory()) return;
            if (!ValidateImportFilePath()) return;


            // Ask for confirmation (includes backup question)
            var confirmResult = MessageBox.Show(
                $"This will import files from:\n{_appState.CurrentImportFilePath}\n\nInto directory:\n{_appState.CurrentProjectDirectory}\n\nExisting files WILL BE OVERWRITTEN.\n\nDo you want to create a backup (.zip) of the target directory first?",
                "Confirm Import & Backup",
                MessageBoxButtons.YesNoCancel, // Yes (Backup), No (No Backup), Cancel
                MessageBoxIcon.Warning);


            bool createBackup = false;
            switch (confirmResult)
            {
                case DialogResult.Yes:
                    createBackup = true;
                    _ui.DisplayMessage("Backup requested.");
                    break;
                case DialogResult.No:
                    createBackup = false;
                    _ui.DisplayMessage("Proceeding without backup.");
                    break;
                case DialogResult.Cancel:
                default:
                    _ui.DisplayWarning("Import cancelled by user.");
                    return; // Cancel the import
            }


            SetBusyState(true);
            try
            {
                // Pass the backup flag to the importer
                await Task.Run(() => _importer.Import(_appState.CurrentProjectDirectory!, _appState.CurrentImportFilePath!, createBackup));
            }
            catch (Exception ex)
            {
                _ui.DisplayError($"An unexpected error occurred during the import task: {ex.Message}");
            }
            finally
            {
                SetBusyState(false);
            }
        }

        private void btnClearStatus_Click(object sender, EventArgs e)
        {
            // Check if called from UI thread
            if (rtbStatus.InvokeRequired)
            {
                rtbStatus.Invoke((MethodInvoker)delegate { rtbStatus.Clear(); });
            }
            else
            {
                rtbStatus.Clear();
            }
        }

        // --- Validation Helpers ---
        private bool ValidateProjectDirectory()
        {
            if (string.IsNullOrWhiteSpace(_appState.CurrentProjectDirectory))
            {
                _ui.DisplayError("Project Directory path is missing.");
                return false;
            }
            if (!Directory.Exists(_appState.CurrentProjectDirectory))
            {
                _ui.DisplayError($"Project Directory '{_appState.CurrentProjectDirectory}' not found or inaccessible.");
                return false;
            }
            return true;
        }
        private bool ValidateExportFilePath()
        {
            if (string.IsNullOrWhiteSpace(_appState.CurrentExportFilePath))
            {
                _ui.DisplayError("Export File Path is missing.");
                return false;
            }
            try { Path.GetFullPath(_appState.CurrentExportFilePath); }
            catch (Exception ex) { _ui.DisplayError($"Export File Path is invalid: {ex.Message}"); return false; }
            return true;
        }
        private bool ValidateImportFilePath()
        {
            if (string.IsNullOrWhiteSpace(_appState.CurrentImportFilePath))
            {
                _ui.DisplayError("Import File Path is missing.");
                return false;
            }
            if (!File.Exists(_appState.CurrentImportFilePath))
            {
                _ui.DisplayError($"Import File '{_appState.CurrentImportFilePath}' does not exist.");
                return false;
            }
            try { Path.GetFullPath(_appState.CurrentImportFilePath); }
            catch (Exception ex) { _ui.DisplayError($"Import File Path is invalid: {ex.Message}"); return false; }

            return true;
        }

        private bool ValidateProfileSelected()
        {
            if (_configService.GetActiveProfile() == null || cmbLanguageProfile.SelectedIndex < 0)
            {
                // Attempt to get profile again in case config service failed silently before
                try
                {
                    _configService.GetActiveProfile(); // This will throw if null
                    if (cmbLanguageProfile.SelectedIndex < 0)
                    {
                        _ui.DisplayError("No language profile is selected in the dropdown.");
                        return false;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _ui.DisplayError($"No language profile is active: {ex.Message}");
                    return false;
                }
            }
            return true;
        }
        // --- End Validation ---


        private void SetBusyState(bool busy)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { SetBusyStateInternal(busy); });
            }
            else
            {
                SetBusyStateInternal(busy);
            }
        }

        private void SetBusyStateInternal(bool busy)
        {
            menuStrip1.Enabled = !busy; // Disable menu during operations
            btnPreviewExport.Enabled = !busy;
            btnExport.Enabled = !busy;
            btnCopyToClipboard.Enabled = !busy;
            btnImport.Enabled = !busy;
            btnBrowseProject.Enabled = !busy;
            btnBrowseExport.Enabled = !busy;
            btnBrowseImport.Enabled = !busy;
            txtProjectDir.Enabled = !busy;
            txtExportPath.Enabled = !busy;
            txtImportPath.Enabled = !busy;
            cmbLanguageProfile.Enabled = !busy;
            btnClearStatus.Enabled = !busy;

            progressBar.Visible = busy;
            progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
            progressBar.Value = busy ? 50 : 0;
        }

        // Override OnFormClosing to ensure settings are saved
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Update state from UI controls one last time before saving
            _appState.CurrentProjectDirectory = txtProjectDir.Text;
            _appState.CurrentExportFilePath = txtExportPath.Text;
            _appState.CurrentImportFilePath = txtImportPath.Text;
            // _appState.ActiveLanguageProfileName is updated by combobox handler

            // Saving is now handled by Program.cs using the FormClosing event hookup there
            // SaveApplicationStateToSettings(_appState); // No longer needed here
            base.OnFormClosing(e);
        }

        // --- Menu Item Handlers ---
        private void fileExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void fileReloadProfilesMenuItem_Click(object sender, EventArgs e)
        {
            _ui.ClearOutput();
            SetBusyState(true); // Show busy indicator briefly
            bool reloaded = _configService.ReloadProfiles();
            if (reloaded)
            {
                // Repopulate dropdown and reselect profile
                PopulateLanguageProfiles();
                // SelectInitialProfile(); // Don't call this here, PopulateLanguageProfiles handles selection restoration/fallback
                _ui.DisplaySuccess("Profiles reloaded and dropdown updated.");
            }
            else
            {
                // Error message displayed by ReloadProfiles
                _ui.DisplayError("Profile reload failed.");
            }
            // Errors handled by ReloadProfiles itself
            SetBusyState(false);
        }

        private void helpAboutMenuItem_Click(object sender, EventArgs e)
        {
            // Simple about box
            MessageBox.Show(this,
                "AI Code Share Tool\n\nVersion 1.1 (AI Generated)\n\nDeveloped by Gemini 2.5 Pro.\n\nA tool to export/import project code for AI interaction.",
                "About AI Code Share Tool",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        // --- End Menu Item Handlers ---

    }
}