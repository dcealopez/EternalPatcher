using System.Collections.Generic;

namespace EternalPatcher
{
    /// <summary>
    /// Game build class
    /// </summary>
    public class GameBuild
    {
        /// <summary>
        /// Game build identifier
        /// </summary>
        public string Id;

        /// <summary>
        /// Build executable file name
        /// </summary>
        public string ExecutableFileName;

        /// <summary>
        /// Game build executable MD5 checksum
        /// </summary>
        public string MD5Checksum;

        /// <summary>
        /// Patch group ids assigned to this game build
        /// </summary>
        public List<string> PatchGroupIds;

        /// <summary>
        /// Available patches for the game build
        /// </summary>
        public List<Patch> Patches = new List<Patch>();

        /// <summary>
        /// Creates a new game build object
        /// </summary>
        /// <param name="id">game build id</param>
        /// <param name="executableFileName">executable file name</param>
        /// <param name="md5Checksum">game executable md5 checksum</param>
        /// <param name="patchGroupIds">patch group ids associated with this game build</param>
        public GameBuild(string id, string executableFileName, string md5Checksum, List<string> patchGroupIds)
        {
            Id = id;
            ExecutableFileName = executableFileName;
            MD5Checksum = md5Checksum;
            PatchGroupIds = patchGroupIds;
        }
    }
}
