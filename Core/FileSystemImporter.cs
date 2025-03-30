

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
            _ui.ClearOutput(); // Clear previous messages in UI
            _ui.DisplayMessage($"--- Starting Import ---");

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
                        // Trim only for marker detection, preserve leading/trailing spaces in content lines
                        string trimmedLine = line.Trim();

                        if (trimmedLine.StartsWith(ExportSettings.StartFileMarkerPrefix) && trimmedLine.EndsWith(ExportSettings.MarkerSuffix))
                        {
                            // Handle case where a previous file block wasn't properly closed
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
                                continue; // Skip to next line
                            }

                            // Normalize path for internal comparison and use
                            currentRelativePath = currentRelativePath.Replace('/', Path.DirectorySeparatorChar);
                            readingFileContent = true;
                            fileContentBuilder.Clear();
                            // Skip the blank line often following the start marker
                            string? nextLinePeek = reader.Peek() == '\r' || reader.Peek() == '\n' ? reader.ReadLine() : null;
                             if (nextLinePeek != null && string.IsNullOrWhiteSpace(nextLinePeek))
                             {
                                 lineNumber++; // Consume the blank line
                             } else if (nextLinePeek != null)
                             {
                                 // First line of content might be immediately after marker, handle it
                                fileContentBuilder.AppendLine(nextLinePeek);
                                lineNumber++;
                             }

                        }
                        else if (trimmedLine.StartsWith(ExportSettings.EndFileMarkerPrefix) && trimmedLine.EndsWith(ExportSettings.MarkerSuffix) && readingFileContent)
                        {
                             // Skip potential blank line just BEFORE the end marker
                             string currentContent = fileContentBuilder.ToString();
                             if (currentContent.EndsWith(Environment.NewLine + Environment.NewLine))
                             {
                                 fileContentBuilder.Length -= Environment.NewLine.Length; // Remove one trailing newline pair
                             }
                             else if (currentContent.EndsWith("\n\n")) // Handle Linux/Mac style maybe
                             {
                                  fileContentBuilder.Length -= 1;
                             }
                             else if (currentContent.EndsWith("\r\n\r\n")) // Handle Windows style maybe
                             {
                                  fileContentBuilder.Length -= 2;
                             }


                            if (currentRelativePath == null)
                            {
                                _ui.DisplayWarning($"Line {lineNumber}: Found end marker without a corresponding valid start marker. Ignoring.");
                                continue; // Skip this marker
                            }

                            string endMarkerPathRaw = ExtractPathFromMarker(trimmedLine, ExportSettings.EndFileMarkerPrefix);
                            if (string.IsNullOrWhiteSpace(endMarkerPathRaw))
                            {
                                 _ui.DisplayWarning($"Line {lineNumber}: Found end marker with invalid/empty path. Ignoring.");
                                 continue; // Skip this marker
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

                            // Reset state for the next file block
                            readingFileContent = false;
                            currentRelativePath = null;
                            fileContentBuilder.Clear();
                        }
                        else if (readingFileContent && currentRelativePath != null)
                        {
                            // Add line to content buffer, preserving original line structure
                            fileContentBuilder.AppendLine(line);
                        }
                        // Ignore lines outside of start/end blocks (like headers or blank lines between files)
                    }
                } // End using StreamReader

                // Check if we ended mid-file
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
            finally
            {
                _ui.DisplayMessage($"--- Import Finished ---");
            }
        }

        private bool ValidateInputs(string projectDirectory, string importFilePath)
        {
            if (string.IsNullOrWhiteSpace(projectDirectory))
            {
                 _ui.DisplayError("Project directory path is missing.");
                 return false;
             }
            if (!Directory.Exists(projectDirectory))
            {
                _ui.DisplayError($"Project directory '{projectDirectory}' does not exist.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(importFilePath))
            {
                 _ui.DisplayError("Import file path is missing.");
                 return false;
            }
            if (!File.Exists(importFilePath))
            {
                _ui.DisplayError($"Import file '{importFilePath}' does not exist.");
                return false;
            }
             // Check if import file path is valid (basic check)
            try
            {
                Path.GetFullPath(importFilePath);
            }
            catch (Exception ex)
            {
                 _ui.DisplayError($"Import file path is invalid: {ex.Message}");
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
            // Basic check for invalid path characters and directory traversal attempts
            // Normalize separators for consistent check before checking invalid chars
            string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            if (normalizedPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || normalizedPath.Contains(".." + Path.DirectorySeparatorChar) || normalizedPath.StartsWith(".." + Path.DirectorySeparatorChar) || normalizedPath == "..")
            {
                _ui.DisplayWarning($"Line {lineNumber}: Invalid or potentially unsafe path detected in marker ('{relativePath}'). Skipping block.");
                return false;
            }
             // Ensure it doesn't start with a drive letter or root path, indicating an absolute path slipped through
            if (Path.IsPathRooted(normalizedPath))
            {
                _ui.DisplayWarning($"Line {lineNumber}: Absolute path detected in marker ('{relativePath}'). Only relative paths allowed. Skipping block.");
                return false;
            }

            return true;
        }


        private bool WriteImportedFile(string baseDirectory, string relativePath, StringBuilder contentBuilder)
        {
            string fullPath;
             // Get final content and trim trailing empty lines only from the *entire block*
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
                // Use WriteAllText which handles overwriting existing files.
                // Ensure UTF-8 encoding without BOM for compatibility.
                File.WriteAllText(fullPath, content, new UTF8Encoding(false));
                return true; // Success
            }
            catch (UnauthorizedAccessException ex) { _ui.DisplayError($"  Error: Access denied writing file '{relativePath}'. Skipping. {ex.Message}"); }
            catch (DirectoryNotFoundException ex) { _ui.DisplayError($"  Error: Could not find part of the path for '{relativePath}'. Directory creation might have failed. Skipping. {ex.Message}"); }
            catch (IOException ex) { _ui.DisplayError($"  Error: I/O error writing file '{relativePath}'. Skipping. {ex.Message}"); }
            catch (Exception ex) { _ui.DisplayError($"  Error: Unexpected error writing file '{relativePath}'. Skipping. {ex.Message}"); }

            return false; // Failed
        }
    }
}