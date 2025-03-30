

using System.Text;
using AiCodeShareTool.Configuration;

namespace AiCodeShareTool.Core
{
    /// <summary>
    /// Implements the export functionality using the local file system.
    /// </summary>
    public class FileSystemExporter : IExporter
    {
        private readonly IUserInterface _ui;

        public FileSystemExporter(IUserInterface ui)
        {
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        }

        public void Export(string projectDirectory, string exportFilePath)
        {
            _ui.ClearOutput(); // Clear previous messages in UI
            _ui.DisplayMessage($"--- Starting Export ---");

            if (!ValidateInputs(projectDirectory, exportFilePath)) return;

            try
            {
                EnsureExportDirectoryExists(exportFilePath);

                _ui.DisplayMessage($"Searching for files in '{projectDirectory}' (applying blacklist and excluding {ExportSettings.BinFolderName}/{ExportSettings.ObjFolderName}/{ExportSettings.VsFolderName})...");

                var codeFiles = FindAndFilterFiles(projectDirectory);

                if (codeFiles.Length == 0)
                {
                    _ui.DisplayWarning($"No suitable, non-blacklisted files found matching patterns in '{projectDirectory}'. Export aborted.");
                    return;
                }

                _ui.DisplayMessage($"Found {codeFiles.Length} file(s) after filtering. Exporting to '{exportFilePath}'...");

                WriteExportFile(projectDirectory, exportFilePath, codeFiles);

                _ui.DisplaySuccess($"\nExport completed successfully to '{exportFilePath}'.");
            }
            catch (UnauthorizedAccessException ex) { _ui.DisplayError($"Access denied during export. {ex.Message}"); }
            catch (IOException ex) { _ui.DisplayError($"I/O error during export setup/write. {ex.Message}"); }
            catch (Exception ex) { _ui.DisplayError($"Unexpected error during export. {ex.Message}"); }
            finally
            {
                 _ui.DisplayMessage($"--- Export Finished ---");
            }
        }

