
namespace AiCodeShareTool.Configuration
{
    /// <summary>
    /// Holds configuration settings specific to the export process.
    /// </summary>
    public static class ExportSettings
    {
        // Add lowercase extensions (starting with '.') or specific filenames to exclude
        public static readonly HashSet<string> BlacklistedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".user", // Visual Studio user settings
            ".suo",  // Visual Studio solution user options (older)
            ".log",  // Log files
            ".tmp",  // Temporary files
            ".pdb",  // Debug symbols (usually not needed for code review)
            ".bak",  // Backup files
            // Add other extensions as needed
        };

        public static readonly HashSet<string> BlacklistedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
             "launchSettings.json",
             ".gitignore",
             "package-lock.json", // Often large and generated
             // Add other specific filenames if needed
        };

        public static readonly string[] DefaultSearchPatterns = {
            "*.cs", "*.xaml", "*.csproj", "*.sln", "*.json", "*.xml", "*.config", "*.md", "*.gitignore",
            "*.razor", "*.css", "*.js", "*.html", "*.htm", "*.props", "*.targets"
        };

        public static readonly string BinFolderName = "bin";
        public static readonly string ObjFolderName = "obj";
        public static readonly string VsFolderName = ".vs";

        public const string StartFileMarkerPrefix = "// === Start File:";
        public const string EndFileMarkerPrefix = "// === End File:";
        public const string ExportRootMarkerPrefix = "// === Export Root:";
        public const string TimestampMarkerPrefix = "// === Timestamp:";
        public const string MarkerSuffix = " ===";
    }
}