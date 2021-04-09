using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
        /// Gets the current patcher version
        /// </summary>
        /// <returns>the current patcher version string</returns>
        private static string GetPatcherVersion()
        {
            return $"v{typeof(Patcher).Assembly.GetName().Version.Major}";
        }

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
                var latestPatchDefMd5Checksum = webClient.DownloadString($"http://{updateServerIp}/{PatchDefinitionsFileNameBase}_{GetPatcherVersion()}.md5");

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
                 webClient.DownloadFile(
                     $"http://{updateServerIp}/{PatchDefinitionsFileNameBase}_{GetPatcherVersion()}.def",
                     $"{PatchDefinitionsFileNameBase}.def");
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
                    // id=executable name:md5 checksum:patch group ids (comma separated)
                    if (!dataDefinition[0].Equals("patch", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string[] gameBuildData = dataDefinition[1].Split(':');

                        for (var i = 0; i < gameBuildData.Length; i++)
                        {
                            gameBuildData[i] = gameBuildData[i].Trim();
                        }

                        // Bad syntax, skip this line
                        if (gameBuildData.Length != 3)
                        {
                            continue;
                        }

                        GameBuilds.Add(new GameBuild(dataDefinition[0], gameBuildData[0], gameBuildData[1], gameBuildData[2].Split(',').ToList()));
                    }
                    else
                    {
                        // patch defintion
                        // syntax:
                        // patch=description:type (offset|pattern):(compatible build ids (comma separated)|all):(offset|pattern):hex patch
                        string[] patchData = dataDefinition[1].Split(':');

                        for (var i = 0; i < patchData.Length; i++)
                        {
                            patchData[i] = patchData[i].Trim();
                        }

                        // Bad syntax, skip this line
                        if (patchData.Length != 5)
                        {
                            continue;
                        }

                        // Bad patch, skip this line
                        if (patchData[4].Length % 2 != 0)
                        {
                            continue;
                        }

                        // Get the patch type
                        var patchType = typeof(Patch);

                        if (patchData[1].Equals("offset", StringComparison.InvariantCultureIgnoreCase))
                        {
                            patchType = typeof(OffsetPatch);
                        }
                        else if (patchData[1].Equals("pattern", StringComparison.InvariantCultureIgnoreCase))
                        {
                            patchType = typeof(PatternPatch);
                        }
                        else
                        {
                            // Bad patch, skip this line
                            continue;
                        }

                        // Patch specific checks now
                        if (patchType == typeof(PatternPatch))
                        {
                            // Bad patch, skip this line
                            if (patchData[3].Length % 2 != 0)
                            {
                                continue;
                            }
                        }

                        // Get the patch group ids assigned to this patch
                        string[] patchGroupIds = patchData[2].Split(',');

                        // Bad patch, skip this line
                        if (patchGroupIds.Length == 0)
                        {
                            continue;
                        }

                        for (var i = 0; i < patchGroupIds.Length; i++)
                        {
                            patchGroupIds[i] = patchGroupIds[i].Trim();
                        }

                        // Load the hex patch
                        byte[] hexPatch = new byte[patchData[4].Length / 2];

                        for (var i = 0; i < patchData[4].Length; i += 2)
                        {
                            hexPatch[i / 2] = Convert.ToByte(patchData[4].Substring(i, 2), 16);
                        }

                        Patch patch;

                        if (patchType == typeof(OffsetPatch))
                        {
                            // Load the offset and create the offset patch
                            patch = new OffsetPatch(patchData[0], Convert.ToInt64(patchData[3], 16), hexPatch);
                        }
                        else
                        {
                            // Load the pattern and create the pattern patch
                            byte[] hexPattern = new byte[patchData[3].Length / 2];

                            for (var i = 0; i < patchData[3].Length; i += 2)
                            {
                                hexPattern[i / 2] = Convert.ToByte(patchData[3].Substring(i, 2), 16);
                            }

                            patch = new PatternPatch(patchData[0], hexPattern, hexPatch);
                        }

                        // Bad patch, skip this line
                        if (patch == null)
                        {
                            continue;
                        }

                        // Assign the patch to the specified game builds with matching patch group ids
                        for (var i = 0; i < patchGroupIds.Length; i++)
                        {
                            foreach (var gameBuild in GameBuilds)
                            {
                                if (gameBuild.PatchGroupIds.Contains(patchGroupIds[i]))
                                {
                                    // Don't add the patch if another with the same id was already added
                                    bool alreadyExists = false;

                                    foreach (var currentPatch in gameBuild.Patches)
                                    {
                                        if (currentPatch.Description.Equals(patch.Description, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            alreadyExists = true;
                                            break;
                                        }
                                    }

                                    if (alreadyExists)
                                    {
                                        break;
                                    }

                                    gameBuild.Patches.Add(patch);
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

        /// <summary>
        /// Applies the given patches to the given binary file at the given file path
        /// </summary>
        /// <param name="binaryFilePath">binary file path</param>
        /// <param name="patches">patch list</param>
        /// <returns>list containing the patching result of each patch</returns>
        public static List<PatchingResult> ApplyPatches(string binaryFilePath, List<Patch> patches)
        {
            var patchingResults = new List<PatchingResult>();

            foreach (var patch in patches)
            {
                patchingResults.Add(new PatchingResult(patch, patch.Apply(binaryFilePath)));
            }

            return patchingResults;
        }
    }
}
