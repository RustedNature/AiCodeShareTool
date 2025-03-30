
using System.Text;
using AiCodeShareTool.Configuration;

namespace AiCodeShareTool.Core
{
    /// <summary>
    /// Implements the import functionality using the local file system.
    /// Import does not depend on language profiles, only on the file format markers.
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
                // Detect encoding (simple BOM check), default to UTF-8
                 Encoding detectedEncoding = DetectEncoding(importFilePath);
                 _ui.DisplayMessage($"Detected import file encoding: {detectedEncoding.EncodingName}");

                using (StreamReader reader = new StreamReader(importFilePath, detectedEncoding))
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

                            // Consume the single blank line typically following the start marker.
                            // Peek to see if the next line exists and is blank.
                            int nextChar = reader.Peek();
                            if (nextChar != -1) // Check if not EOF
                            {
                                // Need to read the line to check if it's blank, then decide if we keep it or not.
                                // This is tricky. Let's assume the exporter *always* puts a blank line after the START marker.
                                // We read and discard this line. If the content starts immediately, this might discard the first line.
                                // A safer approach: don't discard here, let the content builder handle it, and maybe trim later.
                                // Let's stick to the simple approach for now: assume blank line after start marker.
                                string? potentialBlankLine = reader.ReadLine();
                                if (potentialBlankLine != null) // Should not be null if Peek didn't return -1
                                {
                                     lineNumber++;
                                     // If the line wasn't actually blank, add it back? Too complex.
                                     // Assume the convention holds. If not, the first line might be lost.
                                }
                            }
                        }
                        else if (trimmedLine.StartsWith(ExportSettings.EndFileMarkerPrefix) && trimmedLine.EndsWith(ExportSettings.MarkerSuffix) && readingFileContent)
                        {
                             // Before processing the end marker, remove the single blank line *preceding* it,
                             // which the exporter adds.
                             string currentContent = fileContentBuilder.ToString();
                             if (currentContent.EndsWith(Environment.NewLine))
                             {
                                 string contentWithoutLastNewLine = currentContent.Substring(0, currentContent.Length - Environment.NewLine.Length);
                                 // Check if the character before that was *also* a newline char (indicating a blank line)
                                 if (contentWithoutLastNewLine.EndsWith(Environment.NewLine))
                                 {
                                      // It was a blank line, remove it from the builder
                                       fileContentBuilder.Length = contentWithoutLastNewLine.Length;
                                 }
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

                             // Consume the single blank line typically following the END marker.
                             int nextChar = reader.Peek();
                             if (nextChar != -1)
                             {
                                string? potentialBlankLine = reader.ReadLine();
                                if (potentialBlankLine != null) lineNumber++;
                             }
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

        private Encoding DetectEncoding(string filename)
        {
             // Simple BOM detection
            byte[] bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

             if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8; // UTF-8 BOM
             if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; // UTF-16 LE BOM
             if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; // UTF-16 BE BOM
             // Could add UTF-32 checks if needed (bom[0] == 0x00 && bom[1] == 0x00 && bom[2] == 0xfe && bom[3] == 0xff)

             // Fallback or more advanced detection could go here.
             // For now, assume UTF-8 without BOM if no BOM is found.
             return new UTF8Encoding(false); // UTF-8 without BOM
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
             // Get final content. The blank line trimming is now handled during parsing.
             // We still trim trailing whitespace/newlines that might exist at the very end of the content block.
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
                // Ensure UTF-8 encoding without BOM for compatibility, matching the exporter.
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