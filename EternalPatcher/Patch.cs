namespace EternalPatcher
{
    /// <summary>
    /// Base patch class
    /// </summary>
    public abstract class Patch : IPatch
    {
        /// <summary>
        /// Patch description
        /// </summary>
        public string Description;

        public abstract bool Apply(string binaryFilePath);
    }
}
