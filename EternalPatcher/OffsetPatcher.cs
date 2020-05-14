using System.Collections.Generic;
using System.IO;

namespace EternalPatcher
{
    public static class OffsetPatcher
    {
        /// <summary>
        /// Patches the given binary file by replacing bytes at the given offset        
        /// </summary>
        /// <param name="binaryFilePath">path to the binary file to patch</param>
        /// <param name="patches">offset patches to apply</param>
        /// <returns>list of the results of applying each patch</returns>
        public static List<PatchingResult> Patch(string binaryFilePath, List<OffsetPatch> patches)
        {
            var patchResults = new List<PatchingResult>();

            if (patches == null || patches.Count == 0)
            {
                return patchResults;
            }

            // Validate the patches
            for (var i = patches.Count - 1; i >= 0; i--)
            {
                // Invalid patch
                if (patches[i].PatchByteArray == null
                    || patches[i].PatchByteArray.Length == 0)
                {
                    patchResults.Add(new PatchingResult(patches[i], false));

                    // Remove this patch from the list
                    patches.RemoveAt(i);
                }
            }

            using (var fileStream = new FileStream(binaryFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                foreach (var patch in patches)
                {
                    // Check if the patch is valid
                    if (patch.Offset < 0
                        || patch.Offset > fileStream.Length - 1
                        || patch.Offset + patch.PatchByteArray.Length > fileStream.Length - 1)
                    {
                        patchResults.Add(new PatchingResult(patch, false));
                        continue;
                    }

                    // Apply the patch
                    fileStream.Position = patch.Offset;

                    for (int i = 0; i < patch.PatchByteArray.Length; i++)
                    {
                        fileStream.WriteByte(patch.PatchByteArray[i]);
                    }

                    patchResults.Add(new PatchingResult(patch, true));
                }
            }

            return patchResults;
        }
    }
}
