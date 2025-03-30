
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

            try
            {
                EnsureExportDirectoryExists(exportFilePath);

                // Combine global excludes with profile excludes if they were added to profile
                var excludedFolders = new[] {
                    ExportSettings.BinFolderName, ExportSettings.ObjFolderName,
                    ExportSettings.VsFolderName, ExportSettings.GitFolderName,
                    ExportSettings.NodeModulesFolderName
                }.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

                 _ui.DisplayMessage($"Searching for files matching patterns: {string.Join(", ", activeProfile.SearchPatterns)}");
                 _ui.DisplayMessage($"Excluding folders: {string.Join(", ", excludedFolders)}");
                 _ui.DisplayMessage($"Excluding extensions: {string.Join(", ", activeProfile.BlacklistedExtensions)}");
                 _ui.DisplayMessage($"Excluding filenames: {string.Join(", ", activeProfile.BlacklistedFileNames)}");


                _ui.DisplayMessage($"\nSearching in '{projectDirectory}'...");

                var codeFiles = FindAndFilterFiles(projectDirectory, activeProfile, excludedFolders);

                if (codeFiles.Length == 0)
                {
                    _ui.DisplayWarning($"No suitable, non-blacklisted files found matching profile patterns in '{projectDirectory}'. Export aborted.");
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

        private string[] FindAndFilterFiles(string projectDirectory, LanguageProfile profile, string[] globallyExcludedFolders)
        {
            List<string> allFiles = new List<string>();
            EnumerationOptions enumOptions = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                // MatchCasing can be adjusted if needed, default depends on OS
                 MatchType = MatchType.Simple, // Use simple wildcard matching
                 AttributesToSkip = FileAttributes.Hidden | FileAttributes.System // Optionally skip hidden/system files
            };

            foreach (string pattern in profile.SearchPatterns)
            {
                try
                {
                     // Ensure pattern doesn't try to escape the root directory
                     if(pattern.Contains("..")) {
                         _ui.DisplayWarning($"Skipping potentially unsafe search pattern: '{pattern}'");
                         continue;
                     }
                    allFiles.AddRange(Directory.EnumerateFiles(projectDirectory, pattern, enumOptions));
                }
                 catch (ArgumentException argEx) { _ui.DisplayWarning($"Invalid search pattern '{pattern}'. Skipping. Error: {argEx.Message}"); }
                catch (Exception ex) { _ui.DisplayWarning($"Error enumerating files for pattern '{pattern}': {ex.Message}"); }
            }

            string fullProjDirPath = Path.GetFullPath(projectDirectory);
            // Ensure trailing slash for robust StartsWith comparison
            string lowerProjDirWithSlash = Path.TrimEndingDirectorySeparator(fullProjDirPath.ToLowerInvariant()) + Path.DirectorySeparatorChar;

            // Pre-compile excluded folder path fragments for efficiency
            var excludedFolderFragments = globallyExcludedFolders
                 .Select(folder => Path.DirectorySeparatorChar + folder.ToLowerInvariant() + Path.DirectorySeparatorChar)
                 .ToArray();

             // Use HashSets for faster lookups
             var blacklistedExtSet = new HashSet<string>(profile.BlacklistedExtensions, StringComparer.OrdinalIgnoreCase);
             var blacklistedNameSet = new HashSet<string>(profile.BlacklistedFileNames, StringComparer.OrdinalIgnoreCase);


            return allFiles
                .AsParallel() // Process filtering in parallel for potential speedup
                .Where(f => IsFileValidForExport(f, lowerProjDirWithSlash, excludedFolderFragments, blacklistedExtSet, blacklistedNameSet))
                .Distinct()
                .OrderBy(f => f) // Order after filtering and making distinct
                .ToArray();
        }

        private bool IsFileValidForExport(string filePath, string lowerProjDirWithSlash, string[] excludedFolderFragments, HashSet<string> blacklistedExtSet, HashSet<string> blacklistedNameSet)
        {
            try
            {
                string fullFilePath = Path.GetFullPath(filePath); // Resolve symlinks etc.
                string lowerFullFilePath = fullFilePath.ToLowerInvariant();
                string fileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(filePath)?.ToLowerInvariant() ?? ""; // Includes the dot, handle null

                // Check if path contains any excluded folder fragment
                bool isInExcludedFolder = excludedFolderFragments.Any(frag => lowerFullFilePath.Contains(frag, StringComparison.OrdinalIgnoreCase));

                if (isInExcludedFolder) return false;

                bool isBlacklisted = blacklistedNameSet.Contains(fileName) ||
                                     (!string.IsNullOrEmpty(fileExtension) && blacklistedExtSet.Contains(fileExtension));

                if (isBlacklisted) return false;

                // Must be within project dir (using StartsWith check with trailing slash)
                return lowerFullFilePath.StartsWith(lowerProjDirWithSlash, StringComparison.OrdinalIgnoreCase);
            }
            catch (PathTooLongException) {
                _ui.DisplayWarning($"Path too long. Skipping file: '{filePath}'");
                 return false;
            }
            catch (Exception ex) // Catch SecurityException, ArgumentException, etc.
            {
                _ui.DisplayWarning($"Could not process path '{filePath}'. Skipping. Error: {ex.Message}");
                return false;
            }
        }

        private void WriteExportFile(string projectDirectory, string exportFilePath, string[] codeFiles)
        {
            // Use UTF8 without BOM
            var utf8EncodingWithoutBom = new UTF8Encoding(false);

            using (StreamWriter writer = new StreamWriter(exportFilePath, false, utf8EncodingWithoutBom))
            {
                writer.WriteLine($"{ExportSettings.ExportRootMarkerPrefix} {projectDirectory}{ExportSettings.MarkerSuffix}");
                writer.WriteLine($"{ExportSettings.TimestampMarkerPrefix} {DateTime.Now:yyyy-MM-dd HH:mm:ss}{ExportSettings.MarkerSuffix}");
                writer.WriteLine(); // Blank line after headers

                foreach (string filePath in codeFiles) // Already ordered by FindAndFilterFiles
                {
                    try
                    {
                        string relativePath = Path.GetRelativePath(projectDirectory, filePath);
                        // Use forward slashes consistently in markers for cross-platform compatibility
                        string markerPath = relativePath.Replace(Path.DirectorySeparatorChar, '/');

                        writer.WriteLine($"{ExportSettings.StartFileMarkerPrefix} {markerPath}{ExportSettings.MarkerSuffix}");
                        writer.WriteLine(); // Blank line before content

                        // Read file carefully, try detecting encoding if possible, fallback to UTF-8
                        // For simplicity here, stick with reading as UTF-8, which covers many cases.
                        // More robust solution would involve BOM detection or libraries like Ude.NetStandard
                        string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                        writer.WriteLine(fileContent.TrimEnd('\r', '\n')); // Write content, trimming trailing newlines only

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