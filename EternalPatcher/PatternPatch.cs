using System.IO;

namespace EternalPatcher
{
    /// <summary>
    /// Pattern patch class
    /// </summary>
    public class PatternPatch : Patch
    {
        /// <summary>
        /// Byte array pattern to replace
        /// </summary>
        public byte[] Pattern;

        /// <summary>
        /// Byte array containing the patch
        /// </summary>
        public byte[] PatchByteArray;

        /// <summary>
        /// Creates a new pattern patch object
        /// </summary>
        /// <param name="id">patch identifier</param>
        /// <param name="pattern">byte array pattern to replace</param>
        /// <param name="patch">byte array containing the patch to apply</param>
        public PatternPatch(string id, byte[] pattern, byte[] patch)
        {
            Description = id;
            Pattern = pattern;
            PatchByteArray = patch;
        }

        public override bool Apply(string binaryFilePath)
        {
            // Validate the patch
            if (this.PatchByteArray == null
                || this.PatchByteArray.Length == 0
                || this.Pattern == null
                || this.Pattern.Length == 0
                || this.PatchByteArray.Length != this.Pattern.Length)
            {
                return false;
            }

            int bufferSize = 1024;
            int matches = 0;
            long currentFilePos = 0;
            long patternStartPos = -1;

            using (var fileStream = new FileStream(binaryFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                byte[] buffer = new byte[bufferSize];

                while ( fileStream.Read(buffer, 0, bufferSize) != 0)
                {
                    currentFilePos += bufferSize;

                    if (currentFilePos > fileStream.Length)
                    {
                        currentFilePos = fileStream.Length;
                    }

                    // Look for the pattern
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        if (buffer[i] == this.Pattern[matches])
                        {
                            matches++;

                            // Match found
                            if (matches == this.Pattern.Length)
                            {
                                patternStartPos = currentFilePos - (bufferSize - i) - (this.Pattern.Length - 1);
                                break;
                            }
                        }
                        else
                        {
                            matches = 0;
                        }
                    }

                    if (patternStartPos != -1)
                    {
                        break;
                    }
                }

                if (patternStartPos == -1)
                {
                    return false;
                }

                // Apply the patch
                fileStream.Position = patternStartPos;
                fileStream.Write(this.PatchByteArray, 0, this.PatchByteArray.Length);
            }

            return true;
        }
    }
}
