
# AiCodeShareTool

A simple Windows Forms application to export the text-based files from a project directory into a single, structured text file (suitable for sharing with AI models) and to import such a file back into a directory structure.

## Features

*   **Export:**
    *   Select a project root directory.
    *   Select an output text file path.
    *   **Language Profiles:** Select a configuration profile (e.g., ".NET", "Python", "Generic Text") to control which files are included.
        *   Each profile defines specific file search patterns (e.g., `*.cs`, `*.py`).
        *   Each profile defines blacklisted file extensions and specific filenames to exclude.
    *   Recursively finds files based on the selected profile's patterns.
    *   Excludes common unwanted folders globally (`bin`, `obj`, `.vs`, `.git`, `node_modules`).
    *   Writes all found file contents into the output file, delimited by standard markers indicating the relative path.
*   **Import:**
    *   Select a target root directory (where the project structure should be recreated).
    *   Select the structured text file to import from (format is language-agnostic).
    *   Parses the file based on the start/end file markers.
    *   Recreates the directory structure under the target directory.
    *   Writes the content for each file found in the import file.
    *   Includes basic security checks to prevent writing outside the target directory.
*   **GUI:**
    *   Simple Windows Forms interface.
    *   Uses standard file/folder browse dialogs.
    *   Dropdown to select the Language Profile for export.
    *   Displays status messages, warnings, and errors in a text area.

## Building and Running

1.  Make sure you have a compatible .NET SDK installed (e.g., .NET 8, .NET 9) that supports Windows Forms (`-windows` TargetFramework).
2.  Open the solution (`.sln`) file in Visual Studio or use the .NET CLI.
3.  Build the solution (Build -> Build Solution in VS, or `dotnet build` in CLI).
4.  Run the application (Debug -> Start Debugging in VS, or run the executable from the `bin` folder, e.g., `bin/Debug/net9.0-windows/AiCodeShareTool.exe`).

## Usage

1.  **Project Directory:** Click "Browse..." to select the root folder of the project you want to export from or import into.
2.  **Export File Path:** Click "Browse..." to select the `.txt` file where the exported code should be saved.
3.  **Import File Path:** Click "Browse..." to select the `.txt` file containing the code to be imported.
4.  **Language Profile:** Select the appropriate profile from the dropdown *before* exporting. This determines which files are included.
5.  **Click "Export Project":** To gather files from the Project Directory (using the selected Language Profile) and save them to the Export File Path.
6.  **Click "Import Code":** To read the Import File Path and recreate the files/folders within the Project Directory. This operation ignores the selected language profile as the file format dictates the content.
7.  Observe the status messages in the text area below the buttons.

## Configuration

*   **Language Profiles:** Edit the `InitializeProfiles` method in `Configuration/InMemoryConfigurationService.cs` to:
    *   Modify existing profiles (search patterns, blacklists).
    *   Add new profiles for other languages or project types.
*   **Global Exclusions:** Edit the list of excluded folders in `Core/FileSystemExporter.cs` (`excludedFolders` variable within the `Export` method) if needed.
*   **File Format:** Marker constants are defined in `Configuration/ExportSettings.cs`.