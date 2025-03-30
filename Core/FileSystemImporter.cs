
using System.Text;
using AiCodeShareTool.Configuration;

namespace AiCodeShareTool.Core
{
    /// <summary>
    /// Implements the import functionality using the local file system.
    /// </summary>
    public class FileSystemImporter : IImporter
    {
        private readonly IUserInterface _ui;

        public FileSystemImporter(IUserInterface ui)
        {
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
        }

        public void Import(string projectDirectory, string importFilePath)
        {
            if (!ValidateInputs(projectDirectory, importFilePath)) return;

            _ui.DisplayMessage($"Starting import from '{importFilePath}' into '{projectDirectory}'...");
            int filesImported = 0;
            int filesSkipped = 0;
            string? currentRelativePath = null;
            StringBuilder fileContentBuilder = new StringBuilder();
            bool readingFileContent = false;
            int lineNumber = 0;

            try
            {
                using (StreamReader reader = new StreamReader(importFilePath, Encoding.UTF8))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        string trimmedLine = line.Trim();

                        if (trimmedLine.StartsWith(ExportSettings.StartFileMarkerPrefix) && trimmedLine.EndsWith(ExportSettings.MarkerSuffix))
                        {
                            if (readingFileContent && !string.IsNullOrEmpty(currentRelativePath))
                            {
                                _ui.DisplayWarning($"Line ~{lineNumber}: Found new start marker before end marker for '{currentRelativePath}'. Skipping previous partial content.");
                                filesSkipped++;
                            }

                            currentRelativePath = ExtractPathFromMarker(trimmedLine, ExportSettings.StartFileMarkerPrefix);

                            if (!IsPathValidForImport(currentRelativePath, lineNumber))
                            {
                                currentRelativePath = null;
                                readingFileContent = false;
                                filesSkipped++;
                                continue;
                            }

                            currentRelativePath = currentRelativePath.Replace('/', Path.DirectorySeparatorChar);
                            readingFileContent = true;
                            fileContentBuilder.Clear();
                        }
                        else if (trimmedLine.StartsWith(ExportSettings.EndFileMarkerPrefix) && trimmedLine.EndsWith(ExportSettings.MarkerSuffix) && readingFileContent)
                        {
                            if (currentRelativePath == null)
                            {
                                _ui.DisplayWarning($"Line {lineNumber}: Found end marker without a corresponding valid start marker. Ignoring.");
                                continue;
                            }

                            string endMarkerPathRaw = ExtractPathFromMarker(trimmedLine, ExportSettings.EndFileMarkerPrefix);
                            if (string.IsNullOrWhiteSpace(endMarkerPathRaw))
                            {
                                 _ui.DisplayWarning($"Line {lineNumber}: Found end marker with invalid/empty path. Ignoring.");
                                 continue;
                            }
                            string endMarkerPath = endMarkerPathRaw.Replace('/', Path.DirectorySeparatorChar);

                            if (endMarkerPath.Equals(currentRelativePath, StringComparison.OrdinalIgnoreCase))
                            {
                                if (WriteImportedFile(projectDirectory, currentRelativePath, fileContentBuilder))
                                {
                                    filesImported++;
                                }
                                else
                                {
                                    filesSkipped++; // WriteImportedFile logs specific errors
                                }
                            }
                            else
                            {
                                _ui.DisplayWarning($"Line {lineNumber}: End marker path '{endMarkerPath}' did not match expected '{currentRelativePath}'. Skipping content block.");
                                filesSkipped++;
                            }

                            readingFileContent = false;
                            currentRelativePath = null;
                            fileContentBuilder.Clear();
                        }
                        else if (readingFileContent && currentRelativePath != null)
                        {
                            // Add line to content buffer, preserving original line endings potentially
                            fileContentBuilder.AppendLine(line);
                        }
                        // Ignore lines outside of start/end blocks (like headers or blank lines between files)
                    }
                } // End using StreamReader

                if (readingFileContent && !string.IsNullOrEmpty(currentRelativePath))
                {
                    _ui.DisplayWarning($"Reached end of import file while still reading content for '{currentRelativePath}'. File might be truncated or missing end marker. Skipping final content block.");
                    filesSkipped++;
                }

