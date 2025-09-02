using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class ReplaceAbilityOnSlotSystemPatch
{
    static EntityManager EntityManager => Core.EntityManager;

    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), nameof(ReplaceAbilityOnSlotSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(ReplaceAbilityOnSlotSystem __instance)
    {
        if (!Core._initialized) return;

        NativeArray<Entity> entities = __instance.__query_1482480545_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity buffEntity in entities)
            {
                try
                {
                    PrefabGUID prefabGUID = EntityManager.GetComponentData<PrefabGUID>(buffEntity);
                    if (prefabGUID.IsEmpty()) continue;

                    // if (prefabGUID == PrefabGUIDs.EquipBuff_Weapon_Claws_Ability03_Unique01) continue;

                    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(prefabGUID);
                    if (string.IsNullOrEmpty(prefabName)) continue; // Skip if prefabName is invalid

                    if (!prefabName.Contains("unarmed", Il2CppSystem.StringComparison.OrdinalIgnoreCase) && !prefabName.Contains("weapon", Il2CppSystem.StringComparison.OrdinalIgnoreCase)) continue;

                    if (!buffEntity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists()) continue;
                    else if (entityOwner.Owner.TryGetPlayer(out Entity character))
                    {
                        AbilityService.ApplyAbilities(character, buffEntity);
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.LogError($"ReplaceAbilityOnSlotSystemPatch: Exception processing entity {buffEntity.Index}: {ex.Message}");
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}