namespace EternalPatcher
{
    public class OffsetPatch
    {
        /// <summary>
        /// Patch description
        /// </summary>
        public string Description;

        /// <summary>
        /// Offset where the patch will be applied
        /// </summary>
        public long Offset;

        /// <summary>
        /// Byte array containing the patch
        /// </summary>
        public byte[] PatchByteArray;

        /// <summary>
        /// Creates a new offset patch object
        /// </summary>
        /// <param name="id">patch identifier</param>
        /// <param name="offset">offset</param>
        /// <param name="patch">byte array containing the patch to apply</param>
        public OffsetPatch(string id, long offset, byte[] patch)
        {
            Description = id;
            Offset = offset;
            PatchByteArray = patch;
        }
    }
}
