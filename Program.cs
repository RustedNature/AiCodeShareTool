
using AiCodeShareTool.UI;
using AiCodeShareTool.Configuration; // Need this for ConfigurationService

namespace AiCodeShareTool
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread] // Required for Windows Forms UI elements like dialogs
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // --- Dependency Setup ---
            // Create instances of services and components
            var appState = new ApplicationState(); // Still useful for paths
            var configService = new InMemoryConfigurationService(); // Create the config service
             // Set initial active profile in state if needed, or rely on service default
             appState.ActiveLanguageProfileName = configService.GetActiveProfile().Name;

             // Pass dependencies to the form constructor
            var mainForm = new MainForm(appState, configService);
            // --- End Dependency Setup ---


            // Run the application using the main form
            Application.Run(mainForm);
        }
    }
}