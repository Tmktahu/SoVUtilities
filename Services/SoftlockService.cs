
using SoVUtilities.Resources;
using Stunlock.Core;
using Unity.Entities;
using static SoVUtilities.Services.TagService;

namespace SoVUtilities.Services;

public static class SoftlockService
{
  // dictionary of string keys to prefabguild arrays
  public static Dictionary<string, PrefabGUID[]> SoftlockMapping { get; } = new Dictionary<string, PrefabGUID[]>
  {
    { Tags.COMPASS, new PrefabGUID[] { PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood } }, // Prefab 'PrefabGuid(114912615)': CHAR_ChurchOfLight_Cardinal_VBlood
    { Tags.SHEPHERDS, new PrefabGUID[] { PrefabGUIDs.CHAR_Militia_Nun_VBlood } }, // Prefab 'PrefabGuid(-99012450)': CHAR_Militia_Nun_VBlood
    { Tags.AEGIS, new PrefabGUID[] { PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood } }, // Prefab 'PrefabGuid(-1208888966)': CHAR_Undead_ZealousCultist_VBlood
    { Tags.OAKSONG, new PrefabGUID[] { PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood } } // Prefab 'PrefabGuid(109969450)': CHAR_Villager_CursedWanderer_VBlood
  };

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
      // check if the player has the tag associated with the softlocked boss
      string tag = SoftlockMapping.FirstOrDefault(x => x.Value.Contains(vBloodPrefabGuid)).Key;

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
    return SoftlockMapping.Values.SelectMany(prefabs => prefabs).ToArray();
  }
}
