
using System.Collections.ObjectModel;

namespace AiCodeShareTool.Configuration
{
    /// <summary>
    /// Provides hardcoded language profiles and manages the active one.
    /// </summary>
    public class InMemoryConfigurationService : IConfigurationService
    {
        private readonly List<LanguageProfile> _profiles;
        private LanguageProfile _activeProfile;

        public string DefaultProfileName => ".NET Default";

        public InMemoryConfigurationService()
        {
            _profiles = InitializeProfiles();

            // Set the default profile as active initially
            var defaultProfile = _profiles.FirstOrDefault(p => p.Name == DefaultProfileName);
            _activeProfile = defaultProfile ?? _profiles.First(); // Fallback to first if default not found
        }

        private List<LanguageProfile> InitializeProfiles()
        {
            return new List<LanguageProfile>
            {
                // .NET Default Profile
                new LanguageProfile(
                    name: DefaultProfileName,
                    searchPatterns: new[] {
                        "*.cs", "*.xaml", "*.csproj", "*.sln", "*.json", "*.xml", "*.config", "*.md",
                        "*.razor", "*.css", "*.js", "*.html", "*.htm", "*.props", "*.targets", "*.ruleset",
                        ".dockerignore", "Dockerfile", ".editorconfig", "*.sh", "*.ps1", "*.cmd", "*.bat",
                         "*.gitignore" // Keep .gitignore unless explicitly blacklisted below
                    },
                    blacklistedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                        ".user", ".suo", ".log", ".tmp", ".pdb", ".bak", ".dll", ".exe", ".nupkg", ".snupkg"
                    },
                    blacklistedFileNames: new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                        "launchSettings.json", "package-lock.json", "yarn.lock"
                    }
                ),

                // Python Profile
                 new LanguageProfile(
                    name: "Python",
                    searchPatterns: new[] {
                        "*.py", "*.pyw", "*.ipynb", // Code and notebooks
                        "*.json", "*.xml", "*.yaml", "*.yml", "*.toml", // Config
                        "*.md", "*.rst", // Documentation
                        "requirements.txt", "setup.py", "pyproject.toml", // Project/Dependency files
                        ".dockerignore", "Dockerfile", ".editorconfig", "*.sh", "*.bat", // Scripts/Config
                        ".gitignore"
                    },
                    blacklistedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                       ".log", ".tmp", ".bak", ".pyc", ".pyd", ".so", // Compiled/OS specific
                       ".egg", ".whl", // Packaging
                       ".coverage" // Test coverage
                    },
                    blacklistedFileNames: new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                       "__pycache__", // Directory, but good to list name too
                       ".env", // Often contains secrets
                       "pipfile.lock",
                       "poetry.lock"
                    }
                ),

                 // Generic Text Profile (Wide Scope)
                  new LanguageProfile(
                    name: "Generic Text",
                    searchPatterns: new[] {
                        "*.txt", "*.md", "*.json", "*.xml", "*.yaml", "*.yml", "*.toml",
                        "*.csv", "*.tsv",
                        "*.sh", "*.ps1", "*.cmd", "*.bat",
                        "*.html", "*.htm", "*.css", "*.js",
                        "*.config", "*.ini",
                        ".gitignore", ".editorconfig", "Dockerfile", ".dockerignore"
                     },
                    blacklistedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                       ".log", ".tmp", ".bak"
                    },
                    blacklistedFileNames: new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                       // Fewer specific blacklisted names for generic
                    }
                )

                // Add more profiles here (e.g., Java, JavaScript/TypeScript, etc.)
            };
        }


        public ReadOnlyCollection<LanguageProfile> GetAvailableProfiles()
        {
            return _profiles.AsReadOnly();
        }

        public LanguageProfile GetActiveProfile()
        {
            // In a more complex scenario, you might load this on demand
            return _activeProfile ?? throw new InvalidOperationException("No active profile set.");
        }

        public bool SetActiveProfile(string profileName)
        {
            var profile = _profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));
            if (profile != null)
            {
                _activeProfile = profile;
                return true;
            }
            return false;
        }

        public bool ReloadProfiles()
        {
            throw new NotImplementedException();
        }
    }
}