
// using Stunlock.Core;
// using Unity.Entities;
// using ProjectM;

// namespace SoVUtilities.Services;

// public static class ProgressionService
// {
//   static EntityManager EntityManager => Core.EntityManager;

//   public static readonly BossProgressionData CHRISTINA_PROGRESS = new BossProgressionData
//   {
//     // prefabGUID = new PrefabGUID("christina_prefab_guid"),
//     // unlockedVBloodsPrefabGUIDs = new List<PrefabGUID>
//     // {
//     //   new PrefabGUID("christina_vblood_1"),
//     //   new PrefabGUID("christina_vblood_2"),
//     //   new PrefabGUID("christina_vblood_3")
//     // },
//     // unlockedRecipesPrefabGUIDs = new List<PrefabGUID>
//     // {
//     //   new PrefabGUID("christina_recipe_1"),
//     //   new PrefabGUID("christina_recipe_2"),
//     //   new PrefabGUID("christina_recipe_3")
//     // },
//     // unlockedBlueprintsPrefabGUIDs = new List<PrefabGUID>
//     // {
//     //   new PrefabGUID("christina_blueprint_1"),
//     //   new PrefabGUID("christina_blueprint_2"),
//     //   new PrefabGUID("christina_blueprint_3")
//     // },
//     // unlockedProgressionsPrefabGUIDs = new List<PrefabGUID>
//     // {
//     //   new PrefabGUID("christina_progression_1"),
//     //   new PrefabGUID("christina_progression_2"),
//     //   new PrefabGUID("christina_progression_3")
//     // }
//   };

//   // we want a dictionary of string to BossProgressionData
//   public static readonly Dictionary<string, BossProgressionData> BossProgressions = new Dictionary<string, BossProgressionData>
//   {
//     { "christina", CHRISTINA_PROGRESS }
//   };

//   public static bool removeVBloodUnlock(PlayerCharacter playerCharacter, string vBloodKey)
//   {
//     if (BossProgressions.ContainsKey(vBloodKey))
//     {
//       Entity userEntity = playerCharacter.UserEntity;
//       ProgressionMapper progressionMapper = EntityManager.GetComponentData<ProgressionMapper>(userEntity);
//       Entity progressionEntity = progressionMapper.ProgressionEntity._Entity;

//       BossProgressionData progressionData = BossProgressions[vBloodKey];

//       removeUnlockedVBloodData(progressionEntity, progressionData);
//       removeUnlockedRecipe(progressionEntity, progressionData);
//       removeUnlockedBlueprint(progressionEntity, progressionData);
//       removeUnlockedProgression(progressionEntity, progressionData);

//       return true;
//     }

//     return false;
//   }

//   private static bool removeUnlockedVBloodData(Entity progressionEntity, BossProgressionData progressionData)
//   {
//     var vBloodData = EntityManager.GetBuffer<UnlockedVBlood>(progressionEntity);
//     bool removed = false;
//     // Iterate backwards to safely remove items while looping
//     for (int i = vBloodData.Length - 1; i >= 0; i--)
//     {
//       var vBlood = vBloodData[i];
//       if (vBlood.VBlood.Equals(progressionData.prefabGUID))
//       {
//         vBloodData.RemoveAt(i);
//         removed = true;
//       }
//     }
//     return removed;
//   }

//   public static bool removeUnlockedRecipe(Entity progressionEntity, BossProgressionData progressionData)
//   {
//     var recipeData = EntityManager.GetBuffer<UnlockedRecipeElement>(progressionEntity);
//     bool removed = false;
//     // Iterate backwards to safely remove items while looping
//     for (int i = recipeData.Length - 1; i >= 0; i--)
//     {
//       var recipe = recipeData[i];
//       if (recipe.UnlockedRecipe.Equals(progressionData.prefabGUID))
//       {
//         recipeData.RemoveAt(i);
//         removed = true;
//       }
//     }
//     return removed;
//   }

//   public static bool removeUnlockedBlueprint(Entity progressionEntity, BossProgressionData progressionData)
//   {
//     var blueprintData = EntityManager.GetBuffer<UnlockedBlueprintElement>(progressionEntity);
//     bool removed = false;
//     // Iterate backwards to safely remove items while looping
//     for (int i = blueprintData.Length - 1; i >= 0; i--)
//     {
//       var blueprint = blueprintData[i];
//       if (blueprint.UnlockedBlueprint.Equals(progressionData.prefabGUID))
//       {
//         blueprintData.RemoveAt(i);
//         removed = true;
//       }
//     }
//     return removed;
//   }

//   public static bool removeUnlockedProgression(Entity progressionEntity, BossProgressionData progressionData)
//   {
//     var progressionDataBuffer = EntityManager.GetBuffer<UnlockedProgressionElement>(progressionEntity);
//     bool removed = false;
//     // Iterate backwards to safely remove items while looping
//     for (int i = progressionDataBuffer.Length - 1; i >= 0; i--)
//     {
//       var progression = progressionDataBuffer[i];
//       if (progression.UnlockedPrefab.Equals(progressionData.prefabGUID))
//       {
//         progressionDataBuffer.RemoveAt(i);
//         removed = true;
//       }
//     }
//     return removed;
//   }

//   public static string[] getAvailableVBloodRemovals()
//   {
//     // we just return the keys of the BossProgressions dictionary
//     return BossProgressions.Keys.ToArray();
//   }
// }

// public struct BossProgressionData
// {
//   public PrefabGUID prefabGUID;
//   public List<PrefabGUID> unlockedVBloodsPrefabGUIDs;
//   public List<PrefabGUID> unlockedRecipesPrefabGUIDs;
//   public List<PrefabGUID> unlockedBlueprintsPrefabGUIDs;
//   public List<PrefabGUID> unlockedProgressionsPrefabGUIDs;

//   public BossProgressionData(
//     PrefabGUID prefabGUID = default,
//     List<PrefabGUID> unlockedVBloodsPrefabGUIDs = null,
//     List<PrefabGUID> unlockedRecipesPrefabGUIDs = null,
//     List<PrefabGUID> unlockedBlueprintsPrefabGUIDs = null,
//     List<PrefabGUID> unlockedProgressionsPrefabGUIDs = null)
//   {
//     this.prefabGUID = prefabGUID;
//     this.unlockedVBloodsPrefabGUIDs = unlockedVBloodsPrefabGUIDs ?? new List<PrefabGUID>();
//     this.unlockedRecipesPrefabGUIDs = unlockedRecipesPrefabGUIDs ?? new List<PrefabGUID>();
//     this.unlockedBlueprintsPrefabGUIDs = unlockedBlueprintsPrefabGUIDs ?? new List<PrefabGUID>();
//     this.unlockedProgressionsPrefabGUIDs = unlockedProgressionsPrefabGUIDs ?? new List<PrefabGUID>();
//   }
// }