
using Stunlock.Core;
using Unity.Entities;
using static SoVUtilities.Services.PlayerDataService;

namespace SoVUtilities.Services;

public static class SoftlockService
{
  public static string ADMIN_TAG = "admin";
  public static string COMPASS_TAG = "compass";
  public static string SHEPHERDS_TAG = "shepherds";
  public static string AEGIS_TAG = "aegis";
  public static string OAKSONG_TAG = "oaksong";

  // dictionary of string keys to prefabguild arrays
  public static Dictionary<string, PrefabGUID[]> SoftlockMapping { get; } = new Dictionary<string, PrefabGUID[]>
  {
    { COMPASS_TAG, new PrefabGUID[] { new PrefabGUID(114912615) } }, // Prefab 'PrefabGuid(114912615)': CHAR_ChurchOfLight_Cardinal_VBlood
    { SHEPHERDS_TAG, new PrefabGUID[] { new PrefabGUID(-99012450) } }, // Prefab 'PrefabGuid(-99012450)': CHAR_Militia_Nun_VBlood
    { AEGIS_TAG, new PrefabGUID[] { new PrefabGUID(-1208888966) } }, // Prefab 'PrefabGuid(-1208888966)': CHAR_Undead_ZealousCultist_VBlood
    { OAKSONG_TAG, new PrefabGUID[] { new PrefabGUID(109969450) } } // Prefab 'PrefabGuid(109969450)': CHAR_Villager_CursedWanderer_VBlood
  };

  public static bool IsBossSoftlocked(Entity playerCharacter, PrefabGUID vBloodPrefabGuid)
  {
    // if they have the admin tag, they are not softlocked
    if (HasPlayerTag(playerCharacter.GetSteamId(), ADMIN_TAG))
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
      if (HasPlayerTag(steamId, tag))
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
