
using Stunlock.Core;
using Unity.Entities;
using ProjectM;
using SoVUtilities.Resources;
using ProjectM.Network;

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

  public static string CHRISTINA_KEY = "christina";
  public static string BEN_KEY = "ben";
  public static string FOULROT_KEY = "foulrot";
  public static string AZARIEL_KEY = "azariel";
  public static string DRACULA_KEY = "dracula";

  // we want a dictionary of string to BossProgressionData
  public static readonly Dictionary<string, BossProgressionData> BossProgressions = new Dictionary<string, BossProgressionData>
  {
    { CHRISTINA_KEY, CHRISTINA_PROGRESS },
    { BEN_KEY, BEN_PROGRESS },
    { FOULROT_KEY, FOULROT_PROGRESS },
    { AZARIEL_KEY, AZARIEL_PROGRESS },
  };

  // we want a dictionary of string to PrefabGUID
  public static readonly Dictionary<string, PrefabGUID> BossProgressionPrefabGUIDs = new Dictionary<string, PrefabGUID>
  {
    { CHRISTINA_KEY, PrefabGUIDs.CHAR_Militia_Nun_VBlood },
    { BEN_KEY, PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood },
    { FOULROT_KEY, PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood },
    { AZARIEL_KEY, PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood },
    { DRACULA_KEY, PrefabGUIDs.CHAR_Vampire_Dracula_VBlood }
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