        private bool ValidateInputs(string projectDirectory, string exportFilePath)
        {
             if (string.IsNullOrWhiteSpace(projectDirectory))
             {
                 _ui.DisplayError("Project directory path is missing.");
                 return false;
             }
            if (!Directory.Exists(projectDirectory))
            {
                _ui.DisplayError($"Project directory '{projectDirectory}' not found or inaccessible.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(exportFilePath))
            {
                _ui.DisplayError("Export file path is missing.");
                return false;
            }
            // Check if export file path is valid (basic check)
            try
            {
                Path.GetFullPath(exportFilePath);
            }
            catch (Exception ex)
            {
                 _ui.DisplayError($"Export file path is invalid: {ex.Message}");
                return false;
            }

            return true;
        }

        private void EnsureExportDirectoryExists(string exportFilePath)
        {
            string? exportDir = Path.GetDirectoryName(exportFilePath);
            if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
            {
                try
                {
                    Directory.CreateDirectory(exportDir);
                    _ui.DisplayMessage($"Creating directory: {exportDir}");
                }
                catch (Exception ex)
                {
                    throw new IOException($"Error creating export directory '{exportDir}': {ex.Message}", ex);
                }
            }
        }

        private string[] FindAndFilterFiles(string projectDirectory)
        {
            List<string> allFiles = new List<string>();
            EnumerationOptions enumOptions = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            foreach (string pattern in ExportSettings.DefaultSearchPatterns)
            {
                try { allFiles.AddRange(Directory.EnumerateFiles(projectDirectory, pattern, enumOptions)); }
                catch (Exception ex) { _ui.DisplayWarning($"Error enumerating files for pattern '{pattern}': {ex.Message}"); }
            }

            string fullProjDirPath = Path.GetFullPath(projectDirectory);
            string lowerProjDir = fullProjDirPath.ToLowerInvariant() + Path.DirectorySeparatorChar; // Ensure trailing slash

            // Pre-compile path fragments for efficiency
            string binPathFragment = Path.DirectorySeparatorChar + ExportSettings.BinFolderName + Path.DirectorySeparatorChar;
            string objPathFragment = Path.DirectorySeparatorChar + ExportSettings.ObjFolderName + Path.DirectorySeparatorChar;
            string vsPathFragment = Path.DirectorySeparatorChar + ExportSettings.VsFolderName + Path.DirectorySeparatorChar;

            return allFiles
                .Where(f => IsFileValidForExport(f, lowerProjDir, binPathFragment, objPathFragment, vsPathFragment))
                .Distinct()
                .ToArray();
        }

        private bool IsFileValidForExport(string filePath, string lowerProjDirWithSlash, string binFrag, string objFrag, string vsFrag)
        {
            try
            {
                string fullFilePath = Path.GetFullPath(filePath); // Resolve symlinks etc.
                string lowerFullFilePath = fullFilePath.ToLowerInvariant();
                string fileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(filePath)?.ToLowerInvariant() ?? ""; // Includes the dot, handle null

                bool isExcludedFolder = lowerFullFilePath.Contains(binFrag, StringComparison.OrdinalIgnoreCase) ||
                                        lowerFullFilePath.Contains(objFrag, StringComparison.OrdinalIgnoreCase) ||
                                        lowerFullFilePath.Contains(vsFrag, StringComparison.OrdinalIgnoreCase);

                bool isBlacklisted = ExportSettings.BlacklistedFileNames.Contains(fileName) ||
                                      (!string.IsNullOrEmpty(fileExtension) && ExportSettings.BlacklistedExtensions.Contains(fileExtension));

                // Must be within project dir, not in excluded folders, and not blacklisted
                // Ensure startsWith check includes the directory separator for exact match
                return lowerFullFilePath.StartsWith(lowerProjDirWithSlash, StringComparison.OrdinalIgnoreCase) && !isExcludedFolder && !isBlacklisted;
            }
            catch (Exception ex)
            {
                _ui.DisplayWarning($"Could not process path '{filePath}'. Skipping. Error: {ex.Message}");
                return false;
            }
        }

        private void WriteExportFile(string projectDirectory, string exportFilePath, string[] codeFiles)
        {
            using (StreamWriter writer = new StreamWriter(exportFilePath, false, Encoding.UTF8))
            {
                writer.WriteLine($"{ExportSettings.ExportRootMarkerPrefix} {projectDirectory}{ExportSettings.MarkerSuffix}");
                writer.WriteLine($"{ExportSettings.TimestampMarkerPrefix} {DateTime.Now:yyyy-MM-dd HH:mm:ss}{ExportSettings.MarkerSuffix}");
                writer.WriteLine();

                foreach (string filePath in codeFiles.OrderBy(f => f))
                {
                    try
                    {
                        string relativePath = Path.GetRelativePath(projectDirectory, filePath);
                        string markerPath = relativePath.Replace(Path.DirectorySeparatorChar, '/'); // Use forward slash for marker

                        writer.WriteLine($"{ExportSettings.StartFileMarkerPrefix} {markerPath}{ExportSettings.MarkerSuffix}");
                        writer.WriteLine();

                        string fileContent = File.ReadAllText(filePath, Encoding.UTF8); // Assume UTF-8
                        writer.WriteLine(fileContent.TrimEnd('\r', '\n')); // Write content, trimming trailing newlines

                        writer.WriteLine(); // Blank line before end marker
                        writer.WriteLine($"{ExportSettings.EndFileMarkerPrefix} {markerPath}{ExportSettings.MarkerSuffix}");
                        writer.WriteLine(); // Blank line after end marker
                        _ui.DisplayMessage($"  + Exported: {relativePath}"); // Provide feedback
                    }
                    catch (IOException readEx) { _ui.DisplayWarning($"Could not read file '{filePath}'. Skipping. Error: {readEx.Message}"); }
                    catch (Exception fileEx) { _ui.DisplayWarning($"An unexpected error occurred processing file '{filePath}'. Skipping. Error: {fileEx.Message}"); }
                }
            }
        }
    }
}