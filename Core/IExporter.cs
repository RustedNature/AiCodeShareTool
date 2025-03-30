
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
    }
}