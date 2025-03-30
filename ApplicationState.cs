
namespace AiCodeShareTool
{
    /// <summary>
    /// Holds the mutable state of the application, like last used paths.
    /// </summary>
    public class ApplicationState
    {
        public string? CurrentProjectDirectory { get; set; } = null;
        public string? CurrentExportFilePath { get; set; } = null;
        public string? CurrentImportFilePath { get; set; } = null;
        public string? ActiveLanguageProfileName { get; set; } // Store the name of the active profile
    }
}