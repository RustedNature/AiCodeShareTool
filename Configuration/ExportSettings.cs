
namespace AiCodeShareTool.Configuration
{
    /// <summary>
    /// Holds constants related to the export file format.
    /// Specific language settings are now in LanguageProfile.
    /// </summary>
    public static class ExportSettings
    {
        // --- Folders to always exclude, regardless of language ---
        public static readonly string BinFolderName = "bin";
        public static readonly string ObjFolderName = "obj";
        public static readonly string VsFolderName = ".vs";
        public static readonly string GitFolderName = ".git"; // Commonly excluded
        public static readonly string NodeModulesFolderName = "node_modules"; // Very common exclusion


        // --- File Format Markers ---
        public const string StartFileMarkerPrefix = "// === Start File:";
        public const string EndFileMarkerPrefix = "// === End File:";
        public const string ExportRootMarkerPrefix = "// === Export Root:";
        public const string TimestampMarkerPrefix = "// === Timestamp:";
        public const string MarkerSuffix = " ===";
    }
}