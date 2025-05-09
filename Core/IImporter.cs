
namespace AiCodeShareTool.Core
{
    /// <summary>
    /// Defines the contract for importing code from a structured file.
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// Imports files from a specified input file into a target project directory.
        /// </summary>
        /// <param name="projectDirectory">The root directory where the imported files will be placed.</param>
        /// <param name="importFilePath">The path to the file containing the code to import.</param>
        void Import(string projectDirectory, string importFilePath);

         /// <summary>
         /// Imports files from a specified input file into a target project directory,
         /// optionally creating a backup first.
         /// </summary>
         /// <param name="projectDirectory">The root directory where the imported files will be placed.</param>
         /// <param name="importFilePath">The path to the file containing the code to import.</param>
         /// <param name="createBackup">Whether to create a zip backup of the projectDirectory before importing.</param>
        void Import(string projectDirectory, string importFilePath, bool createBackup);
    }
}