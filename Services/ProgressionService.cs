
using Stunlock.Core;
using Unity.Entities;
using ProjectM;
using SoVUtilities.Resources;
using ProjectM.Network;
using SoVUtilities.Services.Bosses;

namespace SoVUtilities.Services;

public static class ProgressionService
{
  static EntityManager EntityManager => Core.EntityManager;

  public static readonly BossProgressionData CHRISTINA_PROGRESS = new BossProgressionData
  {
    prefabGUID = PrefabGUIDs.CHAR_Militia_Nun_VBlood,
    unlockedVBloodsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_Militia_Nun_VBlood
    },
    unlockedRecipesPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.Recipe_Ingredient_WoolThread,
      PrefabGUIDs.Recipe_Bag_New_T03,
    },
    unlockedBlueprintsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.TM_Castle_PillarDecor_Gothic_Candle01_Dyable,
      PrefabGUIDs.TM_Castle_PillarDecor_Gothic_Candle02_Dyable,
      PrefabGUIDs.TM_Castle_PillarDecor_Gothic_Candle03_Dyable,
      PrefabGUIDs.TM_Castle_PillarDecor_Gothic_Candle04_Gold_Dyable,
      PrefabGUIDs.TM_Castle_PillarDecor_Gothic_Candle04_Silver_Dyable,
      PrefabGUIDs.TM_Castle_PillarDecor_Gothic_Candle04_Wood_Dyable,
    },
    unlockedProgressionsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_Militia_Nun_VBlood,
      PrefabGUIDs.Tech_Collection_VBlood_T05_HolyNun,
      PrefabGUIDs.Tech_Collection_Candles_T02,
    }
  };

  public static readonly BossProgressionData BEN_PROGRESS = new BossProgressionData
  {
    prefabGUID = PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood,
    unlockedVBloodsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood
    },
    unlockedRecipesPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.Recipe_Ingredient_PristineLeather,
      PrefabGUIDs.Recipe_Cloak_ShroudOfTheForest,
    },
    unlockedBlueprintsPrefabGUIDs = new List<PrefabGUID>
    {
    },
    unlockedProgressionsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood,
      PrefabGUIDs.Tech_Collection_VBlood_T06_CursedWanderer
    }
  };

  public static readonly BossProgressionData FOULROT_PROGRESS = new BossProgressionData
  {
    prefabGUID = PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood,
    unlockedVBloodsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood
    },
    unlockedRecipesPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.Recipe_Ingredient_Spectraldust,
      PrefabGUIDs.Recipe_UnitSpawn_Banshee,
    },
    unlockedBlueprintsPrefabGUIDs = new List<PrefabGUID>
    {
    },
    unlockedProgressionsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood,
      PrefabGUIDs.Tech_Collection_VBlood_T07_ZealousCultist
    }
  };

  public static readonly BossProgressionData AZARIEL_PROGRESS = new BossProgressionData
  {
    prefabGUID = PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood,
    unlockedVBloodsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood
    },
    unlockedRecipesPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.Recipe_Ingredient_GoldBar,
    },
    unlockedBlueprintsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.TM_Castle_FloorDecor_FancyCarpet01_Dyable,
      PrefabGUIDs.TM_Castle_FloorDecor_FancyCarpet02_Dyable,
    },
    unlockedProgressionsPrefabGUIDs = new List<PrefabGUID>
    {
      PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood,
      PrefabGUIDs.Tech_Collection_VBlood_T07_CardinalPriest,
      PrefabGUIDs.Tech_Collection_Carpet_Ostenstatious,
    }
  };

  // we want a dictionary of string to BossProgressionData
  public static readonly Dictionary<string, BossProgressionData> BossProgressions = new Dictionary<string, BossProgressionData>
  {
    { BossConstants.CHRISTINA_KEY, CHRISTINA_PROGRESS },
    { BossConstants.BEN_KEY, BEN_PROGRESS },
    { BossConstants.FOULROT_KEY, FOULROT_PROGRESS },
    { BossConstants.AZARIEL_KEY, AZARIEL_PROGRESS },
  };

  // we want a dictionary of string to PrefabGUID
  public static readonly Dictionary<string, PrefabGUID> BossProgressionPrefabGUIDs = new Dictionary<string, PrefabGUID>
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

  public static bool RemoveVBloodUnlock(Entity userEntity, string vBloodKey)
  {
    if (BossProgressions.ContainsKey(vBloodKey))
    {
      ProgressionMapper progressionMapper = EntityManager.GetComponentData<ProgressionMapper>(userEntity);
      Entity progressionEntity = progressionMapper.ProgressionEntity._Entity;

      BossProgressionData progressionData = BossProgressions[vBloodKey];

      removeUnlockedVBloodData(progressionEntity, progressionData);
      removeUnlockedRecipe(progressionEntity, progressionData);
      removeUnlockedBlueprint(progressionEntity, progressionData);
      removeUnlockedProgression(progressionEntity, progressionData);

      return true;
    }

    return false;
  }

  private static bool removeUnlockedVBloodData(Entity progressionEntity, BossProgressionData progressionData)
  {
    var vBloodData = EntityManager.GetBuffer<UnlockedVBlood>(progressionEntity);

    bool removed = false;
    // Iterate backwards to safely remove items while looping
    for (int i = vBloodData.Length - 1; i >= 0; i--)
    {
      var vBlood = vBloodData[i];
      if (vBlood.VBlood.Equals(progressionData.prefabGUID))
      {
        vBloodData.RemoveAt(i);
        removed = true;
      }
    }
    return removed;
  }

  public static bool removeUnlockedRecipe(Entity progressionEntity, BossProgressionData progressionData)
  {
    var recipeData = EntityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity);

    List<PrefabGUID> recipeGuidsToRemove = progressionData.unlockedRecipesPrefabGUIDs;
    bool removed = false;
    // Iterate backwards to safely remove items while looping
    for (int i = recipeData.Length - 1; i >= 0; i--)
    {
      var recipe = recipeData[i];
      if (recipeGuidsToRemove.Contains(recipe.UnlockedRecipe))
      {
        recipeData.RemoveAt(i);
        removed = true;
      }
    }
    return removed;
  }

  public static bool removeUnlockedBlueprint(Entity progressionEntity, BossProgressionData progressionData)
  {
    var blueprintData = EntityManager.GetBuffer<UnlockedBlueprintElement>(progressionEntity);

    List<PrefabGUID> blueprintGuidsToRemove = progressionData.unlockedBlueprintsPrefabGUIDs;
    bool removed = false;
    // Iterate backwards to safely remove items while looping
    for (int i = blueprintData.Length - 1; i >= 0; i--)
    {
      var blueprint = blueprintData[i];
      if (blueprintGuidsToRemove.Contains(blueprint.UnlockedBlueprint))
      {
        blueprintData.RemoveAt(i);
        removed = true;
      }
    }
    return removed;
  }

  public static bool removeUnlockedProgression(Entity progressionEntity, BossProgressionData progressionData)
  {
    var progressionDataBuffer = EntityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity);

    List<PrefabGUID> progressionGuidsToRemove = progressionData.unlockedProgressionsPrefabGUIDs;
    bool removed = false;
    // Iterate backwards to safely remove items while looping
    for (int i = progressionDataBuffer.Length - 1; i >= 0; i--)
    {
      var progression = progressionDataBuffer[i];
      if (progressionGuidsToRemove.Contains(progression.UnlockedPrefab))
      {
        progressionDataBuffer.RemoveAt(i);
        removed = true;
      }
    }
    return removed;
  }

  public static string[] GetAvailableVBloodRemovals()
  {
    // we just return the keys of the BossProgressions dictionary
    return BossProgressions.Keys.ToArray();
  }

  public static string[] GetAvailableVBloodUnlocks()
  {
    // we just return the keys of the BossProgressions dictionary
    return BossProgressionPrefabGUIDs.Keys.ToArray();
  }

  public static void UnlockVBlood(Entity characterEntity, string vBloodKey)
  {
    DebugEventsSystem debugEventsSystem = Core.DebugEventsSystem;

    // vBloodKey should be lowercase
    string vBloodKeyLower = vBloodKey.ToLowerInvariant();
    PrefabGUID vBloodPrefabGUID = BossProgressionPrefabGUIDs[vBloodKeyLower];
    FromCharacter fromCharacter = new FromCharacter
    {
      User = characterEntity.GetUserEntity(),
      Character = characterEntity
    };
    UnlockVBlood unlockVBlood = new UnlockVBlood
    {
      VBlood = vBloodPrefabGUID
    };

    debugEventsSystem.UnlockVBloodEvent(debugEventsSystem, unlockVBlood, fromCharacter);
  }
}

