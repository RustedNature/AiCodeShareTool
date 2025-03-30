

using AiCodeShareTool.UI;

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

            // Create the main form
            var mainForm = new MainForm();

            // Run the application using the main form
            Application.Run(mainForm);
        }
    }
}