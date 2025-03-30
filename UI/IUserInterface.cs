
namespace AiCodeShareTool
{
    /// <summary>
    /// Defines the contract for user interactions.
    /// This interface is now slightly adapted for a GUI context.
    /// </summary>
    public interface IUserInterface
    {
        // --- Methods no longer applicable in GUI context ---
        // char ShowMainMenu(); // Replaced by direct button interaction
        // char AskChangePathChoice(); // Replaced by browse buttons
        // void WaitForEnter(); // Not needed in event-driven GUI

        /// <summary>
        /// Prompts the user to select a directory.
        /// </summary>
        /// <param name="description">The description to show the user.</param>
        /// <param name="currentPath">The currently stored path for this type, if any, used as initial directory.</param>
        /// <param name="askUseCurrent">This parameter is largely ignored in GUI, dialog always shown.</param>
        /// <returns>The selected directory path, or null if cancelled.</returns>
        string? GetDirectoryPath(string description, string? currentPath, bool askUseCurrent = true);

        /// <summary>
        /// Prompts the user to select a file path for saving.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file filter string (e.g., "Text Files|*.txt").</param>
        /// <param name="defaultExt">The default file extension (e.g., "txt").</param>
        /// <param name="currentPath">The currently stored path for this type, if any, used for initial dir/filename.</param>
        /// <param name="askUseCurrent">This parameter is largely ignored in GUI, dialog always shown.</param>
        /// <returns>The selected file path, or null if cancelled.</returns>
        string? GetSaveFilePath(string title, string filter, string defaultExt, string? currentPath, bool askUseCurrent = true);

        /// <summary>
        /// Prompts the user to select an existing file path for opening.
        /// </summary>
        /// <param name="title">The title for the dialog.</param>
        /// <param name="filter">The file filter string (e.g., "Text Files|*.txt").</param>
        /// <param name="currentPath">The currently stored path for this type, if any, used for initial dir/filename.</param>
        /// <param name="askUseCurrent">This parameter is largely ignored in GUI, dialog always shown.</param>
        /// <returns>The selected file path, or null if cancelled.</returns>
        string? GetOpenFilePath(string title, string filter, string? currentPath, bool askUseCurrent = true);

        /// <summary>
        /// Displays a standard informational message to the user (e.g., in a status area).
        /// </summary>
        /// <param name="message">The message to display.</param>
        void DisplayMessage(string message);

        /// <summary>
        /// Displays a warning message to the user (e.g., highlighted in a status area).
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        void DisplayWarning(string message);

        /// <summary>
        /// Displays an error message to the user (e.g., highlighted in a status area).
        /// </summary>
        /// <param name="message">The error message to display.</param>
        void DisplayError(string message);

        /// <summary>
        /// Displays a success message to the user (e.g., highlighted in a status area).
        /// </summary>
        /// <param name="message">The success message to display.</param>
        void DisplaySuccess(string message);

        /// <summary>
        /// Clears any previous output in the status area.
        /// </summary>
        void ClearOutput();
    }
}