
using System.Text;
using AiCodeShareTool.Configuration;

namespace AiCodeShareTool.Core
{
    /// <summary>
    /// Implements the export functionality using the local file system.
    /// Uses the active configuration profile provided by IConfigurationService.
    /// </summary>
    public class FileSystemExporter : IExporter
    {
        private readonly IUserInterface _ui;
        private readonly IConfigurationService _configService;

        public FileSystemExporter(IUserInterface ui, IConfigurationService configService)
        {
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        /// <summary>
        /// Finds files matching the active profile but does not write the export file.
        /// </summary>
        /// <param name="projectDirectory">The root directory of the project.</param>
        /// <returns>A list of relative file paths that would be exported, or null if an error occurs.</returns>
        public List<string>? PreviewExport(string projectDirectory)
        {
             _ui.ClearOutput(); // Clear previous messages
             LanguageProfile activeProfile;
             try { activeProfile = _configService.GetActiveProfile(); }
             catch (InvalidOperationException ex)
             {
                 _ui.DisplayError($"Cannot preview: No language profile is active. {ex.Message}");
                 return null;
             }

             _ui.DisplayMessage($"--- Starting Export Preview using '{activeProfile.Name}' profile ---");

             if (string.IsNullOrWhiteSpace(projectDirectory) || !Directory.Exists(projectDirectory))
             {
                 _ui.DisplayError($"Project directory '{projectDirectory ?? "<null>"}' not found or inaccessible.");
                 return null;
             }

             try
             {
                 var excludedFolders = GetExcludedFolders();
                 _ui.DisplayMessage($"Searching in '{projectDirectory}' for files matching profile patterns...");
                 _ui.DisplayMessage($"Excluding folders: {string.Join(", ", excludedFolders)}");

                 string[] foundFiles = FindAndFilterFiles(projectDirectory, activeProfile, excludedFolders);

                 if (foundFiles.Length == 0)
                 {
                     _ui.DisplayWarning($"No suitable files found matching profile patterns in '{projectDirectory}'.");
                     return new List<string>(); // Return empty list
                 }

                 _ui.DisplaySuccess($"Preview complete: Found {foundFiles.Length} file(s) that would be exported.");

                 // Return relative paths
                 string baseDirFullPath = Path.GetFullPath(projectDirectory);
                 return foundFiles
                     .Select(fullPath => Path.GetRelativePath(baseDirFullPath, fullPath))
                     .OrderBy(relativePath => relativePath)
                     .ToList();
             }
              catch (ArgumentException argEx)
              {
                    // Catch specific errors like different drives during relative path calculation
                    _ui.DisplayError($"Error processing paths during preview (e.g., files on different drives?): {argEx.Message}");
                    return null;
              }
             catch (Exception ex)
             {
                  _ui.DisplayError($"Unexpected error during preview: {ex.Message}");
                  return null;
             }
              finally
             {
                  _ui.DisplayMessage($"--- Preview Finished ---");
             }
        }


        public void Export(string projectDirectory, string exportFilePath)
        {
            _ui.ClearOutput(); // Clear previous messages in UI
            LanguageProfile activeProfile;
            try
            {
                activeProfile = _configService.GetActiveProfile();
            }
            catch (InvalidOperationException ex)
            {
                _ui.DisplayError($"Cannot export: No language profile is active. {ex.Message}");
                return;
            }

            _ui.DisplayMessage($"--- Starting Export using '{activeProfile.Name}' profile ---");

            if (!ValidateInputs(projectDirectory, exportFilePath)) return;

            long totalCharactersExported = 0;

            try
            {
                EnsureExportDirectoryExists(exportFilePath);

                var excludedFolders = GetExcludedFolders();
                LogExportParameters(activeProfile, excludedFolders);
                _ui.DisplayMessage($"\nSearching in '{projectDirectory}'...");

                var codeFiles = FindAndFilterFiles(projectDirectory, activeProfile, excludedFolders);

                if (codeFiles.Length == 0)
                {
                    _ui.DisplayWarning($"No suitable, non-blacklisted files found matching profile patterns in '{projectDirectory}'. Export aborted.");
                    return;
                }

                _ui.DisplayMessage($"Found {codeFiles.Length} file(s) after filtering. Exporting to '{exportFilePath}'...");

                totalCharactersExported = WriteExportFile(projectDirectory, exportFilePath, codeFiles);

                _ui.DisplaySuccess($"\nExport completed successfully to '{exportFilePath}'.");
                 _ui.DisplayMessage($"Total characters exported: {totalCharactersExported:N0}"); // Display character count
            }
            catch (UnauthorizedAccessException ex) { _ui.DisplayError($"Access denied during export. {ex.Message}"); }
            catch (IOException ex) { _ui.DisplayError($"I/O error during export setup/write. {ex.Message}"); }
            catch (Exception ex) { _ui.DisplayError($"Unexpected error during export. {ex.Message}"); }
            finally
            {
                 _ui.DisplayMessage($"--- Export Finished ---");
            }
        }

        // Implementation matches the interface signature (StringBuilder?)
        public StringBuilder? ExportToStringBuilder(string projectDirectory, out long totalCharacters)
        {
            totalCharacters = 0;
            _ui.ClearOutput();
            LanguageProfile activeProfile;
            try { activeProfile = _configService.GetActiveProfile(); }
            catch (InvalidOperationException ex)
            {
                _ui.DisplayError($"Cannot export: No language profile is active. {ex.Message}");
                return null; // Return null on setup error
            }

            _ui.DisplayMessage($"--- Starting Export to String using '{activeProfile.Name}' profile ---");

            if (string.IsNullOrWhiteSpace(projectDirectory) || !Directory.Exists(projectDirectory))
             {
                 _ui.DisplayError($"Project directory '{projectDirectory ?? "<null>"}' not found or inaccessible.");
                 return null; // Return null on setup error
             }

            try
            {
                var excludedFolders = GetExcludedFolders();
                 LogExportParameters(activeProfile, excludedFolders);
                _ui.DisplayMessage($"\nSearching in '{projectDirectory}'...");

                var codeFiles = FindAndFilterFiles(projectDirectory, activeProfile, excludedFolders);

                if (codeFiles.Length == 0)
                {
                    _ui.DisplayWarning($"No suitable, non-blacklisted files found matching profile patterns in '{projectDirectory}'. Export aborted.");
                     // Return empty builder, not null, as the operation technically succeeded but found nothing.
                     return new StringBuilder();
                }

                _ui.DisplayMessage($"Found {codeFiles.Length} file(s) after filtering. Building export string...");

                // BuildExportContent returns non-null StringBuilder but can throw OOM
                var sb = BuildExportContent(projectDirectory, codeFiles, out totalCharacters);

                _ui.DisplaySuccess($"\nExport string built successfully.");
                _ui.DisplayMessage($"Total characters exported: {totalCharacters:N0}");
                return sb; // Return the non-null StringBuilder

            }
             catch (UnauthorizedAccessException ex) { _ui.DisplayError($"Access denied during export string build. {ex.Message}"); return null; }
             catch (IOException ex) { _ui.DisplayError($"I/O error during export string build. {ex.Message}"); return null; } // Less likely here
             catch (OutOfMemoryException oomEx) { _ui.DisplayError($"Out of memory during export string build. {oomEx.Message}"); return null; } // Catch OOM specifically
            catch (Exception ex) { _ui.DisplayError($"Unexpected error during export string build. {ex.Message}"); return null; }
            finally
            {
                 _ui.DisplayMessage($"--- Export to String Finished ---");
            }
        }

         private void LogExportParameters(LanguageProfile activeProfile, string[] excludedFolders)
         {
             _ui.DisplayMessage($"Searching for files matching patterns: {string.Join(", ", activeProfile.SearchPatterns)}");
             _ui.DisplayMessage($"Excluding folders: {string.Join(", ", excludedFolders)}");
             _ui.DisplayMessage($"Excluding extensions: {string.Join(", ", activeProfile.BlacklistedExtensions)}");
             _ui.DisplayMessage($"Excluding filenames: {string.Join(", ", activeProfile.BlacklistedFileNames)}");
         }

        private string[] GetExcludedFolders() => new[] {
            ExportSettings.BinFolderName, ExportSettings.ObjFolderName,
            ExportSettings.VsFolderName, ExportSettings.GitFolderName,
            ExportSettings.NodeModulesFolderName
        }.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();


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

        private string[] FindAndFilterFiles(string projectDirectory, LanguageProfile profile, string[] globallyExcludedFolders)
        {
            List<string> allFiles = new List<string>();
            EnumerationOptions enumOptions = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                 MatchType = MatchType.Simple,
                 AttributesToSkip = FileAttributes.Hidden | FileAttributes.System
            };

            // Use the profile's SearchPatternsHashSet for potentially faster lookups if needed,
            // but Directory.EnumerateFiles handles the patterns directly.
             foreach (string pattern in profile.SearchPatterns) // Use the ReadOnlyCollection for iteration
            {
                try
                {
                     if(pattern.Contains("..")) {
                         _ui.DisplayWarning($"Skipping potentially unsafe search pattern: '{pattern}'");
                         continue;
                     }
                    allFiles.AddRange(Directory.EnumerateFiles(projectDirectory, pattern, enumOptions));
                }
                 catch (ArgumentException argEx) { _ui.DisplayWarning($"Invalid search pattern '{pattern}'. Skipping. Error: {argEx.Message}"); }
                 catch (DirectoryNotFoundException) { /* Ignore specific error if a pattern targets a non-existent dir */ }
                catch (Exception ex) { _ui.DisplayWarning($"Error enumerating files for pattern '{pattern}': {ex.Message}"); }
            }

            string fullProjDirPath = Path.GetFullPath(projectDirectory);
             // Use TrimEndingDirectorySeparator before adding slash to handle root dirs correctly (e.g., C:\)
            string lowerProjDirWithSlash = Path.TrimEndingDirectorySeparator(fullProjDirPath).ToLowerInvariant() + Path.DirectorySeparatorChar;


            // Use the internal HashSets from the profile for efficient filtering
             var blacklistedExtSet = profile.BlacklistedExtensionsHashSet;
             var blacklistedNameSet = profile.BlacklistedFileNamesHashSet;


            return allFiles
                .AsParallel()
                 // Pass globallyExcludedFolders to the lambda
                .Where(f => IsFileValidForExport(f, lowerProjDirWithSlash, globallyExcludedFolders, blacklistedExtSet, blacklistedNameSet))
                .Distinct()
                .OrderBy(f => f)
                .ToArray();
        }

