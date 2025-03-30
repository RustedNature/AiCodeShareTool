
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
             // Temporary UI for config service loading errors before main form UI is ready
            var consoleUiForConfig = new ConsoleUI(); // Or a simple MessageBox based UI

            // Use JsonConfigurationService now
             var configService = new JsonConfigurationService(consoleUiForConfig);

             // Load last used settings - Changed LoadApplicationStateFromSettings to return ApplicationState
             ApplicationState appState = LoadApplicationStateFromSettings(configService);


             // Pass dependencies to the form constructor
            var mainForm = new MainForm(appState, configService);

             // Ensure main form saves state on close
             // Capture the 'appState' variable (now a local var, not out param)
             mainForm.FormClosing += (sender, e) => SaveApplicationStateToSettings(appState);

            // --- End Dependency Setup ---


            // Run the application using the main form
            Application.Run(mainForm);
        }

         // Changed signature: returns ApplicationState instead of using 'out'
         private static ApplicationState LoadApplicationStateFromSettings(IConfigurationService configService)
         {
             var loadedState = new ApplicationState(); // Create instance here
             try
             {
                 loadedState.CurrentProjectDirectory = Properties.Settings.Default.LastProjectDirectory;
                 loadedState.CurrentExportFilePath = Properties.Settings.Default.LastExportFilePath;
                 loadedState.CurrentImportFilePath = Properties.Settings.Default.LastImportFilePath;
                 loadedState.ActiveLanguageProfileName = Properties.Settings.Default.LastProfileName;

                 // Validate the loaded profile name exists in the service
                 if (!string.IsNullOrEmpty(loadedState.ActiveLanguageProfileName))
                 {
                     bool profileExists = configService.GetAvailableProfiles()
                         .Any(p => p.Name.Equals(loadedState.ActiveLanguageProfileName, StringComparison.OrdinalIgnoreCase));

                     if (!profileExists)
                     {
                         Console.WriteLine($"Warning: Last used profile '{loadedState.ActiveLanguageProfileName}' not found in current configuration. Falling back to default.");
                         loadedState.ActiveLanguageProfileName = configService.DefaultProfileName;
                         // Save the fallback immediately? Or wait until app close? Let's wait.
                     }
                 }
                 else
                 {
                      // If no last profile was saved, use the default
                      loadedState.ActiveLanguageProfileName = configService.DefaultProfileName;
                 }
             }
             catch (Exception ex)
             {
                  // Log error if settings fail to load
                  Console.WriteLine($"Error loading application settings: {ex.Message}");
                  // loadedState will have default null/empty values
                  loadedState.ActiveLanguageProfileName = configService.DefaultProfileName; // Ensure default profile is set
             }
              return loadedState; // Return the created/populated state object
         }

         private static void SaveApplicationStateToSettings(ApplicationState appState)
         {
              try
             {
                 Properties.Settings.Default.LastProjectDirectory = appState.CurrentProjectDirectory ?? "";
                 Properties.Settings.Default.LastExportFilePath = appState.CurrentExportFilePath ?? "";
                 Properties.Settings.Default.LastImportFilePath = appState.CurrentImportFilePath ?? "";
                 Properties.Settings.Default.LastProfileName = appState.ActiveLanguageProfileName ?? ""; // Save currently active profile name

                 Properties.Settings.Default.Save();
             }
             catch (Exception ex)
             {
                  // Log error if settings fail to save
                 Console.WriteLine($"Error saving application settings: {ex.Message}");
                 // Optionally show a message to the user in a real app
             }
         }
    }
}