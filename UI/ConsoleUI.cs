
using System.Text;

namespace AiCodeShareTool.UI
{
    /// <summary>
    /// Implements the IUserInterface using the console and Windows Forms dialogs.
    /// </summary>
    public class ConsoleUI : IUserInterface
    {
        public ConsoleUI()
        {
            // Ensure console can display paths correctly, especially on Windows
            try { Console.OutputEncoding = Encoding.UTF8; } catch { /* Ignore if fails */ }
        }

        public char ShowMainMenu()
        {
            Console.WriteLine("\nChoose an operation:");
            Console.WriteLine(" E) Export Project");
            Console.WriteLine(" I) Import Code");
            Console.WriteLine(" C) Change Paths");
            Console.WriteLine(" Q) Quit");
            Console.Write("Enter your choice: ");
            string? choice = Console.ReadLine()?.Trim().ToUpper();
            return string.IsNullOrEmpty(choice) ? '\0' : choice[0];
        }

        public char AskChangePathChoice()
        {
            Console.WriteLine("\n-- Change Paths --");
            Console.WriteLine("Select which path to change:");
            Console.WriteLine(" 1) Project Directory");
            Console.WriteLine(" 2) Export File Path");
            Console.WriteLine(" 3) Import File Path");
            Console.WriteLine(" Any other key to cancel");
            Console.Write("Enter your choice: ");

            var key = Console.ReadKey(intercept: true);
            Console.WriteLine(); // Move to next line after key press

            return key.KeyChar switch
            {
                '1' => '1',
                '2' => '2',
                '3' => '3',
                _ => '\0', // Represents cancel or invalid choice
            };
        }

        public string? GetDirectoryPath(string description, string? currentPath, bool askUseCurrent = true)
        {
            if (askUseCurrent && !string.IsNullOrEmpty(currentPath))
            {
                if (AskToUseCurrentPath("directory", currentPath))
                {
                    if (Directory.Exists(currentPath)) return currentPath;
                    DisplayWarning("Current directory no longer exists. Please select a new one.");
                }
                else
                {
                    DisplayMessage("Proceeding to select a new directory...");
                }
            }

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = true;
                SetInitialDialogPath(dialog, currentPath);

                DisplayMessage($"\nPlease select the directory: {description}");
                DialogResult result = dialog.ShowDialog(); // Requires STAThread

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    DisplayMessage($"Selected Path: {dialog.SelectedPath}");
                    return dialog.SelectedPath;
                }
                else
                {
                    DisplayWarning("Operation cancelled or no folder selected.");
                    return null;
                }
            }
        }

        public string? GetSaveFilePath(string title, string filter, string defaultExt, string? currentPath, bool askUseCurrent = true)
        {
            if (askUseCurrent && !string.IsNullOrEmpty(currentPath))
            {
                if (AskToUseCurrentPath("file path", currentPath))
                {
                    // No need to check existence for save dialog
                    return currentPath;
                }
                else
                {
                    DisplayMessage("Proceeding to select a new file path...");
                }
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;
                dialog.DefaultExt = defaultExt;
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                SetInitialDialogPath(dialog, currentPath);

                DisplayMessage($"\nPlease select the file: {title}");
                DialogResult result = dialog.ShowDialog(); // Requires STAThread

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    DisplayMessage($"Selected File: {dialog.FileName}");
                    return dialog.FileName;
                }
                else
                {
                    DisplayWarning("Operation cancelled or no file selected.");
                    return null;
                }
            }
        }

        public string? GetOpenFilePath(string title, string filter, string? currentPath, bool askUseCurrent = true)
        {
            if (askUseCurrent && !string.IsNullOrEmpty(currentPath))
            {
                if (AskToUseCurrentPath("file", currentPath))
                {
                    if (File.Exists(currentPath)) return currentPath;
                    DisplayWarning("Current file no longer exists. Please select a new one.");
                }
                else
                {
                    DisplayMessage("Proceeding to select a new file path...");
                }
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = title;
                dialog.Filter = filter;
                dialog.CheckFileExists = true;
                dialog.Multiselect = false;
                SetInitialDialogPath(dialog, currentPath);

                DisplayMessage($"\nPlease select the file: {title}");
                DialogResult result = dialog.ShowDialog(); // Requires STAThread

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    DisplayMessage($"Selected File: {dialog.FileName}");
                    return dialog.FileName;
                }
                else
                {
                    DisplayWarning("Operation cancelled or no file selected.");
                    return null;
                }
            }
        }


        private bool AskToUseCurrentPath(string pathType, string path)
        {
            Console.Write($"Current {pathType}: {path}. Use this? (Y/N): ");
            var key = Console.ReadKey(intercept: true);
            Console.WriteLine(); // New line after input
            if (key.Key == ConsoleKey.Y)
            {
                DisplayMessage($"Using current {pathType}.");
                return true;
            }
            if (key.Key != ConsoleKey.N) { DisplayWarning("Invalid input. Assuming 'No'."); }
            return false;
        }

        private void SetInitialDialogPath(CommonDialog dialog, string? currentPath)
        {
            if (string.IsNullOrEmpty(currentPath)) return;

            try
            {
                string? initialDir = null;
                string? initialFileName = null;

                if (dialog is FileDialog fileDialog)
                {
                    initialDir = Path.GetDirectoryName(currentPath);
                    initialFileName = Path.GetFileName(currentPath);
                    if (!string.IsNullOrEmpty(initialFileName))
                    {
                        fileDialog.FileName = initialFileName;
                    }
                }
                else if (dialog is FolderBrowserDialog folderDialog)
                {
                    // FolderBrowserDialog expects SelectedPath to be the directory itself
                    if (Directory.Exists(currentPath)) initialDir = currentPath;
                    else initialDir = Path.GetDirectoryName(currentPath); // Fallback to parent if file path was given
                }


                if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                {
                    // Set InitialDirectory for FileDialogs, SelectedPath for FolderBrowserDialog
                    if (dialog is FileDialog fd) fd.InitialDirectory = initialDir;
                    else if (dialog is FolderBrowserDialog fbd) fbd.SelectedPath = initialDir;
                }
                // No else needed, dialog defaults to a standard location
            }
            catch (ArgumentException)
            {
                // Handle cases like invalid chars or UNC paths FBD doesn't like
                DisplayWarning("Warning: Could not set initial path (possibly invalid characters or network path issue). Starting from default location.");
            }
            catch (Exception ex) // Catch broader errors during path manipulation
            {
                DisplayWarning($"Warning: Unexpected error setting initial path: {ex.Message}. Starting from default location.");
            }
        }


        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void DisplayWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: {message}");
            Console.ResetColor();
        }

        public void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {message}");
            Console.ResetColor();
        }

        public void DisplaySuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public void WaitForEnter()
        {
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }

        public void ClearOutput()
        {
            throw new NotImplementedException();
        }
    }
}