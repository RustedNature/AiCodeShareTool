
using System.IO.Compression; // Required for ZipFile
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

        /// <summary>
        /// Imports files, optionally creating a backup first.
        /// </summary>
        public void Import(string projectDirectory, string importFilePath, bool createBackup)
        {
            _ui.ClearOutput(); // Clear previous messages in UI
            _ui.DisplayMessage($"--- Starting Import ---");

            if (!ValidateInputs(projectDirectory, importFilePath)) return;

             // --- Backup Step ---
            if (createBackup)
            {
                if (!CreateBackup(projectDirectory))
                {
                     // CreateBackup displays errors, but confirm with user if they want to proceed
                     // (Using a blocking MessageBox here, ideally IUserInterface would handle confirmation flows)
                     var result = MessageBox.Show("Backup failed. Do you want to continue with the import without a backup?",
                                                  "Backup Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                     if (result == DialogResult.No)
                     {
                         _ui.DisplayWarning("Import cancelled by user due to backup failure.");
                         _ui.DisplayMessage($"--- Import Cancelled ---");
                         return;
                     }
                     _ui.DisplayWarning("Proceeding with import without backup.");
                }
            }
             // --- End Backup Step ---


            _ui.DisplayMessage($"Starting import from '{importFilePath}' into '{projectDirectory}'...");
            int filesImported = 0;
            int filesSkipped = 0;
            string? currentRelativePath = null;
            StringBuilder fileContentBuilder = new StringBuilder();
            bool readingFileContent = false;
            int lineNumber = 0;

            try
            {
                 Encoding detectedEncoding = DetectEncoding(importFilePath);
                 _ui.DisplayMessage($"Detected import file encoding: {detectedEncoding.EncodingName}");

                using (StreamReader reader = new StreamReader(importFilePath, detectedEncoding))
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

                            int nextChar = reader.Peek();
                            if (nextChar != -1)
                            {
                                string? potentialBlankLine = reader.ReadLine();
                                if (potentialBlankLine != null) lineNumber++;
                            }
                        }
                        else if (trimmedLine.StartsWith(ExportSettings.EndFileMarkerPrefix) && trimmedLine.EndsWith(ExportSettings.MarkerSuffix) && readingFileContent)
                        {
                             string currentContent = fileContentBuilder.ToString();
                             if (currentContent.EndsWith(Environment.NewLine))
                             {
                                 string contentWithoutLastNewLine = currentContent.Substring(0, currentContent.Length - Environment.NewLine.Length);
                                 if (contentWithoutLastNewLine.EndsWith(Environment.NewLine))
                                 {
                                       fileContentBuilder.Length = contentWithoutLastNewLine.Length;
                                 }
                             }

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
                                    filesSkipped++;
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

                             int nextChar = reader.Peek();
                             if (nextChar != -1)
                             {
                                string? potentialBlankLine = reader.ReadLine();
                                if (potentialBlankLine != null) lineNumber++;
                             }
                        }
                        else if (readingFileContent && currentRelativePath != null)
                        {
                            fileContentBuilder.AppendLine(line);
                        }
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
            finally
            {
                _ui.DisplayMessage($"--- Import Finished ---");
            }
        }

        // Overload for backward compatibility or simpler calls
         public void Import(string projectDirectory, string importFilePath)
         {
             Import(projectDirectory, importFilePath, false); // Default to no backup
         }

         private bool CreateBackup(string projectDirectory)
         {
             string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
             string backupFileName = $"ProjectBackup_{Path.GetFileName(projectDirectory)}_{timestamp}.zip";
             // Place backup in the parent directory of the project directory for less clutter
             string? parentDir = Path.GetDirectoryName(projectDirectory.TrimEnd(Path.DirectorySeparatorChar));
             if(string.IsNullOrEmpty(parentDir) || !Directory.Exists(parentDir))
             {
                 // Fallback to placing it inside the project dir if parent is inaccessible/root
                 parentDir = projectDirectory;
             }
             string backupFilePath = Path.Combine(parentDir, backupFileName);

             _ui.DisplayMessage($"Attempting to create backup of '{projectDirectory}' to '{backupFilePath}'...");

             try
             {
                 // Ensure System.IO.Compression is available
                 ZipFile.CreateFromDirectory(projectDirectory, backupFilePath, CompressionLevel.Optimal, includeBaseDirectory: true);
                 _ui.DisplaySuccess($"Backup created successfully: {backupFilePath}");
                 return true;
             }
             catch (UnauthorizedAccessException uaEx) { _ui.DisplayError($"Backup failed: Permission denied accessing '{projectDirectory}' or writing to '{backupFilePath}'. {uaEx.Message}"); }
             catch (DirectoryNotFoundException dnfEx) { _ui.DisplayError($"Backup failed: Directory not found. {dnfEx.Message}"); }
             catch (IOException ioEx) { _ui.DisplayError($"Backup failed: I/O error (e.g., disk full, file locked). {ioEx.Message}"); }
             catch (Exception ex) { _ui.DisplayError($"Backup failed: An unexpected error occurred. {ex.Message}"); }

             return false;
         }


        private Encoding DetectEncoding(string filename)
        {
            byte[] bom = new byte[4];
            try
            {
                using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    file.Read(bom, 0, 4);
                }
            }
            catch(IOException ioEx)
            {
                _ui.DisplayWarning($"Could not read start of import file to detect encoding: {ioEx.Message}. Defaulting to UTF-8.");
                return new UTF8Encoding(false);
            }


             if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
             if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode;
             if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode;

             return new UTF8Encoding(false);
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
                 // Offer to create the directory? For import, it might be safer to require it exists.
                 _ui.DisplayError($"Project (target) directory '{projectDirectory}' does not exist. Please create it first.");
                 // Alternatively:
                 // _ui.DisplayWarning($"Project directory '{projectDirectory}' does not exist. It will be created.");
                 // Directory.CreateDirectory(projectDirectory); // If deciding to auto-create
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
            if (endIndex <= startIndex) return null;

            return line.Substring(startIndex, endIndex - startIndex).Trim();
        }

        private bool IsPathValidForImport(string? relativePath, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                _ui.DisplayWarning($"Line {lineNumber}: Invalid empty path in marker. Skipping block.");
                return false;
            }
            string normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            if (normalizedPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || normalizedPath.Contains(".." + Path.DirectorySeparatorChar) || normalizedPath.StartsWith(".." + Path.DirectorySeparatorChar) || normalizedPath == "..")
            {
                _ui.DisplayWarning($"Line {lineNumber}: Invalid or potentially unsafe path detected in marker ('{relativePath}'). Skipping block.");
                return false;
            }
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
            string content = contentBuilder.ToString().TrimEnd('\r', '\n');

            try
            {
                string combinedPath = Path.Combine(baseDirectory, relativePath);
                fullPath = Path.GetFullPath(combinedPath);

                string fullBasePathCanonical = Path.GetFullPath(baseDirectory);
                // Robust check: ensure the resulting full path starts with the base path + directory separator
                // This handles cases like base="C:\foo", relative="bar", full="C:\foo\bar" (good)
                // And base="C:\foo", relative="..\bar", full="C:\bar" (bad)
                // And base="C:\foo", relative="C:\abs\path" (bad)
                 if (!fullPath.StartsWith(fullBasePathCanonical + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && fullPath != fullBasePathCanonical) // Allow writing to base itself if needed? Unlikely.
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
                File.WriteAllText(fullPath, content, new UTF8Encoding(false));
                return true;
            }
            catch (UnauthorizedAccessException ex) { _ui.DisplayError($"  Error: Access denied writing file '{relativePath}'. Skipping. {ex.Message}"); }
            catch (DirectoryNotFoundException ex) { _ui.DisplayError($"  Error: Could not find part of the path for '{relativePath}'. Directory creation might have failed. Skipping. {ex.Message}"); }
            catch (IOException ex) { _ui.DisplayError($"  Error: I/O error writing file '{relativePath}'. Skipping. {ex.Message}"); }
            catch (Exception ex) { _ui.DisplayError($"  Error: Unexpected error writing file '{relativePath}'. Skipping. {ex.Message}"); }

            return false;
        }
    }
}