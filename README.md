
# AiCodeShareTool

A simple Windows Forms application to export the text-based files from a project directory into a single, structured text file (suitable for sharing with AI models) and to import such a file back into a directory structure.

## Features

*   **Export:**
    *   Select a project root directory.
    *   Select an output text file path.
    *   Recursively finds code/config files (based on common extensions).
    *   Excludes `bin`, `obj`, `.vs` folders.
    *   Excludes configurable file types/names (e.g., `.user`, `.log`, `launchSettings.json`).
    *   Writes all found file contents into the output file, delimited by markers indicating the relative path.
*   **Import:**
    *   Select a target root directory (where the project structure should be recreated).
    *   Select the structured text file to import from.
    *   Parses the file based on the start/end file markers.
    *   Recreates the directory structure under the target directory.
    *   Writes the content for each file found in the import file.
    *   Includes basic security checks to prevent writing outside the target directory.
*   **GUI:**
    *   Simple Windows Forms interface.
    *   Uses standard file/folder browse dialogs.
    *   Displays status messages, warnings, and errors in a text area.

## Building and Running

1.  Make sure you have a compatible .NET SDK installed (e.g., .NET 8, .NET 9) that supports Windows Forms (`-windows` TargetFramework).
2.  Open the solution (`.sln`) file in Visual Studio or use the .NET CLI.
3.  Build the solution (Build -> Build Solution in VS, or `dotnet build` in CLI).
4.  Run the application (Debug -> Start Debugging in VS, or run the executable from the `bin` folder, e.g., `bin/Debug/net9.0-windows/AiCodeShareTool.exe`).

## Usage

1.  **Project Directory:** Click "Browse..." to select the root folder of the project you want to export from or import into.
2.  **Export File Path:** Click "Browse..." to select the `.txt` file where the exported code should be saved (for Export) or the file containing the code to be imported (for Import).
3.  **Click "Export Project":** To gather files from the Project Directory and save them to the Export File Path.
4.  **Click "Import Code":** To read the Export File Path and recreate the files/folders within the Project Directory.
5.  Observe the status messages in the text area below the buttons.

## Configuration

*   **File Exclusions:** Edit the `BlacklistedExtensions` and `BlacklistedFileNames` sets in `Configuration/ExportSettings.cs` to customize which files are ignored during export.
*   **Search Patterns:** Modify `DefaultSearchPatterns` in `Configuration/ExportSettings.cs` to change which file types are included in the export.