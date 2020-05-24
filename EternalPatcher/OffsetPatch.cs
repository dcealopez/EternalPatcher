using System.IO;

namespace EternalPatcher
{
    /// <summary>
    /// Offset Patch class
    /// </summary>
    public class OffsetPatch : Patch
    {
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

        public override bool Apply(string binaryFilePath)
        {
            // Validate the patch
            if (this.PatchByteArray == null
                || this.PatchByteArray.Length == 0)
            {
                return false;
            }

            using (var fileStream = new FileStream(binaryFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                // Check if the patch is valid
                if (this.Offset < 0
                    || this.Offset > fileStream.Length - 1
                    || this.Offset + this.PatchByteArray.Length > fileStream.Length - 1)
                {
                    return false;
                }

                // Apply the patch
                fileStream.Position = this.Offset;

                for (int i = 0; i < this.PatchByteArray.Length; i++)
                {
                    fileStream.WriteByte(this.PatchByteArray[i]);
                }
            }

            return true;
        }
    }
}
