
using System.Text; // Added using directive for StringBuilder

namespace AiCodeShareTool.Core
{
    /// <summary>
    /// Defines the contract for exporting project files.
    /// The implementation will use the currently active language profile.
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// Exports files from a project directory to a specified output file,
        /// using the currently active language profile configuration.
        /// </summary>
        /// <param name="projectDirectory">The root directory of the project to export.</param>
        /// <param name="exportFilePath">The path to the file where the exported content will be saved.</param>
        void Export(string projectDirectory, string exportFilePath);

         /// <summary>
         /// Generates a list of files that would be included in an export based on the current settings.
         /// </summary>
         /// <param name="projectDirectory">The root directory of the project.</param>
         /// <returns>A list of relative file paths, or null if an error occurred.</returns>
         List<string>? PreviewExport(string projectDirectory);

         /// <summary>
         /// Exports the project content directly to a string builder.
         /// </summary>
         /// <param name="projectDirectory">The root directory of the project to export.</param>
         /// <param name="totalCharacters">Output parameter for the total characters exported (content only).</param>
         /// <returns>A StringBuilder containing the formatted export content, or null if a critical error occurred during setup or file searching.</returns>
         StringBuilder? ExportToStringBuilder(string projectDirectory, out long totalCharacters);
    }
}