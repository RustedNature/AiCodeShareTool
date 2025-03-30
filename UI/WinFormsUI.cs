

namespace AiCodeShareTool.UI
{
    /// <summary>
    /// Implements IUserInterface using Windows Forms controls and dialogs.
    /// Assumes it's running on the UI thread or handles marshalling.
    /// </summary>
    public class WinFormsUI : IUserInterface
    {
        private readonly Form _owner; // Owner form for dialogs
        private readonly RichTextBox _statusOutput; // Target control for messages

        public WinFormsUI(Form owner, RichTextBox statusOutput)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _statusOutput = statusOutput ?? throw new ArgumentNullException(nameof(statusOutput));
             _statusOutput.DetectUrls = true; // Make file paths clickable
        }

        public string? GetDirectoryPath(string description, string? currentPath, bool askUseCurrent = true)
        {
            // askUseCurrent is ignored, we always show the dialog for GUI
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.UseDescriptionForTitle = true; // More prominent title in some OS versions
                dialog.ShowNewFolderButton = true;
                SetInitialDialogPath(dialog, currentPath);

                DialogResult result = dialog.ShowDialog(_owner); // Show modal to owner

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }
                return null; // Cancelled or empty path
            }
        }

        public string? GetSaveFilePath(string title, string filter, string defaultExt, string? currentPath, bool askUseCurrent = true)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;
                dialog.DefaultExt = defaultExt;
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true; // Warn if overwriting existing file
                SetInitialDialogPath(dialog, currentPath);

                DialogResult result = dialog.ShowDialog(_owner);

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    return dialog.FileName;
                }
                return null;
            }
        }

        public string? GetOpenFilePath(string title, string filter, string? currentPath, bool askUseCurrent = true)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;
                dialog.CheckFileExists = true; // Ensure the selected file exists
                dialog.Multiselect = false;
                SetInitialDialogPath(dialog, currentPath);

                DialogResult result = dialog.ShowDialog(_owner);

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    return dialog.FileName;
                }
                return null;
            }
        }

        private void SetInitialDialogPath(CommonDialog dialog, string? currentPath)
        {
            if (string.IsNullOrEmpty(currentPath)) return;

            try
            {
                string? initialDir = null;
                string? initialFileName = null;

                 // Check if currentPath is a directory or a file path
                if (Directory.Exists(currentPath)) // It's a directory path
                {
                    initialDir = currentPath;
                }
                else if (File.Exists(currentPath)) // It's a file path
                {
                    initialDir = Path.GetDirectoryName(currentPath);
                    initialFileName = Path.GetFileName(currentPath);
                }
                else // Path doesn't exist, try getting directory anyway
                {
                     initialDir = Path.GetDirectoryName(currentPath);
                     // If GetDirectoryName returns null (e.g., just "file.txt"), don't use it
                     if(initialDir == null) initialFileName = currentPath; // Treat as filename only
                }


                if (dialog is FileDialog fileDialog)
                {
                     if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                     {
                         fileDialog.InitialDirectory = initialDir;
                     }
                     if (!string.IsNullOrEmpty(initialFileName))
                     {
                         fileDialog.FileName = initialFileName;
                     }
                }
                else if (dialog is FolderBrowserDialog folderDialog)
                {
                    // FolderBrowserDialog only uses SelectedPath if it's an existing directory
                     if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                     {
                        folderDialog.SelectedPath = initialDir;
                     }
                     // It doesn't have an InitialDirectory property like FileDialog
                     // Setting SelectedPath to a non-existent dir usually doesn't work well.
                }
            }
            catch (ArgumentException)
            {
                 // Handle invalid path characters etc. Silently fail, dialog uses default.
            }
            catch (Exception) // Catch broader errors during path manipulation
            {
                // Silently fail, dialog uses default.
                 // Avoid showing message boxes from here, let the dialog open normally.
            }
        }

        // --- Message Display Methods ---

        public void ClearOutput()
        {
             SafeAction(() => _statusOutput.Clear());
        }

        public void DisplayMessage(string message)
        {
            AppendStatusText(message + Environment.NewLine, Color.Black);
        }

        public void DisplayWarning(string message)
        {
            AppendStatusText($"Warning: {message}{Environment.NewLine}", Color.DarkOrange);
        }

        public void DisplayError(string message)
        {
            AppendStatusText($"Error: {message}{Environment.NewLine}", Color.Red);
        }

        public void DisplaySuccess(string message)
        {
            AppendStatusText($"{message}{Environment.NewLine}", Color.Green);
        }

        // Helper to safely append text to the RichTextBox from any thread
        private void AppendStatusText(string text, Color color)
        {
            SafeAction(() =>
            {
                _statusOutput.SelectionStart = _statusOutput.TextLength;
                _statusOutput.SelectionLength = 0;
                _statusOutput.SelectionColor = color;
                _statusOutput.AppendText(text);
                _statusOutput.SelectionColor = _statusOutput.ForeColor; // Reset color
                _statusOutput.ScrollToCaret(); // Keep latest messages visible
            });
        }

        // Helper to execute an action on the UI thread if needed
        private void SafeAction(Action action)
        {
            if (_statusOutput.InvokeRequired)
            {
                _statusOutput.Invoke(action);
            }
            else
            {
                action();
            }
        }

        // --- Methods from ConsoleUI not directly applicable ---
        // public char ShowMainMenu() => throw new NotSupportedException("Main menu is handled by the form's layout.");
        // public char AskChangePathChoice() => throw new NotSupportedException("Path changes are handled by browse buttons.");
        // public void WaitForEnter() { /* No-op in GUI */ }
    }
}