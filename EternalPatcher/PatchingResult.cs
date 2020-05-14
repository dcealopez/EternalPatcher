namespace EternalPatcher
{
    /// <summary>
    /// Class used to store the result of applying a patch
    /// </summary>
    public class PatchingResult
    {
        /// <summary>
        /// Offset patch applied
        /// </summary>
        public OffsetPatch Patch;

        /// <summary>
        /// Patching result message
        /// </summary>
        public bool Success;

        /// <summary>
        /// Creates a new patching result object
        /// </summary>
        /// <param name="patch">offset patch</param>
        /// <param name="message">message</param>
        public PatchingResult(OffsetPatch patch, bool success)
        {
            Patch = patch;
            Success = success;
        }
    }
}
