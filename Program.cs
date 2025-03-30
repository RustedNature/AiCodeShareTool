
using AiCodeShareTool.Core;
using AiCodeShareTool.UI;

namespace AiCodeShareTool
{
    internal class Program
    {
        [STAThread] // Required for Windows Forms dialogs used by ConsoleUI
        static void Main(string[] args)
        {
            // --- Dependency Setup ---
            var appState = new ApplicationState(); // Holds session paths
            IUserInterface ui = new ConsoleUI();
            IExporter exporter = new FileSystemExporter(ui);
            IImporter importer = new FileSystemImporter(ui);
            // --- End Dependency Setup ---

            ui.DisplayMessage("--- AI Code Share Tool ---");

            bool exit = false;
            while (!exit)
            {
                char choice = ui.ShowMainMenu();

                switch (choice)
                {
                    case 'E':
                        PerformExport(ui, exporter, appState);
                        break;

                    case 'I':
                        PerformImport(ui, importer, appState);
                        break;

                    case 'C':
                        ChangePaths(ui, appState);
                        break;

                    case 'Q':
                        ui.DisplayMessage("Exiting tool. Goodbye!");
                        exit = true;
                        break;

                    default:
                        ui.DisplayWarning("Invalid choice. Please try again.");
                        break;
                }

                if (!exit)
                {
                   ui.DisplayMessage("\n------------------------------");
                   // Optional: Add a pause if operations finish too quickly
                   // ui.WaitForEnter();
                }
            }
        }

        private static void PerformExport(IUserInterface ui, IExporter exporter, ApplicationState state)
        {
            ui.DisplayMessage("\n-- Export Operation --");

            string? projDir = ui.GetDirectoryPath("Select the Project Directory to Export From", state.CurrentProjectDirectory);
            if (string.IsNullOrEmpty(projDir)) { ui.DisplayWarning("Export cancelled: No project directory selected."); return; }
            state.CurrentProjectDirectory = projDir; // Update state

            string? exportFile = ui.GetSaveFilePath("Select Export File Path", "Text Files|*.txt|All Files|*.*", "txt", state.CurrentExportFilePath);
            if (string.IsNullOrEmpty(exportFile)) { ui.DisplayWarning("Export cancelled: No export file selected."); return; }
            state.CurrentExportFilePath = exportFile; // Update state

            // Delegate the actual work to the exporter
            exporter.Export(state.CurrentProjectDirectory, state.CurrentExportFilePath);
        }

        private static void PerformImport(IUserInterface ui, IImporter importer, ApplicationState state)
        {
            ui.DisplayMessage("\n-- Import Operation --");

            string? projDir = ui.GetDirectoryPath("Select the Project Directory to Import Into", state.CurrentProjectDirectory);
            if (string.IsNullOrEmpty(projDir)) { ui.DisplayWarning("Import cancelled: No project directory selected."); return; }
            state.CurrentProjectDirectory = projDir; // Update state

            string? importFile = ui.GetOpenFilePath("Select the Code File to Import", "Text Files|*.txt|All Files|*.*", state.CurrentImportFilePath);
            if (string.IsNullOrEmpty(importFile)) { ui.DisplayWarning("Import cancelled: No import file selected."); return; }
            state.CurrentImportFilePath = importFile; // Update state

            // Delegate the actual work to the importer
            importer.Import(state.CurrentProjectDirectory, state.CurrentImportFilePath);
        }

        private static void ChangePaths(IUserInterface ui, ApplicationState state)
        {
           char pathChoice = ui.AskChangePathChoice();

            switch (pathChoice)
            {
                case '1': // Project Directory
                    string? newProjDir = ui.GetDirectoryPath("Select New Project Directory", state.CurrentProjectDirectory, askUseCurrent: false); // Don't ask again immediately
                    if (newProjDir != null) state.CurrentProjectDirectory = newProjDir;
                    else ui.DisplayWarning("Project directory change cancelled.");
                    break;
                case '2': // Export File Path
                    string? newExportFile = ui.GetSaveFilePath("Select New Export File", "Text Files|*.txt|All Files|*.*", "txt", state.CurrentExportFilePath, askUseCurrent: false);
                    if (newExportFile != null) state.CurrentExportFilePath = newExportFile;
                     else ui.DisplayWarning("Export file path change cancelled.");
                    break;
                case '3': // Import File Path
                    string? newImportFile = ui.GetOpenFilePath("Select New Import File", "Text Files|*.txt|All Files|*.*", state.CurrentImportFilePath, askUseCurrent: false);
                    if (newImportFile != null) state.CurrentImportFilePath = newImportFile;
                     else ui.DisplayWarning("Import file path change cancelled.");
                    break;
                default:
                    ui.DisplayMessage("Change paths cancelled.");
                    break;
            }
        }
    }
}