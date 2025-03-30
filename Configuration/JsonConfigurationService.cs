
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiCodeShareTool.Configuration
{
    /// <summary>
    /// Manages language profiles stored in a JSON file.
    /// </summary>
    public class JsonConfigurationService : IConfigurationService
    {
        private List<LanguageProfile> _profiles;
        // Made _activeProfile nullable to satisfy compiler warning CS8618
        private LanguageProfile? _activeProfile;
        private readonly string _configFilePath;
        private readonly IUserInterface _ui; // For error reporting

        // Default profile name remains constant for now
        public string DefaultProfileName => ".NET Default";

        // JSON Serializer options
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true, // Make the JSON file readable
            PropertyNameCaseInsensitive = true, // Allow flexibility in JSON property names
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // Don't write null properties
             Converters = { new HashSetStringConverter() } // Custom converter for HashSet<string>
        };

        public JsonConfigurationService(IUserInterface ui, string configFileName = "language_profiles.json")
        {
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));

            // Store config file next to the executable for simplicity
            string appDirectory = AppContext.BaseDirectory;
            _configFilePath = Path.Combine(appDirectory, configFileName);

            _profiles = LoadProfilesFromFile();
            SetInitialActiveProfile();
        }

        private void SetInitialActiveProfile()
        {
             // Try to load last active profile name from settings (handled by MainForm now)
             // For now, just set the default as active initially after loading
            var defaultProfile = _profiles.FirstOrDefault(p => p.Name == DefaultProfileName);

             // If default isn't found (e.g., deleted from JSON), use the first available one
             _activeProfile = defaultProfile ?? _profiles.FirstOrDefault() ?? CreateAndAddDefaultProfile(); // Can still result in null if Create fails

             if (_activeProfile == null)
             {
                 // This should only happen if creation failed and file was empty/corrupt
                 _ui.DisplayError("FATAL: Could not load or create any language profiles. Functionality may be limited.");
                  // Ensure profiles list is not null, even if empty
                  if (_profiles == null) _profiles = new List<LanguageProfile>();
                 // _activeProfile remains null, GetActiveProfile will throw.
             }
        }

        // Can return null if saving the newly created profile fails and _profiles was empty.
        private LanguageProfile? CreateAndAddDefaultProfile()
        {
             _ui.DisplayWarning($"Default profile '{DefaultProfileName}' not found. Creating a new default profile.");
             var defaultProfile = CreateDefaultDotNetProfile();
             if (_profiles == null) _profiles = new List<LanguageProfile>(); // Ensure list exists
             _profiles.Add(defaultProfile);
             // Attempt to save the newly created default profile immediately
             if (!SaveProfilesToFile())
             {
                 _ui.DisplayError("Failed to save the newly created default profile to the configuration file.");
                  // If saving fails, we still technically created it in memory for this session.
             }
             return defaultProfile;
        }


        private List<LanguageProfile> LoadProfilesFromFile()
        {
            if (!File.Exists(_configFilePath))
            {
                _ui.DisplayMessage($"Configuration file '{_configFilePath}' not found. Creating with default profiles.");
                var defaultProfiles = GetDefaultProfiles();
                 // Attempt to save defaults immediately, but proceed even if save fails
                SaveProfilesToFile(defaultProfiles);
                return defaultProfiles;
            }

            try
            {
                string json = File.ReadAllText(_configFilePath);
                 // Handle empty file case explicitly before deserialization
                if (string.IsNullOrWhiteSpace(json))
                {
                     _ui.DisplayWarning($"Configuration file '{_configFilePath}' is empty. Loading default profiles.");
                     return GetDefaultProfiles();
                }

                var loadedProfiles = JsonSerializer.Deserialize<List<LanguageProfileData>>(json, _jsonOptions);

                 if (loadedProfiles == null || !loadedProfiles.Any())
                 {
                     _ui.DisplayWarning($"Configuration file '{_configFilePath}' deserialized to empty or null list. Loading default profiles.");
                     return GetDefaultProfiles(); // Return defaults but don't overwrite file yet
                 }

                // Convert LanguageProfileData to LanguageProfile (handling potential nulls)
                return loadedProfiles
                    .Where(data => data != null) // Filter out potential null entries in the JSON array
                    .Select(data => new LanguageProfile(
                        data.Name ?? $"Unnamed Profile (Line: ~)", // TODO: Improve error reporting for unnamed profiles if possible
                        data.SearchPatterns ?? Enumerable.Empty<string>(),
                        new HashSet<string>(data.BlacklistedExtensions ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase),
                        new HashSet<string>(data.BlacklistedFileNames ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase)
                     ))
                    .ToList();
            }
            catch (JsonException jsonEx)
            {
                _ui.DisplayError($"Error reading or parsing configuration file '{_configFilePath}': {jsonEx.Message}. Loading default profiles.");
                return GetDefaultProfiles();
            }
            catch (IOException ioEx)
            {
                _ui.DisplayError($"Error accessing configuration file '{_configFilePath}': {ioEx.Message}. Loading default profiles.");
                return GetDefaultProfiles();
            }
             catch (Exception ex)
             {
                _ui.DisplayError($"Unexpected error loading configuration: {ex.Message}. Loading default profiles.");
                 return GetDefaultProfiles();
             }
        }

        private bool SaveProfilesToFile(List<LanguageProfile>? profilesToSave = null)
        {
            var profiles = profilesToSave ?? _profiles;
             if (profiles == null || !profiles.Any()) {
                 _ui.DisplayWarning("Save requested, but there are no profiles loaded to save.");
                 return false; // Nothing to save
             }

             // Convert LanguageProfile to LanguageProfileData for serialization
             var dataToSave = profiles.Select(p => new LanguageProfileData
             {
                 Name = p.Name,
                 SearchPatterns = p.SearchPatterns.ToList(), // Convert ReadOnlyCollection to List
                 BlacklistedExtensions = p.BlacklistedExtensionsHashSet.ToList(), // Convert HashSet to List
                 BlacklistedFileNames = p.BlacklistedFileNamesHashSet.ToList() // Convert HashSet to List
             }).ToList();


            try
            {
                string json = JsonSerializer.Serialize(dataToSave, _jsonOptions);
                File.WriteAllText(_configFilePath, json);
                // Avoid success message on automatic saves (like creating defaults)
                // Only show message on explicit user action (if a save button is added later)
                // _ui.DisplayMessage($"Configuration saved to '{_configFilePath}'.");
                return true;
            }
            catch (JsonException jsonEx) { _ui.DisplayError($"Error serializing configuration: {jsonEx.Message}"); return false; }
            catch (IOException ioEx) { _ui.DisplayError($"Error writing configuration file '{_configFilePath}': {ioEx.Message}"); return false; }
            catch (UnauthorizedAccessException uaEx) { _ui.DisplayError($"Permission denied writing configuration file '{_configFilePath}': {uaEx.Message}"); return false; }
            catch (Exception ex) { _ui.DisplayError($"Unexpected error saving configuration: {ex.Message}"); return false; }
        }

        private List<LanguageProfile> GetDefaultProfiles()
        {
            // Return the same defaults as InMemoryConfigurationService initially
            return new List<LanguageProfile>
            {
                CreateDefaultDotNetProfile(),
                CreateDefaultPythonProfile(),
                CreateDefaultGenericTextProfile()
            };
        }

        // --- Default Profile Creation Methods (Extracted for reusability) ---
        private LanguageProfile CreateDefaultDotNetProfile() => new LanguageProfile(
            name: DefaultProfileName,
            searchPatterns: new[] { "*.cs", "*.xaml", "*.csproj", "*.sln", "*.json", "*.xml", "*.config", "*.md", "*.razor", "*.css", "*.js", "*.html", "*.htm", "*.props", "*.targets", "*.ruleset", ".dockerignore", "Dockerfile", ".editorconfig", "*.sh", "*.ps1", "*.cmd", "*.bat" },
            blacklistedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".user", ".suo", ".log", ".tmp", ".pdb", ".bak", ".dll", ".exe", ".nupkg", ".snupkg" },
            blacklistedFileNames: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "launchSettings.json", "package-lock.json", "yarn.lock", ".gitignore" }
        );

        private LanguageProfile CreateDefaultPythonProfile() => new LanguageProfile(
            name: "Python",
            searchPatterns: new[] { "*.py", "*.pyw", "*.ipynb", "*.json", "*.xml", "*.yaml", "*.yml", "*.toml", "*.md", "*.rst", "requirements.txt", "setup.py", "pyproject.toml", ".dockerignore", "Dockerfile", ".editorconfig", "*.sh", "*.bat" },
            blacklistedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".log", ".tmp", ".bak", ".pyc", ".pyd", ".so", ".egg", ".whl", ".coverage" },
            blacklistedFileNames: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "__pycache__", ".env", "pipfile.lock", "poetry.lock", ".gitignore" }
        );

        private LanguageProfile CreateDefaultGenericTextProfile() => new LanguageProfile(
            name: "Generic Text",
            searchPatterns: new[] { "*.txt", "*.md", "*.json", "*.xml", "*.yaml", "*.yml", "*.toml", "*.csv", "*.tsv", "*.sh", "*.ps1", "*.cmd", "*.bat", "*.html", "*.htm", "*.css", "*.js", "*.config", "*.ini", ".editorconfig", "Dockerfile", ".dockerignore" },
            blacklistedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".log", ".tmp", ".bak" },
            blacklistedFileNames: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".gitignore" }
        );
        // --- Interface Implementation ---

        public ReadOnlyCollection<LanguageProfile> GetAvailableProfiles()
        {
             // If _profiles is null (critical load failure), return empty collection
            return (_profiles ?? new List<LanguageProfile>()).AsReadOnly();
        }

        public LanguageProfile GetActiveProfile()
        {
            // Throw if _activeProfile is null, indicating a problem during initialization
            return _activeProfile ?? throw new InvalidOperationException("No active profile set. Configuration might be corrupted or missing.");
        }

        public bool SetActiveProfile(string profileName)
        {
            var profile = _profiles?.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
            if (profile != null)
            {
                _activeProfile = profile;
                return true;
            }
            // Don't warn here, MainForm handles informing the user if selection fails
            // _ui.DisplayWarning($"Profile '{profileName}' not found. Active profile unchanged.");
            return false;
        }

        public bool ReloadProfiles()
        {
            _ui.DisplayMessage("Reloading language profiles from file...");
             var reloadedProfiles = LoadProfilesFromFile();
             // Only update if loading succeeded (returned a non-null list)
             // LoadProfilesFromFile now returns empty list instead of null on failure
             if (reloadedProfiles != null)
             {
                 _profiles = reloadedProfiles;
                 // Re-validate and set active profile
                 string? currentActiveName = _activeProfile?.Name; // Store current name before potentially changing _activeProfile
                 SetInitialActiveProfile(); // This tries to find default or first if current is gone

                 // Warn if the previously active profile is no longer the active one after reload
                 if (!string.IsNullOrEmpty(currentActiveName) && _activeProfile?.Name != currentActiveName)
                 {
                     _ui.DisplayWarning($"Previously active profile '{currentActiveName}' is no longer active (possibly removed or default changed). Switched to '{_activeProfile?.Name ?? "None"}'.");
                 }
                 else if (_activeProfile == null)
                 {
                     _ui.DisplayError("Failed to set any active profile after reloading. Configuration may be severely corrupted.");
                 }

                 _ui.DisplaySuccess("Profiles reloaded successfully.");
                 return true;
             }
             else
             {
                 // This path should ideally not be reached if LoadProfilesFromFile always returns a list
                  _ui.DisplayError("Failed to reload profiles (internal error). Existing profiles remain active.");
                 return false;
             }
        }


        // --- Helper Class for JSON Serialization ---

        /// <summary>
        /// Data Transfer Object (DTO) for LanguageProfile to facilitate JSON serialization,
        /// especially converting HashSet to List for standard JSON arrays.
        /// </summary>
        private class LanguageProfileData
        {
            public string? Name { get; set; }
            public List<string>? SearchPatterns { get; set; }
            public List<string>? BlacklistedExtensions { get; set; }
            public List<string>? BlacklistedFileNames { get; set; }
        }

        /// <summary>
        /// Custom JSON converter to handle HashSet<string> with specific StringComparer.
        /// Deserializes JSON arrays into HashSet<string> using OrdinalIgnoreCase comparer.
        /// Serializes HashSet<string> into JSON arrays.
        /// </summary>
         private class HashSetStringConverter : JsonConverter<HashSet<string>>
         {
             public override HashSet<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
             {
                 if (reader.TokenType != JsonTokenType.StartArray)
                 {
                     throw new JsonException("Expected start of array.");
                 }

                 var hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                 while (reader.Read())
                 {
                     if (reader.TokenType == JsonTokenType.EndArray)
                     {
                         return hashSet;
                     }

                     if (reader.TokenType == JsonTokenType.String)
                     {
                          // Ensure leading dot for extensions during deserialization if desired,
                          // but LanguageProfile constructor already handles this. Keep it simple here.
                         hashSet.Add(reader.GetString()!);
                     }
                     else
                     {
                          // Skip other token types like numbers or objects within the array
                          reader.Skip();
                     }
                 }
                 throw new JsonException("Unexpected end of JSON."); // Should find EndArray
             }

             public override void Write(Utf8JsonWriter writer, HashSet<string> value, JsonSerializerOptions options)
             {
                 writer.WriteStartArray();
                 foreach (var item in value.OrderBy(s => s, StringComparer.OrdinalIgnoreCase)) // Optional: sort for consistent output
                 {
                     writer.WriteStringValue(item);
                 }
                 writer.WriteEndArray();
             }
         }
    }
}