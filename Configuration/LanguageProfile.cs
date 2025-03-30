
using System.Collections.ObjectModel;
using System.Text.Json.Serialization; // Required for JsonConverter attribute if used directly

namespace AiCodeShareTool.Configuration
{
    /// <summary>
    /// Represents a configuration profile for a specific language or project type.
    /// Now uses HashSet internally for efficient lookups, while exposing ReadOnlyCollection.
    /// </summary>
    public class LanguageProfile
    {
        public string Name { get; }

        // Expose ReadOnlyCollections for external use
        public ReadOnlyCollection<string> SearchPatterns { get; }
        public ReadOnlyCollection<string> BlacklistedExtensions { get; }
        public ReadOnlyCollection<string> BlacklistedFileNames { get; }

        // Internal HashSets for efficient checking
        // Mark with JsonIgnore as we use LanguageProfileData DTO for serialization
        [JsonIgnore]
        internal HashSet<string> BlacklistedExtensionsHashSet { get; }
         [JsonIgnore]
        internal HashSet<string> BlacklistedFileNamesHashSet { get; }
         [JsonIgnore]
        internal HashSet<string> SearchPatternsHashSet { get; } // Less critical, but potentially useful


        // Constructor now takes HashSet parameters for internal storage
        public LanguageProfile(
            string name,
            IEnumerable<string> searchPatterns,
            HashSet<string> blacklistedExtensions, // Accept HashSet
            HashSet<string> blacklistedFileNames) // Accept HashSet
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            // Store original enumerables/hashsets
            SearchPatternsHashSet = new HashSet<string>(searchPatterns ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase); // Store patterns case-insensitively too
            // Ensure leading dot for extensions in the internal HashSet
            BlacklistedExtensionsHashSet = new HashSet<string>(
                (blacklistedExtensions ?? Enumerable.Empty<string>()).Select(ext => ext.StartsWith('.') ? ext : "." + ext),
                StringComparer.OrdinalIgnoreCase);
            BlacklistedFileNamesHashSet = new HashSet<string>(blacklistedFileNames ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);

             // Initialize ReadOnlyCollections from the internal HashSets/Lists for public access
             SearchPatterns = new ReadOnlyCollection<string>(SearchPatternsHashSet.ToList()); // List for order preservation if needed later
            BlacklistedExtensions = new ReadOnlyCollection<string>(BlacklistedExtensionsHashSet.ToList());
            BlacklistedFileNames = new ReadOnlyCollection<string>(BlacklistedFileNamesHashSet.ToList());


            if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Profile name cannot be empty.", nameof(name));
            if (!SearchPatterns.Any()) throw new ArgumentException("Profile must have at least one search pattern.", nameof(searchPatterns));
        }

        // Overload constructor for convenience if starting from IEnumerable, creating HashSets internally
         public LanguageProfile(
             string name,
             IEnumerable<string> searchPatterns,
             IEnumerable<string> blacklistedExtensions,
             IEnumerable<string> blacklistedFileNames)
             : this(name,
                    searchPatterns,
                    new HashSet<string>(blacklistedExtensions ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase),
                    new HashSet<string>(blacklistedFileNames ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase))
         { }

    }
}