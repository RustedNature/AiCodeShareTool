
using System.Text;

namespace AiCodeShareTool.UI
{
    /// <summary>
    /// Implements the IUserInterface using the console and Windows Forms dialogs.
    /// NOTE: This class is no longer used by the main WinForms application
    /// but is kept for potential future CLI usage or reference.
    /// It does NOT support the new features like language profile selection.
    /// </summary>
    public class ConsoleUI : IUserInterface
    {
        public ConsoleUI()
        {
            // Ensure console can display paths correctly, especially on Windows
            try { Console.OutputEncoding = Encoding.UTF8; } catch { /* Ignore if fails */ }
        }

        // --- Methods no longer directly applicable or need updates ---
        // public char ShowMainMenu() { ... } // Needs update for profile selection
        // public char AskChangePathChoice() { ... }

        public string? GetDirectoryPath(string description, string? currentPath, bool askUseCurrent = true)
        {
            // The core logic using FolderBrowserDialog remains valid if STAThread is ensured
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

            // Ensure STAThread for console apps using WinForms dialogs
            string? selectedPath = null;
            Thread staThread = new Thread(() =>
            {
                try
                {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        dialog.Description = description;
                        dialog.UseDescriptionForTitle = true;
                        dialog.ShowNewFolderButton = true;
                        SetInitialDialogPath(dialog, currentPath); // Helper method needs context or rework

                        Console.WriteLine($"\nPlease select the directory: {description} (Dialog should appear)");
                        DialogResult result = dialog.ShowDialog(); // Requires STAThread

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                        {
                            selectedPath = dialog.SelectedPath;
                            Console.WriteLine($"Selected Path: {selectedPath}"); // Feedback in console
                        }
                        else
                        {
                            Console.WriteLine("Operation cancelled or no folder selected."); // Feedback
                        }
                    }
                }
                catch (Exception ex)
                {
                     Console.WriteLine($"Error showing folder browser: {ex.Message}");
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join(); // Wait for the dialog thread to complete

            if(selectedPath == null) DisplayWarning("Directory selection failed or was cancelled.");

            return selectedPath; // Return result from STA thread
        }

         public string? GetSaveFilePath(string title, string filter, string defaultExt, string? currentPath, bool askUseCurrent = true)
        {
            // Similar STAThread requirement as GetDirectoryPath
            if (askUseCurrent && !string.IsNullOrEmpty(currentPath))
            {
                if (AskToUseCurrentPath("file path", currentPath)) return currentPath;
                else DisplayMessage("Proceeding to select a new file path...");
            }

            string? selectedPath = null;
             Thread staThread = new Thread(() =>
             {
                 try
                 {
                     using (var dialog = new SaveFileDialog())
                     {
                         dialog.Title = title;
                         dialog.Filter = filter;
                         dialog.DefaultExt = defaultExt;
                         dialog.AddExtension = true;
                         dialog.OverwritePrompt = true;
                         SetInitialDialogPath(dialog, currentPath);

                         Console.WriteLine($"\nPlease select the file: {title} (Dialog should appear)");
                         DialogResult result = dialog.ShowDialog(); // Requires STAThread

                         if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
                         {
                             selectedPath = dialog.FileName;
                             Console.WriteLine($"Selected File: {selectedPath}");
                         }
                         else
                         {
                             Console.WriteLine("Operation cancelled or no file selected.");
                         }
                     }
                 }
                  catch (Exception ex)
                 {
                      Console.WriteLine($"Error showing save file dialog: {ex.Message}");
                 }
             });
             staThread.SetApartmentState(ApartmentState.STA);
             staThread.Start();
             staThread.Join();

             if(selectedPath == null) DisplayWarning("Save file selection failed or was cancelled.");
             return selectedPath;
        }

        public string? GetOpenFilePath(string title, string filter, string? currentPath, bool askUseCurrent = true)
        {
             // Similar STAThread requirement as GetDirectoryPath
             if (askUseCurrent && !string.IsNullOrEmpty(currentPath))
            {
                if (AskToUseCurrentPath("file", currentPath))
                {
                     if (File.Exists(currentPath)) return currentPath;
                     DisplayWarning("Current file no longer exists. Please select a new one.");
                }
                 else DisplayMessage("Proceeding to select a new file path...");
            }

             string? selectedPath = null;
             Thread staThread = new Thread(() =>
             {
                 try
                 {
                     using (var dialog = new OpenFileDialog())
                     {
                         dialog.Title = title;
                         dialog.Filter = filter;
                         dialog.CheckFileExists = true;
                         dialog.Multiselect = false;
                         SetInitialDialogPath(dialog, currentPath);

                         Console.WriteLine($"\nPlease select the file: {title} (Dialog should appear)");
                         DialogResult result = dialog.ShowDialog(); // Requires STAThread

                         if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.FileName))
                         {
                             selectedPath = dialog.FileName;
                             Console.WriteLine($"Selected File: {selectedPath}");
                         }
                         else
                         {
                              Console.WriteLine("Operation cancelled or no file selected.");
                         }
                     }
                 }
                  catch (Exception ex)
                 {
                      Console.WriteLine($"Error showing open file dialog: {ex.Message}");
                 }
             });
             staThread.SetApartmentState(ApartmentState.STA);
             staThread.Start();
             staThread.Join();

             if(selectedPath == null) DisplayWarning("Open file selection failed or was cancelled.");
             return selectedPath;
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

        // This helper needs careful handling in STA thread context or rework
        private void SetInitialDialogPath(CommonDialog dialog, string? currentPath)
        {
            if (string.IsNullOrEmpty(currentPath)) return;
             try
            {
                 // Basic logic, might need refinement for STA context if it accesses UI elements indirectly
                string? initialDir = null;
                string? initialFileName = null;

                 if (Directory.Exists(currentPath)) initialDir = currentPath;
                 else {
                     initialDir = Path.GetDirectoryName(currentPath);
                     initialFileName = Path.GetFileName(currentPath);
                 }

                if (dialog is FileDialog fileDialog)
                {
                    if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir)) fileDialog.InitialDirectory = initialDir;
                    if (!string.IsNullOrEmpty(initialFileName)) fileDialog.FileName = initialFileName;
                }
                else if (dialog is FolderBrowserDialog folderDialog)
                {
                     if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir)) folderDialog.SelectedPath = initialDir;
                }
            }
             catch (Exception ex) // Catch broad errors
            {
                 // Log to console instead of UI warning
                 Console.WriteLine($"Warning: Could not set initial path for dialog: {ex.Message}");
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

         public void ClearOutput()
        {
             // Best effort for console
             try { Console.Clear(); } catch (IOException) { /* May fail if console redirected */ }
        }
    }
}