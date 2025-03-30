
namespace AiCodeShareTool
{
    /// <summary>
    /// Defines the contract for user interactions.
    /// </summary>
    public interface IUserInterface
    {
        /// <summary>
        /// Displays the main menu and gets the user's choice.
        /// </summary>
        /// <returns>The user's selected menu option character (uppercase).</returns>
        char ShowMainMenu();

        /// <summary>
        /// Prompts the user to select a directory.
        /// </summary>
        /// <param name="description">The description to show the user.</param>
        /// <param name="currentPath">The currently stored path for this type, if any.</param>
        /// <param name="askUseCurrent">Whether to ask the user if they want to reuse the current path.</param>
        /// <returns>The selected directory path, or null if cancelled.</returns>
        string? GetDirectoryPath(string description, string? currentPath, bool askUseCurrent = true);

        /// <summary>
        /// Prompts the user to select a file path for saving.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file filter string (e.g., "Text Files|*.txt").</param>
        /// <param name="defaultExt">The default file extension (e.g., "txt").</param>
        /// <param name="currentPath">The currently stored path for this type, if any.</param>
        /// <param name="askUseCurrent">Whether to ask the user if they want to reus the current path.</param>
        /// <returns>The selected file path, or null if cancelled.</returns>
        string? GetSaveFilePath(string title, string filter, string defaultExt, string? currentPath, bool askUseCurrent = true);

        /// <summary>
        /// Prompts the user to select an existing file path for opening.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file filter string (e.g., "Text Files|*.txt").</param>
        /// <param name="currentPath">The currently stored path for this type, if any.</param>
        /// <param name="askUseCurrent">Whether to ask the user if they want to reuse the current path.</param>
        /// <returns>The selected file path, or null if cancelled.</returns>
        string? GetOpenFilePath(string title, string filter, string? currentPath, bool askUseCurrent = true);

        /// <summary>
        /// Displays a standard informational message to the user.
        /// </summary>
        /// <param name="message">The message to display.</param>
        void DisplayMessage(string message);

        /// <summary>
        /// Displays a warning message to the user.
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        void DisplayWarning(string message);

        /// <summary>
        /// Displays an error message to the user.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        void DisplayError(string message);

        /// <summary>
        /// Displays a success message to the user.
        /// </summary>
        /// <param name="message">The success message to display.</param>
        void DisplaySuccess(string message);


        /// <summary>
        /// Asks the user which path they want to change.
        /// </summary>
        /// <returns>Character representing the choice ('1', '2', '3') or '\0' for cancel.</returns>
        char AskChangePathChoice();

        /// <summary>
        /// Waits for the user to press Enter before continuing.
        /// </summary>
        void WaitForEnter();
    }
}