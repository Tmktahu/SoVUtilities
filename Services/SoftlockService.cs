
using SoVUtilities.Resources;
using Stunlock.Core;
using Unity.Entities;
using static SoVUtilities.Services.TagService;

namespace SoVUtilities.Services;

public static class SoftlockService
{
  // Dictionary of available bosses that can be softlocked - maps boss name to PrefabGUID
  public static Dictionary<string, PrefabGUID> AvailableBosses { get; } = new Dictionary<string, PrefabGUID>
  {
    { "azariel", PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood },
    { "christina", PrefabGUIDs.CHAR_Militia_Nun_VBlood },
    { "foulrot", PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood },
    { "ben", PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood },
    { "styx", PrefabGUIDs.CHAR_BatVampire_VBlood }
  };

  // Default softlock mapping - used when no saved data exists
  // private static readonly Dictionary<string, PrefabGUID[]> DefaultSoftlockMapping = new Dictionary<string, PrefabGUID[]>
  // {
  //   { Tags.COMPASS, new PrefabGUID[] { PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood } },
  //   { Tags.SHEPHERDS, new PrefabGUID[] { PrefabGUIDs.CHAR_Militia_Nun_VBlood } },
  //   { Tags.AEGIS, new PrefabGUID[] { PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood } },
  //   { Tags.OAKSONG, new PrefabGUID[] { PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood } },
  //   { Tags.STYX, new PrefabGUID[] { PrefabGUIDs.CHAR_BatVampire_VBlood } }
  // };

  public static bool IsBossSoftlocked(Entity playerCharacter, PrefabGUID vBloodPrefabGuid)
  {
    // if they have the admin tag, they are not softlocked
    if (HasPlayerTag(playerCharacter, Tags.ADMIN))
    {
      return false;
    }

    ulong steamId = playerCharacter.GetSteamId();
    PrefabGUID[] softlockedBosses = GetAllSoftlockedBosses();
    if (softlockedBosses.Contains(vBloodPrefabGuid))
    {
      // Ensure the mapping exists
      if (!ModDataService.ModData.ContainsKey(ModDataService.SOFTLOCK_MAPPING_KEY))
      {
        return false;
      }

      var mapping = (Dictionary<string, PrefabGUID[]>)ModDataService.ModData[ModDataService.SOFTLOCK_MAPPING_KEY];

      // check if the player has the tag associated with the softlocked boss
      string tag = mapping.FirstOrDefault(x => x.Value.Contains(vBloodPrefabGuid)).Key;

      // if they have the tag, they have access to the boss
      if (HasPlayerTag(playerCharacter, tag))
      {
        return false; // not softlocked
      }

      return true;
    }

    return false;
  }

  // a function that gets us a full array of all softlocked bosses from the dictionary
  // aka concatting the values of the dictionary
  public static PrefabGUID[] GetAllSoftlockedBosses()
  {
    if (!ModDataService.ModData.ContainsKey(ModDataService.SOFTLOCK_MAPPING_KEY))
    {
      return new PrefabGUID[0];
    }

    var mapping = (Dictionary<string, PrefabGUID[]>)ModDataService.ModData[ModDataService.SOFTLOCK_MAPPING_KEY];
    return mapping.Values.SelectMany(prefabs => prefabs).ToArray();
  }  /// <summary>
     /// Associates a boss with a tag in the softlock mapping
     /// </summary>
     /// <param name="tagName">The tag name to associate the boss with</param>
     /// <param name="bossKey">The boss key from AvailableBosses dictionary</param>
     /// <returns>True if successful, false if boss key not found</returns>
  public static bool AddBossToTag(string tagName, string bossKey)
  {
    // Check if the boss key exists in available bosses
    if (!AvailableBosses.TryGetValue(bossKey, out PrefabGUID bossPrefabGuid))
    {
      Core.Log.LogWarning($"Boss key '{bossKey}' not found in available bosses");
      return false;
    }

    // Ensure the mapping exists
    if (!ModDataService.ModData.ContainsKey(ModDataService.SOFTLOCK_MAPPING_KEY))
    {
      ModDataService.ModData[ModDataService.SOFTLOCK_MAPPING_KEY] = new Dictionary<string, PrefabGUID[]>();
    }

    var mapping = (Dictionary<string, PrefabGUID[]>)ModDataService.ModData[ModDataService.SOFTLOCK_MAPPING_KEY];

    // Access the mapping directly
    if (mapping.TryGetValue(tagName, out PrefabGUID[] existingBosses))
    {
      // Check if this boss is already associated with this tag
      if (existingBosses.Contains(bossPrefabGuid))
      {
        Core.Log.LogWarning($"Boss '{bossKey}' is already associated with tag '{tagName}'");
        return true; // Not an error, just already exists
      }

      // Add the new boss to the existing array
      var newBosses = existingBosses.Concat([bossPrefabGuid]).ToArray();
      mapping[tagName] = newBosses;
    }
    else
    {
      // Create a new entry for this tag
      mapping[tagName] = [bossPrefabGuid];
    }

    // Save the data
    ModDataService.SaveData();

    Core.Log.LogInfo($"Successfully associated boss '{bossKey}' with tag '{tagName}'");
    return true;
  }

  /// <summary>
  /// Removes a boss association from a tag in the softlock mapping
  /// </summary>
  /// <param name="tagName">The tag name to remove the boss from</param>
  /// <param name="bossKey">The boss key from AvailableBosses dictionary</param>
  /// <returns>True if successful, false if boss key not found or association doesn't exist</returns>
  public static bool RemoveBossFromTag(string tagName, string bossKey)
  {
    // Check if the boss key exists in available bosses
    if (!AvailableBosses.TryGetValue(bossKey, out PrefabGUID bossPrefabGuid))
    {
      Core.Log.LogWarning($"Boss key '{bossKey}' not found in available bosses");
      return false;
    }

    // Ensure the mapping exists
    if (!ModDataService.ModData.ContainsKey(ModDataService.SOFTLOCK_MAPPING_KEY))
    {
      Core.Log.LogWarning($"Tag '{tagName}' not found in softlock mapping");
      return false;
    }

    var mapping = (Dictionary<string, PrefabGUID[]>)ModDataService.ModData[ModDataService.SOFTLOCK_MAPPING_KEY];

    // Check if the tag exists in the mapping
    if (!mapping.TryGetValue(tagName, out PrefabGUID[] existingBosses))
    {
      Core.Log.LogWarning($"Tag '{tagName}' not found in softlock mapping");
      return false;
    }

    // Check if this boss is associated with this tag
    if (!existingBosses.Contains(bossPrefabGuid))
    {
      Core.Log.LogWarning($"Boss '{bossKey}' is not associated with tag '{tagName}'");
      return false;
    }

    // Remove the boss from the array
    var newBosses = existingBosses.Where(boss => boss != bossPrefabGuid).ToArray();

    // If the array is now empty, remove the entire entry
    if (newBosses.Length == 0)
    {
      mapping.Remove(tagName);
      Core.Log.LogInfo($"Removed boss '{bossKey}' from tag '{tagName}' and cleaned up empty tag entry");
    }
    else
    {
      mapping[tagName] = newBosses;
      Core.Log.LogInfo($"Successfully removed boss '{bossKey}' from tag '{tagName}'");
    }

    // Save the data
    ModDataService.SaveData();

    return true;
  }
}
