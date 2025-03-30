
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
            _exporter = new FileSystemExporter(_ui, _configService); // Pass config service to exporter
            _importer = new FileSystemImporter(_ui); // Importer doesn't need config service

            // Load initial paths and populate profile dropdown
            LoadInitialPaths();
            PopulateLanguageProfiles();
            SelectInitialProfile();
        }

        private void LoadInitialPaths()
        {
            // In a real app, load from persisted settings
            txtProjectDir.Text = _appState.CurrentProjectDirectory ?? "";
            txtExportPath.Text = _appState.CurrentExportFilePath ?? "";
            txtImportPath.Text = _appState.CurrentImportFilePath ?? "";
        }

         private void PopulateLanguageProfiles()
         {
            var profiles = _configService.GetAvailableProfiles();
            cmbLanguageProfile.Items.Clear();
            cmbLanguageProfile.DisplayMember = nameof(LanguageProfile.Name); // Show the Name property
            foreach (var profile in profiles)
            {
                cmbLanguageProfile.Items.Add(profile);
            }
         }

         private void SelectInitialProfile()
         {
             // Try selecting based on stored state, fallback to service default
            string profileToSelect = _appState.ActiveLanguageProfileName ?? _configService.DefaultProfileName;

            for (int i = 0; i < cmbLanguageProfile.Items.Count; i++)
            {
                if (cmbLanguageProfile.Items[i] is LanguageProfile profile && profile.Name.Equals(profileToSelect, StringComparison.OrdinalIgnoreCase))
                {
                    cmbLanguageProfile.SelectedIndex = i;
                    // Update the active profile in the service to match the UI selection
                    _configService.SetActiveProfile(profile.Name);
                    _ui.DisplayMessage($"Initial language profile set to: {profile.Name}");
                    return;
                }
            }

            // Fallback if stored name not found (shouldn't happen with default logic)
             if (cmbLanguageProfile.Items.Count > 0)
             {
                cmbLanguageProfile.SelectedIndex = 0;
                if(cmbLanguageProfile.SelectedItem is LanguageProfile selectedProfile)
                {
                     _configService.SetActiveProfile(selectedProfile.Name);
                     _ui.DisplayMessage($"Initial language profile defaulted to: {selectedProfile.Name}");
                }
             }
         }

         private void cmbLanguageProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
             if (cmbLanguageProfile.SelectedItem is LanguageProfile selectedProfile)
             {
                bool success = _configService.SetActiveProfile(selectedProfile.Name);
                 _appState.ActiveLanguageProfileName = selectedProfile.Name; // Update state
                if(success)
                {
                    _ui.DisplayMessage($"Active language profile changed to: {selectedProfile.Name}");
                } else {
                    // This shouldn't happen if the item came from the list
                     _ui.DisplayWarning($"Could not activate selected profile: {selectedProfile.Name}");
                }
             }
        }


        private void btnBrowseProject_Click(object sender, EventArgs e)
        {
            string? selectedPath = _ui.GetDirectoryPath("Select Project Directory", _appState.CurrentProjectDirectory);
            if (selectedPath != null)
            {
                _appState.CurrentProjectDirectory = selectedPath;
                txtProjectDir.Text = selectedPath;
                _ui.DisplayMessage($"Project Directory set to: {selectedPath}");
            }
        }

        private void btnBrowseExport_Click(object sender, EventArgs e)
        {
            // Use export path state for initial dir/file
            string? selectedPath = _ui.GetSaveFilePath("Select Export Output File", "Text Files|*.txt|All Files|*.*", "txt", _appState.CurrentExportFilePath);
             if (selectedPath != null)
            {
                _appState.CurrentExportFilePath = selectedPath;
                txtExportPath.Text = selectedPath;
                 _ui.DisplayMessage($"Export File Path set to: {selectedPath}");

                // Convenience: If import path is empty, set it to the same file
                if (string.IsNullOrWhiteSpace(txtImportPath.Text))
                {
                     _appState.CurrentImportFilePath = selectedPath;
                     txtImportPath.Text = selectedPath;
                      _ui.DisplayMessage($"Import File Path also updated for convenience.");
                }
            }
        }

        private void btnBrowseImport_Click(object sender, EventArgs e)
        {
             // Use import path state for initial dir/file
            string? selectedPath = _ui.GetOpenFilePath("Select Import Code File", "Text Files|*.txt|All Files|*.*", _appState.CurrentImportFilePath);
             if (selectedPath != null)
            {
                _appState.CurrentImportFilePath = selectedPath;
                txtImportPath.Text = selectedPath;
                 _ui.DisplayMessage($"Import File Path set to: {selectedPath}");
            }
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            // Ensure state matches UI just before action
            _appState.CurrentProjectDirectory = txtProjectDir.Text;
            _appState.CurrentExportFilePath = txtExportPath.Text;
             // Active profile is already set by combobox handler

            if (string.IsNullOrWhiteSpace(_appState.CurrentProjectDirectory) || string.IsNullOrWhiteSpace(_appState.CurrentExportFilePath))
            {
                _ui.DisplayError("Please select both a Project Directory and an Export File Path before exporting.");
                return;
            }
             if (_configService.GetActiveProfile() == null) // Sanity check
             {
                 _ui.DisplayError("No language profile is selected. Please select one from the dropdown.");
                 return;
             }

            // Disable buttons during operation
            SetBusyState(true);

            // Run export potentially long running task in background
            try
            {
                 // Using Task.Run to avoid blocking the UI thread
                 // Exporter now uses the active profile from the injected config service
                 await Task.Run(() => _exporter.Export(_appState.CurrentProjectDirectory, _appState.CurrentExportFilePath));
            }
            catch(Exception ex)
            {
                 // Catch unexpected errors from the Task wrapper itself if any
                _ui.DisplayError($"An unexpected error occurred during the export task: {ex.Message}");
            }
            finally
            {
                 // Re-enable buttons
                SetBusyState(false);
            }
        }

        private async void btnImport_Click(object sender, EventArgs e)
        {
             // Ensure state matches UI just before action
            _appState.CurrentProjectDirectory = txtProjectDir.Text;
            _appState.CurrentImportFilePath = txtImportPath.Text; // Use import path field

            if (string.IsNullOrWhiteSpace(_appState.CurrentProjectDirectory) || string.IsNullOrWhiteSpace(_appState.CurrentImportFilePath))
            {
                _ui.DisplayError("Please select both a Project Directory and an Import File Path before importing.");
                return;
            }

            // Ask for confirmation before potentially overwriting files
            var confirmResult = MessageBox.Show(
                $"This will import files from:\n{_appState.CurrentImportFilePath}\n\nInto directory:\n{_appState.CurrentProjectDirectory}\n\nExisting files with the same name WILL BE OVERWRITTEN.\n\nAre you sure you want to proceed?",
                "Confirm Import",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.No)
            {
                _ui.DisplayWarning("Import cancelled by user.");
                return;
            }

            // Disable buttons during operation
            SetBusyState(true);

             // Run import potentially long running task in background
            try
            {
                 // Using Task.Run to avoid blocking the UI thread
                 await Task.Run(() => _importer.Import(_appState.CurrentProjectDirectory, _appState.CurrentImportFilePath));
            }
            catch(Exception ex)
            {
                // Catch unexpected errors from the Task wrapper itself if any
                 _ui.DisplayError($"An unexpected error occurred during the import task: {ex.Message}");
            }
            finally
            {
                 // Re-enable buttons
                SetBusyState(false);
            }
        }

        private void SetBusyState(bool busy)
        {
            // Use Invoke if called from a non-UI thread, though here it's from UI events
            this.Invoke((MethodInvoker)delegate {
                btnExport.Enabled = !busy;
                btnImport.Enabled = !busy;
                btnBrowseProject.Enabled = !busy;
                btnBrowseExport.Enabled = !busy;
                btnBrowseImport.Enabled = !busy;
                txtProjectDir.Enabled = !busy;
                txtExportPath.Enabled = !busy;
                txtImportPath.Enabled = !busy;
                 cmbLanguageProfile.Enabled = !busy; // Disable profile selection during operation

                // Optional: Show a progress indicator like a marquee progress bar
                 progressBar.Visible = busy;
                 progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
                 progressBar.Value = busy ? 50 : 0; // Marquee ignores value but set something
            });
        }
    }
}