public struct BossProgressionData
{
  public PrefabGUID prefabGUID;
  public List<PrefabGUID> unlockedVBloodsPrefabGUIDs;
  public List<PrefabGUID> unlockedRecipesPrefabGUIDs;
  public List<PrefabGUID> unlockedBlueprintsPrefabGUIDs;
  public List<PrefabGUID> unlockedProgressionsPrefabGUIDs;

  public BossProgressionData(
    PrefabGUID prefabGUID = default,
    List<PrefabGUID> unlockedVBloodsPrefabGUIDs = null,
    List<PrefabGUID> unlockedRecipesPrefabGUIDs = null,
    List<PrefabGUID> unlockedBlueprintsPrefabGUIDs = null,
    List<PrefabGUID> unlockedProgressionsPrefabGUIDs = null)
  {
    this.prefabGUID = prefabGUID;
    this.unlockedVBloodsPrefabGUIDs = unlockedVBloodsPrefabGUIDs ?? new List<PrefabGUID>();
    this.unlockedRecipesPrefabGUIDs = unlockedRecipesPrefabGUIDs ?? new List<PrefabGUID>();
    this.unlockedBlueprintsPrefabGUIDs = unlockedBlueprintsPrefabGUIDs ?? new List<PrefabGUID>();
    this.unlockedProgressionsPrefabGUIDs = unlockedProgressionsPrefabGUIDs ?? new List<PrefabGUID>();
  }
}