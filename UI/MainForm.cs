

using AiCodeShareTool.Core;

namespace AiCodeShareTool.UI
{
    public partial class MainForm : Form
    {
        private readonly ApplicationState _appState;
        private readonly IUserInterface _ui;
        private readonly IExporter _exporter;
        private readonly IImporter _importer;

        public MainForm()
        {
            InitializeComponent();

            // --- Dependency Setup ---
            _appState = new ApplicationState();
            // Pass the form's RichTextBox to the UI implementation
            _ui = new WinFormsUI(this, this.rtbStatus);
            _exporter = new FileSystemExporter(_ui);
            _importer = new FileSystemImporter(_ui);
            // --- End Dependency Setup ---

            // Set initial control states if needed (e.g., load persisted paths)
            LoadInitialPaths();
        }

        private void LoadInitialPaths()
        {
            // In a real app, load from settings/config file
            // For now, just reflect the initial null state
            txtProjectDir.Text = _appState.CurrentProjectDirectory ?? "";
            txtExportPath.Text = _appState.CurrentExportFilePath ?? "";
            txtImportPath.Text = _appState.CurrentImportFilePath ?? "";
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

            if (string.IsNullOrWhiteSpace(_appState.CurrentProjectDirectory) || string.IsNullOrWhiteSpace(_appState.CurrentExportFilePath))
            {
                _ui.DisplayError("Please select both a Project Directory and an Export File Path before exporting.");
                return;
            }

            // Disable buttons during operation
            SetBusyState(true);

            // Run export potentially long running task in background
            try
            {
                 // Using Task.Run to avoid blocking the UI thread
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
                // Optional: Show a progress indicator like a marquee progress bar
                 progressBar.Visible = busy;
                 progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
                 progressBar.Value = busy ? 50 : 0; // Marquee ignores value but set something
            });
        }
    }
}