                _ui.DisplaySuccess($"\nImport finished. {filesImported} file(s) processed, {filesSkipped} file block(s) skipped due to warnings or errors.");

            }
            catch (IOException ex) { _ui.DisplayError($"An I/O error occurred during import file reading: {ex.Message}"); }
            catch (OutOfMemoryException oomEx) { _ui.DisplayError($"Out of memory, the import file might be too large. {oomEx.Message}"); }
            catch (Exception ex) { _ui.DisplayError($"An unexpected error occurred during import parsing: {ex.Message}"); }
        }

        private bool ValidateInputs(string projectDirectory, string importFilePath)
        {
            if (!Directory.Exists(projectDirectory))
            {
                _ui.DisplayError($"Project directory '{projectDirectory}' does not exist.");
                return false;
            }
            if (!File.Exists(importFilePath))
            {
                _ui.DisplayError($"Import file '{importFilePath}' does not exist.");
                return false;
            }
            return true;
        }

        private string? ExtractPathFromMarker(string line, string prefix)
        {
            if (!line.StartsWith(prefix) || !line.EndsWith(ExportSettings.MarkerSuffix)) return null;

            int startIndex = prefix.Length;
            int endIndex = line.Length - ExportSettings.MarkerSuffix.Length;
            if (endIndex <= startIndex) return null; // Empty path

            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private bool IsPathValidForImport(string? relativePath, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                _ui.DisplayWarning($"Line {lineNumber}: Invalid empty path in marker. Skipping block.");
                return false;
            }
            // Basic check for invalid path characters and directory traversal
            if (relativePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || relativePath.Contains(".."))
            {
                _ui.DisplayWarning($"Line {lineNumber}: Invalid or potentially unsafe path detected in marker ('{relativePath}'). Skipping block.");
                return false;
            }
            return true;
        }


        private bool WriteImportedFile(string baseDirectory, string relativePath, StringBuilder contentBuilder)
        {
            string fullPath;
            // Get final content and trim trailing empty lines only
            string content = contentBuilder.ToString().TrimEnd('\r', '\n');

            try
            {
                // Combine first, then get full path for security check
                string combinedPath = Path.Combine(baseDirectory, relativePath);
                fullPath = Path.GetFullPath(combinedPath);

                // **Security Check:** Prevent writing outside the intended project directory
                string fullBasePathCanonical = Path.GetFullPath(baseDirectory + Path.DirectorySeparatorChar); // Ensure trailing slash for comparison
                if (!fullPath.StartsWith(fullBasePathCanonical, StringComparison.OrdinalIgnoreCase))
                {
                    _ui.DisplayWarning($"  Security Warning: Skipping file '{relativePath}'. Target path '{fullPath}' is outside the base project directory '{fullBasePathCanonical}'.");
                    return false;
                }
            }
            catch (ArgumentException argEx) { _ui.DisplayError($"  Error creating path for '{relativePath}'. Skipping. {argEx.Message}"); return false; }
            catch (PathTooLongException ptlEx) { _ui.DisplayError($"  Error: Resulting path for '{relativePath}' is too long. Skipping. {ptlEx.Message}"); return false; }
            catch (Exception pathEx) { _ui.DisplayError($"  Error resolving path for '{relativePath}'. Skipping. {pathEx.Message}"); return false; }

            try
            {
                string? targetDir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
                {
                    _ui.DisplayMessage($"  Creating directory: {targetDir}");
                    Directory.CreateDirectory(targetDir);
                }

                _ui.DisplayMessage($"  Writing file: {relativePath}");
                File.WriteAllText(fullPath, content, Encoding.UTF8); // Assume UTF-8 for writing
                return true; // Success
            }
            catch (UnauthorizedAccessException ex) { _ui.DisplayError($"  Error: Access denied writing file '{relativePath}'. Skipping. {ex.Message}"); }
            catch (IOException ex) { _ui.DisplayError($"  Error: I/O error writing file '{relativePath}'. Skipping. {ex.Message}"); }
            catch (Exception ex) { _ui.DisplayError($"  Error: Unexpected error writing file '{relativePath}'. Skipping. {ex.Message}"); }

            return false; // Failed
        }
    }
}