# SoVUtilities

A custom made VRising mod for the Shadows of Vardoran RP community. It includes a variety of custom built features for the server.

## Installation
1. Build the project or download the DLL from the Releases section.
2. Copy the DLL to your VRising BepInEx plugins folder.

## Tagging System
Admins are able to add tags to players. Valid tags are hardcoded for now.

When tags are assigned to players, they are mapped to the player by their Steam ID. A JSON file is created in the `/BepInEx/config/SoVUtilities` folder by the name of `sov_utilities_player_data.json`. This contains all of the tag data applied to characters and is loaded when the server boots up.

## Boss Softlocking System
Bosses can be softlocked. This means that when a player kills the boss, they get no progression. It is as if they never killed it.

This integrates with the Tagging System. Softlocked bosses are associated with a tag, and if the player has the tag then they can obtain the boss's progression data.

Currently this is all hardcoded.

## Credits
- Chunks of the code and concepts are taken directly from Bloodcraft (https://github.com/mfoltz/Bloodcraft), licensed under CC BY-NC 4.0.