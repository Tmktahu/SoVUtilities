// using SoVUtilities.Resources;
// using SoVUtilities.Services;
// using HarmonyLib;
// using ProjectM;
// using Stunlock.Core;
// using Unity.Entities;
// using SoVUtilities.Services.Buffs;
// using ProjectM.Sequencer;
// using Unity.Collections;

// namespace SoVUtilities.Patches;

// [HarmonyPatch]
// internal static class SequenceSystemsPatch
// {
//   static EntityManager EntityManager => Core.EntityManager;

//   // static readonly EntityQuery _query = QueryService.BuffSpawnServerQuery;

//   [HarmonyPatch(typeof(EntitySequenceSystem_Spawn), nameof(EntitySequenceSystem_Spawn.OnUpdate))]
//   [HarmonyPrefix]
//   static void OnUpdatePrefix(EntitySequenceSystem_Spawn __instance)
//   {
//     if (!Core._initialized) return;

//     // Core.Log.LogInfo("[EntitySequenceSystem_Spawn] - OnUpdatePrefix triggered");

//     // NativeArray<Entity> entities = __instance.__query_313887672_0.ToEntityArray(Allocator.Temp);

//     // 
//     // using NativeAccessor<Entity> entities = _query.ToEntityArrayAccessor();
//     // using NativeAccessor<PrefabGUID> prefabGuids = _query.ToComponentDataArrayAccessor<PrefabGUID>();
//     // using NativeAccessor<Buff> buffs = _query.ToComponentDataArrayAccessor<Buff>();

//     // ComponentLookup<PlayerCharacter> playerCharacterLookup = __instance.GetComponentLookup<PlayerCharacter>(true);

//     // try
//     // {
//     //   for (int i = 0; i < entities.Length; i++)
//     //   {
//     //     // debug logging to see what we actually have here
//     //     // Entity entity = entities[i];
//     //     // PrefabGUID buffPrefabGUID = prefabGuids[i];
//     //     // Entity buffTarget = buffs[i].Target;

//     //     // bool isPlayerTarget = playerCharacterLookup.HasComponent(buffTarget);

//     //     // Core.Log.LogInfo($"[BuffSequenceSystem_Spawn] - Buff Applied: {buffPrefabGUID} to {buffTarget}");
//     //   }
//     // }
//     // catch (Exception e)
//     // {
//     //   Core.Log.LogWarning($"[BuffSequenceSystem_Spawn] - Exception: {e}");
//     // }
//     // finally
//     // {
//     //   entities.Dispose();
//     //   prefabGuids.Dispose();
//     //   buffs.Dispose();
//     // }
//   }
// }
