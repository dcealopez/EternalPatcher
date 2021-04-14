# EternalPatcher


**DOOM Eternal patcher for modding purposes.**

This tool patches the DOOM Eternal game executable for modding purposes. The patches are defined in a "patch definitions file" that is automatically downloaded and updated from the update server specified in the configuration file.

**Official DOOM 2016 and DOOM Eternal modding Discord server:** https://discord.gg/W9t4nZa

### Features

 - User friendly GUI to make patching the game easy.
 - Backs up the game executable before applying patches (optional)
 - Automatic updates for the patch definitions.
 - Can be used through the command-line, no need for the GUI.

### Command-line usage

This tool can also be used through the command-line without using the GUI. The following arguments can be used:

```
--update - Checks for updates and downloads them if available
--patch <file path> - Patches the given game executable
```

### Update server

The update server can be configured in the configuration file of the application by changing the "UpdateServer" key. The specified server must serve the following files:

 - EternalPatcher.def
   - File containing the patch definitions. This is the file that is downloaded.
 - EternalPatcher.md5
   - File containing the MD5 checksum of the patch definitions file above. This file must be a one-liner with no line breaks. This is the file used to check for updates.
   
The default update server specified in the configuration file is hosted by myself and I will keep the patch definitions updated.

### Patch definitions file

An example of a patch definitions file and it's syntax:

```
# game build definitions
# id=executable name:md5 checksum:patch group ids (comma separated)
steamv1=DOOMEternalx64vk.exe:7ea73e0ee1a2066dc43502930ededced:global
steamdlc2=DOOMEternalx64vk.exe:96556f8b0dfc56111090a6b663969b86:global,dlc2
bnetv1=DOOMEternalx64vk.exe:7bb1e931cbbbc3d2d3cea1dd6df05539:global
bnetdlc2=DOOMEternalx64vk.exe:b4eef9284826e5ffaedbcd73fe6d2ae6:global,dlc2

# patches
# for patches with the same id (description), to support multiple game builds for different patterns, position the 'global' patch as the last one
# otherwise it will override other patches with different patch group ids
# syntax -> patch=description (serves as the id):type (offset|pattern):(compatible patch group ids (comma separated)):(offset|pattern):hex patch

# skip data checksum checks (by emoose)
patch=skip data checksum checks:pattern:global:741E8B534841B8EFBEADDE:EB1E8B534841B8EFBEADDE

# unrestrict binds (by SunBeam, ported by emoose)
patch=unrestrict binds:pattern:dlc2:084C8B03BA01:084C8B03BA00
patch=unrestrict binds:pattern:global:084C8B0FBA01:084C8B0FBA00
```
