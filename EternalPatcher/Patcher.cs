using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;

namespace EternalPatcher
{
    /// <summary>
    /// Main patcher class
    /// </summary>
    public static class Patcher
    {
        /// <summary>
        /// Base file name for the patch definitions file
        /// </summary>
        private const string PatchDefinitionsFileNameBase = "EternalPatcher";

        /// <summary>
        /// Game build list
        /// </summary>
        private static List<GameBuild> GameBuilds = new List<GameBuild>();      

        /// <summary>
        /// Check if there are any patches loaded
        /// </summary>
        /// <returns>true if there are any patches loaded, false if not</returns>
        public static bool AnyPatchesLoaded()
        {
            if (GameBuilds == null || GameBuilds.Count == 0)
            {
                return false;
            }

            foreach (var build in GameBuilds)
            {
                if (build.Patches != null && build.Patches.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if there is any update available
        /// </summary>
        /// <returns>true if there is any update available, false if not</returns>
        public static bool AnyUpdateAvailable()
        {
            if (!File.Exists($"./{PatchDefinitionsFileNameBase}.def"))
            {
                return true;
            }

            var currentPatchDefMd5Checksum = Util.GetFileMD5Checksum($"{PatchDefinitionsFileNameBase}.def");
            var updateServerIp = ConfigurationManager.AppSettings.Get("UpdateServer");

            using (var webClient = new WebClient())
            {
                var latestPatchDefMd5Checksum = webClient.DownloadString($"http://{updateServerIp}/{PatchDefinitionsFileNameBase}.md5");

                if (!currentPatchDefMd5Checksum.Equals(latestPatchDefMd5Checksum))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Downloads the latest patch definitions file
        /// </summary>
        public static void DownloadLatestPatchDefinitions()
        {
            var updateServerIp = ConfigurationManager.AppSettings.Get("UpdateServer");

            using (var webClient = new WebClient())
            {
                 webClient.DownloadFile($"http://{updateServerIp}/{PatchDefinitionsFileNameBase}.def", $"{PatchDefinitionsFileNameBase}.def");
            }
        }

        /// <summary>
        /// Loads the patch definitions file
        /// </summary>
        public static void LoadPatchDefinitions()
        {
            if (!File.Exists($"{PatchDefinitionsFileNameBase}.def"))
            {
                return;
            }

            // Clear the currently loaded patches
            if (GameBuilds != null)
            {
                GameBuilds.Clear();
            }
            else
            {
                GameBuilds = new List<GameBuild>();
            }

            // Parse the patch definitions file
            using (var streamReader = new StreamReader($"{PatchDefinitionsFileNameBase}.def"))
            {
                while (streamReader.Peek() != -1)
                {
                    string currentLine = streamReader.ReadLine().Trim();

                    // Skip comments
                    if (currentLine.StartsWith("#"))
                    {
                        continue;
                    }

                    // Load the game build and patch definitions
                    string[] dataDefinition = currentLine.Split('=');

                    // Skip bad lines
                    if (dataDefinition.Length <= 1)
                    {
                        continue;
                    }

                    // 'patch' keyword is reserved, assume this is a game build definition        
                    // syntax:
                    // id=executable name:md5 checksum
                    if (!dataDefinition[0].Equals("patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string[] gameBuildData = dataDefinition[1].Split(':');

                        for (var i = 0; i < gameBuildData.Length; i++)
                        {
                            gameBuildData[i] = gameBuildData[i].Trim();
                        }

                        // Bad syntax, skip this line
                        if (gameBuildData.Length != 2)
                        {
                            continue;
                        }

                        // TODO: Validate MD5 checksum
                        
                        GameBuilds.Add(new GameBuild(dataDefinition[0], gameBuildData[0], gameBuildData[1]));
                    }
                    else
                    {
                        // patch defintion
                        // syntax:
                        // patch=description:compatible build ids (comma separated):offset:hex patch

                        string[] patchData = dataDefinition[1].Split(':');

                        for (var i = 0; i < patchData.Length; i++)
                        {
                            patchData[i] = patchData[i].Trim();
                        }

                        // Bad syntax, skip this line
                        if (patchData.Length != 4)
                        {
                            continue;
                        }

                        // Bad patch, skip this line
                        if (patchData[3].Length % 2 != 0)
                        {
                            continue;
                        }

                        // Get the game builds assigned to this patch
                        string[] gameBuilds = patchData[1].Split(',');

                        // Bad patch, skip this line
                        if (gameBuilds.Length == 0)
                        {
                            continue;
                        }

                        for (var i = 0; i < gameBuilds.Length; i++)
                        {
                            gameBuilds[i] = gameBuilds[i].Trim();
                        }

                        // Load the offset
                        long offset = Convert.ToInt64(patchData[2], 16); ;

                        // Load the hex patch
                        byte[] patch = new byte[patchData[3].Length / 2];

                        for (var i = 0; i < patchData[3].Length; i += 2)
                        {
                            patch[i / 2] = Convert.ToByte(patchData[3].Substring(i, 2), 16);
                        }

                        var offsetPatch = new OffsetPatch(patchData[0], offset, patch);

                        // Assign the patch to the specified game builds
                        for (var i = 0; i < gameBuilds.Length; i++)
                        {
                            foreach (var gameBuild in GameBuilds)
                            {
                                if (gameBuild.Id.Equals(gameBuilds[i]))
                                {
                                    gameBuild.Patches.Add(offsetPatch);
                                }
                            }
                        }
                    }
                }                
            }
        }

        /// <summary>
        /// Gets the game build of the given game executable file located
        /// at the given file path
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>the game build object of the given game executable file
        /// at the given file path</returns>
        public static GameBuild GetGameBuild(string filePath)
        {
            var fileMd5Checksum = Util.GetFileMD5Checksum(filePath);

            foreach (var build in GameBuilds)
            {
                if (build.MD5Checksum.Equals(fileMd5Checksum)
                    && Path.GetFileName(filePath).Equals(build.ExecutableFileName))
                {
                    return build;
                }
            }

            return null;
        }
    }
}
