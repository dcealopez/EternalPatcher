using System;
using System.IO;
using System.Security.Cryptography;

namespace EternalPatcher
{
    /// <summary>
    /// Utility class
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Computes the MD5 checksum of the given file at the
        /// specified file path
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>the MD5 checksum of the file at the given file path</returns>
        public static string GetFileMD5Checksum(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var md5 = MD5.Create())
                {
                    return BitConverter.ToString(md5.ComputeHash(fileStream)).Replace("-", "").ToLower();
                }
            }
        }
    }
}
