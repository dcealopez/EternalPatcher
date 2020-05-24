namespace EternalPatcher
{
    /// <summary>
    /// Patch interface
    /// </summary>
    public interface IPatch
    {
        /// <summary>
        /// Apply the patch to the given binary file at the given file path
        /// </summary>
        /// <param name="binaryFilePath">binary file path</param>
        /// <returns>true if sucessful, false if not</returns>
        bool Apply(string binaryFilePath);
    }
}