        private bool IsFileValidForExport(string filePath, string lowerProjDirWithSlash, string[] excludedFolderNames, HashSet<string> blacklistedExtSet, HashSet<string> blacklistedNameSet)
        {
            try
            {
                string fullFilePath = Path.GetFullPath(filePath);
                string lowerFullFilePath = fullFilePath.ToLowerInvariant();
                string fileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(filePath) ?? ""; // Ensure non-null, HashSet handles leading dot

                // Must be within project dir check first
                if (!lowerFullFilePath.StartsWith(lowerProjDirWithSlash, StringComparison.OrdinalIgnoreCase))
                {
                     return false; // Not under the project root
                }


                // Check if path contains any excluded folder name as a whole component
                 // Get relative path AFTER ensuring it's under the root
                string relativePath = Path.GetRelativePath(lowerProjDirWithSlash, lowerFullFilePath);

                 // Handle edge case where relative path might be just the filename (if file is in root)
                 // or if GetDirectoryName returns null for a root path component.
                string? dirName = Path.GetDirectoryName(relativePath);
                string[] pathComponents = string.IsNullOrEmpty(dirName)
                                          ? Array.Empty<string>() // No directory components if in root or invalid
                                          : dirName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                 // Check if any path component matches an excluded folder name
                 bool isInExcludedFolder = pathComponents.Any(component =>
                    !string.IsNullOrEmpty(component) && // Ensure component is not empty
                    excludedFolderNames.Contains(component, StringComparer.OrdinalIgnoreCase)
                 );


                if (isInExcludedFolder) return false;

                 // Use the pre-computed HashSets with OrdinalIgnoreCase comparer
                bool isBlacklisted = blacklistedNameSet.Contains(fileName) ||
                                     (!string.IsNullOrEmpty(fileExtension) && blacklistedExtSet.Contains(fileExtension)); // HashSet handles leading dot check internally now

                if (isBlacklisted) return false;

                 return true; // If not excluded or blacklisted, it's valid
            }
            catch (PathTooLongException) {
                _ui.DisplayWarning($"Path too long. Skipping file: '{filePath}'");
                 return false;
            }
            catch (ArgumentException argEx) {
                 // Path.GetRelativePath can throw if paths are on different drives etc.
                 _ui.DisplayWarning($"Could not determine relative path for '{filePath}'. Skipping. Error: {argEx.Message}");
                 return false;
            }
            catch (Exception ex)
            {
                _ui.DisplayWarning($"Could not process path '{filePath}'. Skipping. Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Writes the export file and returns the total number of characters written (content only).
        /// </summary>
        private long WriteExportFile(string projectDirectory, string exportFilePath, string[] codeFiles)
        {
            var utf8EncodingWithoutBom = new UTF8Encoding(false);
            long totalChars;

            using (StreamWriter writer = new StreamWriter(exportFilePath, false, utf8EncodingWithoutBom))
            {
                 // Use shared logic to build content
                 // BuildExportContent now returns non-null SB
                StringBuilder content = BuildExportContent(projectDirectory, codeFiles, out totalChars);
                writer.Write(content.ToString()); // Write the whole built string
            }
            return totalChars;
        }

         /// <summary>
         /// Builds the complete export content into a StringBuilder. Can throw OutOfMemoryException.
         /// </summary>
         /// <param name="projectDirectory">Base project directory.</param>
         /// <param name="codeFiles">Array of full file paths to include.</param>
         /// <param name="totalChars">Output parameter for total content characters.</param>
         /// <returns>A StringBuilder containing the formatted export data (always non-null, but might throw OOM).</returns>
         /// <exception cref="OutOfMemoryException">Thrown if reading a file exhausts available memory.</exception>
         private StringBuilder BuildExportContent(string projectDirectory, string[] codeFiles, out long totalChars)
         {
             totalChars = 0;
             StringBuilder sb = new StringBuilder();

             sb.AppendLine($"{ExportSettings.ExportRootMarkerPrefix} {projectDirectory}{ExportSettings.MarkerSuffix}");
             sb.AppendLine($"{ExportSettings.TimestampMarkerPrefix} {DateTime.Now:yyyy-MM-dd HH:mm:ss}{ExportSettings.MarkerSuffix}");
             sb.AppendLine(); // Blank line after headers

             foreach (string filePath in codeFiles)
             {
                 try
                 {
                     string relativePath = Path.GetRelativePath(projectDirectory, filePath);
                     string markerPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                     sb.AppendLine($"{ExportSettings.StartFileMarkerPrefix} {markerPath}{ExportSettings.MarkerSuffix}");
                     sb.AppendLine(); // Blank line before content

                     string fileContent = File.ReadAllText(filePath, Encoding.UTF8); // This can throw OOM
                     string trimmedContent = fileContent.TrimEnd('\r', '\n');
                     sb.AppendLine(trimmedContent); // AppendLine handles line ending

                     totalChars += trimmedContent.Length; // Count characters of the actual content written

                     sb.AppendLine(); // Blank line before end marker
                     sb.AppendLine($"{ExportSettings.EndFileMarkerPrefix} {markerPath}{ExportSettings.MarkerSuffix}");
                     sb.AppendLine(); // Blank line after end marker
                     _ui.DisplayMessage($"  + Processed: {relativePath}"); // Provide feedback during build too
                 }
                 catch (IOException readEx) { _ui.DisplayWarning($"Could not read file '{filePath}'. Skipping. Error: {readEx.Message}"); }
                 // Allow OOM to propagate up from File.ReadAllText
                 catch (OutOfMemoryException oomEx) { _ui.DisplayError($"Out of memory reading file '{filePath}'. Skipping remaining files. {oomEx.Message}"); throw; }
                 catch (Exception fileEx) { _ui.DisplayWarning($"An unexpected error occurred processing file '{filePath}'. Skipping. Error: {fileEx.Message}"); }
             }
             return sb; // Return the builder, potentially empty, but never null unless OOM was thrown
         }
    }
}