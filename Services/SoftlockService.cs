
using SoVUtilities.Resources;
using Stunlock.Core;
using Unity.Entities;
using SoVUtilities.Services.Bosses;
using static SoVUtilities.Services.TagService;

namespace SoVUtilities.Services;

public static class SoftlockService
{
  // Dictionary of available bosses that can be softlocked - maps boss name to PrefabGUID
  public static Dictionary<string, PrefabGUID> AvailableBosses { get; } = new Dictionary<string, PrefabGUID>
  {
    // act 1 bosses
    { BossConstants.ALPHA_WOLF_KEY, PrefabGUIDs.CHAR_Forest_Wolf_VBlood },
    { BossConstants.KEELY_KEY, PrefabGUIDs.CHAR_Bandit_Frostarrow_VBlood },
    { BossConstants.ERROL_KEY, PrefabGUIDs.CHAR_Bandit_StoneBreaker_VBlood },
    { BossConstants.RUFUS_KEY, PrefabGUIDs.CHAR_Bandit_Foreman_VBlood },
    { BossConstants.GRAYSON_KEY, PrefabGUIDs.CHAR_Bandit_Stalker_VBlood },
    { BossConstants.GORESWINE_KEY, PrefabGUIDs.CHAR_Undead_BishopOfDeath_VBlood },
    { BossConstants.LIDIA_KEY, PrefabGUIDs.CHAR_Bandit_Chaosarrow_VBlood },
    { BossConstants.CLIVE_KEY, PrefabGUIDs.CHAR_Bandit_Bomber_VBlood },
    { BossConstants.NIBBLES_KEY, PrefabGUIDs.CHAR_Vermin_DireRat_VBlood },
    { BossConstants.FINN_KEY, PrefabGUIDs.CHAR_Bandit_Fisherman_VBlood },
    { BossConstants.POLORA_KEY, PrefabGUIDs.CHAR_Poloma_VBlood },
    { BossConstants.KODIA_KEY, PrefabGUIDs.CHAR_Forest_Bear_Dire_Vblood },
    { BossConstants.NOCHOLAUS_KEY, PrefabGUIDs.CHAR_Undead_Priest_VBlood },
    { BossConstants.QUINCEY_KEY, PrefabGUIDs.CHAR_Bandit_Tourok_VBlood },
    // act 2 bosses
    { BossConstants.BEATRICE_KEY, PrefabGUIDs.CHAR_Villager_Tailor_VBlood },
    { BossConstants.VINCENT_KEY, PrefabGUIDs.CHAR_Militia_Guard_VBlood },
    { BossConstants.CHRISTINA_KEY, PrefabGUIDs.CHAR_Militia_Nun_VBlood },
    { BossConstants.TRISTAN_KEY, PrefabGUIDs.CHAR_VHunter_Leader_VBlood },
    { BossConstants.ERWIN_KEY, PrefabGUIDs.CHAR_Militia_Fabian_VBlood },
    { BossConstants.KRIIG_KEY, PrefabGUIDs.CHAR_Undead_Leader_Vblood },
    { BossConstants.LEANDRA_KEY, PrefabGUIDs.CHAR_Undead_BishopOfShadows_VBlood },
    { BossConstants.MAJA_KEY, PrefabGUIDs.CHAR_Militia_Scribe_VBlood },
    { BossConstants.BANE_KEY, PrefabGUIDs.CHAR_Undead_Infiltrator_VBlood },
    { BossConstants.GRETHEL_KEY, PrefabGUIDs.CHAR_Militia_Glassblower_VBlood },
    { BossConstants.MEREDITH_KEY, PrefabGUIDs.CHAR_Militia_Longbowman_LightArrow_Vblood },
    { BossConstants.TERAH_KEY, PrefabGUIDs.CHAR_Geomancer_Human_VBlood },
    { BossConstants.FROSTMAW_KEY, PrefabGUIDs.CHAR_Wendigo_VBlood },
    { BossConstants.ELENA_KEY, PrefabGUIDs.CHAR_Vampire_IceRanger_VBlood },
    { BossConstants.GAIUS_KEY, PrefabGUIDs.CHAR_Undead_ArenaChampion_VBlood },
    { BossConstants.CASSIUS_KEY, PrefabGUIDs.CHAR_Vampire_HighLord_VBlood },
    { BossConstants.JADE_KEY, PrefabGUIDs.CHAR_VHunter_Jade_VBlood },
    { BossConstants.RAZIEL_KEY, PrefabGUIDs.CHAR_Militia_BishopOfDunley_VBlood },
    { BossConstants.OCTAVIAN_KEY, PrefabGUIDs.CHAR_Militia_Leader_VBlood },
    // act 3 bosses
    { BossConstants.ZIVA_KEY, PrefabGUIDs.CHAR_Gloomrot_Iva_VBlood },
    { BossConstants.DOMINA_KEY, PrefabGUIDs.CHAR_Gloomrot_Voltage_VBlood },
    { BossConstants.ANGRAM_KEY, PrefabGUIDs.CHAR_Gloomrot_Purifier_VBlood },
    { BossConstants.UNGORA_KEY, PrefabGUIDs.CHAR_Spider_Queen_VBlood },
    { BossConstants.BEN_KEY, PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood },
    { BossConstants.FOULROT_KEY, PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood },
    { BossConstants.ALBERT_KEY, PrefabGUIDs.CHAR_Cursed_ToadKing_VBlood },
    { BossConstants.WILFRED_KEY, PrefabGUIDs.CHAR_WerewolfChieftain_Human }, // MIGHT BE INCORRECT
    { BossConstants.CYRIL_KEY, PrefabGUIDs.CHAR_Undead_CursedSmith_VBlood },
    // act 4 bosses
    { BossConstants.MAGNUS_KEY, PrefabGUIDs.CHAR_ChurchOfLight_Overseer_VBlood },
    { BossConstants.BARON_KEY, PrefabGUIDs.CHAR_ChurchOfLight_Sommelier_VBlood },
    { BossConstants.MORIAN_KEY, PrefabGUIDs.CHAR_Harpy_Matriarch_VBlood },
    { BossConstants.MAIRWYN_KEY, PrefabGUIDs.CHAR_ArchMage_VBlood },
    { BossConstants.HENRY_KEY, PrefabGUIDs.CHAR_Gloomrot_TheProfessor_VBlood },
    { BossConstants.JAKIRA_KEY, PrefabGUIDs.CHAR_Blackfang_Livith_VBlood },
    { BossConstants.STAVROS_KEY, PrefabGUIDs.CHAR_Blackfang_CarverBoss_VBlood },
    { BossConstants.LUCILE_KEY, PrefabGUIDs.CHAR_Blackfang_Lucie_VBlood },
    { BossConstants.MATKA_KEY, PrefabGUIDs.CHAR_Cursed_Witch_VBlood },
    { BossConstants.TERRORCLAW_KEY, PrefabGUIDs.CHAR_Winter_Yeti_VBlood },
    { BossConstants.AZARIEL_KEY, PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood },
    { BossConstants.VOLTATIA_KEY, PrefabGUIDs.CHAR_Gloomrot_RailgunSergeant_VBlood },
    { BossConstants.SIMON_KEY, PrefabGUIDs.CHAR_VHunter_CastleMan },
    { BossConstants.DANTOS_KEY, PrefabGUIDs.CHAR_Blackfang_Valyr_VBlood },
    { BossConstants.STYX_KEY, PrefabGUIDs.CHAR_BatVampire_VBlood },
    { BossConstants.GORECRUSHER_KEY, PrefabGUIDs.CHAR_Cursed_MountainBeast_VBlood },
    { BossConstants.VALENCIA_KEY, PrefabGUIDs.CHAR_Vampire_BloodKnight_VBlood },
    { BossConstants.SOLARUS_KEY, PrefabGUIDs.CHAR_ChurchOfLight_Paladin_VBlood },
    { BossConstants.TALZUR_KEY, PrefabGUIDs.CHAR_Manticore_VBlood },
    { BossConstants.ADAM_KEY, PrefabGUIDs.CHAR_Gloomrot_Monster_VBlood },
    { BossConstants.MEGARA_KEY, PrefabGUIDs.CHAR_Blackfang_Morgana_VBlood },
    { BossConstants.DRACULA_KEY, PrefabGUIDs.CHAR_Vampire_Dracula_VBlood }
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
