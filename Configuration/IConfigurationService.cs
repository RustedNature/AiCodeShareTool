
using System.Collections.ObjectModel;

namespace AiCodeShareTool.Configuration
{
    /// <summary>
    /// Defines the contract for managing language configuration profiles.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets a read-only collection of all available language profiles.
        /// </summary>
        ReadOnlyCollection<LanguageProfile> GetAvailableProfiles();

        /// <summary>
        /// Gets the currently active language profile.
        /// </summary>
        /// <returns>The active LanguageProfile.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no profile is active.</exception>
        LanguageProfile GetActiveProfile();

        /// <summary>
        /// Sets the active language profile by its name.
        /// </summary>
        /// <param name="profileName">The unique name of the profile to activate.</param>
        /// <returns>True if the profile was found and activated, false otherwise.</returns>
        bool SetActiveProfile(string profileName);

        /// <summary>
        /// Gets the name of the default profile.
        /// </summary>
        string DefaultProfileName { get; }
    }
}