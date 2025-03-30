
# AiCodeShareTool

**Disclaimer:** This program was developed entirely by the Gemini 1.5 Pro language model from Google AI, based on a series of prompts defining requirements and requesting code generation and modifications.

A simple Windows Forms application to export the text-based files from a project directory into a single, structured text file (suitable for sharing with AI models) and to import such a file back into a directory structure.

## Features

*   **Export:**
    *   Select a project root directory.
    *   Select an output text file path.
    *   **Language Profiles:** Select a configuration profile (e.g., ".NET", "Python", "Generic Text") to control which files are included. Profiles are loaded from `language_profiles.json` next to the executable.
        *   Each profile defines specific file search patterns (e.g., `*.cs`, `*.py`).
        *   Each profile defines blacklisted file extensions and specific filenames to exclude.
    *   Recursively finds files based on the selected profile's patterns.
    *   Excludes common unwanted folders globally (`bin`, `obj`, `.vs`, `.git`, `node_modules`).
    *   **Preview:** Button to show a list of files that *will* be exported based on current settings *before* performing the full export.
    *   **Character Count:** Displays the total number of characters exported after a successful export (or copy).
    *   Writes all found file contents into the output file, delimited by standard markers indicating the relative path.
    *   **Copy to Clipboard:** Button to perform the export directly to the system clipboard instead of saving to a file.
*   **Import:**
    *   Select a target root directory (where the project structure should be recreated).
    *   Select the structured text file to import from (format is language-agnostic).
    *   **Backup Option:** Prompts the user before import if they want to create a `.zip` backup of the target directory first.
    *   Parses the file based on the start/end file markers.
    *   Recreates the directory structure under the target directory.
    *   Writes the content for each file found in the import file. Existing files are overwritten.
    *   Includes basic security checks to prevent writing outside the target directory.
*   **GUI:**
    *   Simple Windows Forms interface with Menu Bar.
    *   Uses standard file/folder browse dialogs.
    *   Dropdown to select the Language Profile for export.
    *   Displays status messages, warnings, and errors in a text area.
    *   **Clear Status:** Button to clear the status message area.
    *   **Persistence:** Remembers the last used project directory, export/import file paths, and selected language profile between sessions.
    *   **Drag and Drop:** Allows dragging folders onto the "Project Directory" field and text files onto the "Export File Path" and "Import File Path" fields.
    *   **Tooltips:** Hover text explaining the purpose of buttons and inputs.

## Building and Running

1.  Make sure you have a compatible .NET SDK installed (e.g., .NET 8, .NET 9) that supports Windows Forms (`-windows` TargetFramework).
2.  Open the solution (`.sln`) file in Visual Studio or use the .NET CLI.
3.  Build the solution (Build -> Build Solution in VS, or `dotnet build` in CLI).
4.  Run the application (Debug -> Start Debugging in VS, or run the executable from the `bin` folder, e.g., `bin/Debug/net9.0-windows/AiCodeShareTool.exe`).

## Usage

1.  **Project Directory:** Click "Browse..." or drag and drop a folder to select the root folder of the project you want to export from or import into.
2.  **Export File Path:** Click "Browse..." or drag and drop a `.txt` file to select the file where the exported code should be saved (used by "Export Project" button).
3.  **Import File Path:** Click "Browse..." or drag and drop a `.txt` file to select the file containing the code to be imported.
4.  **Language Profile:** Select the appropriate profile from the dropdown *before* exporting or previewing. This determines which files are included. Profiles are loaded from `language_profiles.json`. Use **File -> Reload Profiles** to load external changes.
5.  **Click "Preview Export":** (Optional) To see a list of files that will be included in the export based on the current Project Directory and selected Language Profile.
6.  **Click "Export Project":** To gather files from the Project Directory (using the selected Language Profile) and save them to the Export File Path. The character count will be shown in the status area upon completion.
7.  **Click "Copy to Clipboard":** To gather files from the Project Directory (using the selected Language Profile) and copy the resulting formatted text directly to the clipboard.
8.  **Click "Import Code":** To read the Import File Path and recreate the files/folders within the Project Directory. You will be asked if you want to create a backup first. **This operation WILL OVERWRITE existing files.** It ignores the selected language profile as the file format dictates the content.
9.  Observe the status messages in the text area below the buttons. Click "Clear" to clear the status area.
10. Use the **File** menu to Reload Profiles or Exit. Use the **Help** menu for basic application info.

## Configuration

*   **Language Profiles:** Edit the `language_profiles.json` file located in the same directory as the application's executable (`.exe`). You can:
    *   Modify existing profiles (change `Name`, `SearchPatterns`, `BlacklistedExtensions`, `BlacklistedFileNames`).
    *   Add new profiles by copying an existing structure and customizing it.
    *   Delete profiles by removing their JSON object from the list.
    *   Use **File -> Reload Profiles** in the application to load changes without restarting.
    *   *Note:* Ensure JSON syntax is correct after editing. Use a JSON validator if unsure.
*   **Global Exclusions:** Edit the list of excluded folders in `Core/FileSystemExporter.cs` (`GetExcludedFolders` method) if needed. Recompilation is required for changes here.
*   **File Format:** Marker constants are defined in `Configuration/ExportSettings.cs`.

## AI Interaction Format (System Instruction)

This tool is designed to facilitate code sharing with AI models. The AI used to generate this tool (Gemini 1.5 Pro) expects interactions using the following format when requesting code updates or generation:

```text
Output Only New/Edited Files: Your response MUST only contain files that are newly created or have been edited/modified according to the current request. Do NOT include files requested in previous interactions if they remain unchanged.

Encapsulation: All code for the new or edited files MUST be contained within one single markdown code block (e.g., ```csharp ... ``` or ```xml ... ``` as appropriate). Do NOT add any introductory or concluding text outside of this code block unless explicitly requested.

File Markers: Each individual file's content included in the output MUST be clearly delineated by specific start and end markers.

Start Marker Format: Before the content of each file, you MUST include a line exactly like this:


// === Start File: {relativePath} ===

End Marker Format: After the content of each file, you MUST include a line exactly like this:


// === End File: {relativePath} ===


Path Requirement: The `{relativePath}` placeholder in the markers MUST be the correct relative path of the file *from the project's base directory*, even if it's an existing file being modified.
    *   Use forward slashes (/) as directory separators.
    *   Examples: `MyClass.cs`, `Services/UserService.cs`, `Models/Order.cs`.

Content Placement: The complete and unmodified content of the new or edited file MUST be placed between its corresponding Start File and End File markers.

Spacing:
    *   Include exactly one blank line immediately after the Start File marker line (before the file content).
    *   Include exactly one blank line immediately before the End File marker line (after the file content).
    *   Include exactly one blank line immediately after the End File marker line (before the next Start File marker, or the end of the code block).

Omission of Unchanged Files: Files provided in previous responses that are not explicitly part of the changes requested in the current prompt MUST be omitted from the output block.

No Extra Text: Do not include any other text or comments between the files, other than the specified markers and blank lines.
```

Adhering to this format allows the AI to correctly process code changes and maintain project context across multiple interactions.
