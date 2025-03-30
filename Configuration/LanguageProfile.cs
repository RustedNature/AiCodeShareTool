
using System.Collections.ObjectModel;

namespace AiCodeShareTool.Configuration
{
    /// <summary>
    /// Represents a configuration profile for a specific language or project type.
    /// </summary>
    public class LanguageProfile
    {
        public string Name { get; }
        public ReadOnlyCollection<string> SearchPatterns { get; }
        public ReadOnlyCollection<string> BlacklistedExtensions { get; }
        public ReadOnlyCollection<string> BlacklistedFileNames { get; }

        // Folders to exclude are currently global in ExportSettings, but could be moved here if needed per-profile
        // public ReadOnlyCollection<string> BlacklistedFolderNames { get; }

        public LanguageProfile(
            string name,
            IEnumerable<string> searchPatterns,
            IEnumerable<string> blacklistedExtensions,
            IEnumerable<string> blacklistedFileNames)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            SearchPatterns = new ReadOnlyCollection<string>((searchPatterns ?? Enumerable.Empty<string>()).ToList());
            BlacklistedExtensions = new ReadOnlyCollection<string>((blacklistedExtensions ?? Enumerable.Empty<string>())
                                                                    .Select(ext => ext.StartsWith('.') ? ext : "." + ext) // Ensure leading dot
                                                                    .ToList());
            BlacklistedFileNames = new ReadOnlyCollection<string>((blacklistedFileNames ?? Enumerable.Empty<string>()).ToList());

            if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Profile name cannot be empty.", nameof(name));
            if (!SearchPatterns.Any()) throw new ArgumentException("Profile must have at least one search pattern.", nameof(searchPatterns));
        }
    }
}