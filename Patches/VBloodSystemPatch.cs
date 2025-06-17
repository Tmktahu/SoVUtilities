using SOVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using static SOVUtilities.Services.SoftlockService;

namespace SOVUtilities.Patches;

[HarmonyPatch]
internal static class VBloodSystemPatch
{
    static EntityManager EntityManager => Core.EntityManager;
    static SystemService SystemService => Core.SystemService;
    static PrefabCollectionSystem PrefabCollectionSystem => SystemService.PrefabCollectionSystem;

    [HarmonyPatch(typeof(VBloodSystem), nameof(VBloodSystem.OnUpdate))]
    [HarmonyPrefix]
    static bool OnUpdatePrefix(VBloodSystem __instance)
    {
        if (!Core._initialized) return true; // Allow original method to run if not initialized

        NativeList<VBloodConsumed> events = __instance.EventList;

        try
        {
            foreach (VBloodConsumed vBloodConsumed in events)
            {
                Entity playerCharacter = vBloodConsumed.Target;
                Entity vBlood = PrefabCollectionSystem._PrefabGuidToEntityMap[vBloodConsumed.Source];
                PrefabGUID vBloodGuid = EntityManager.GetComponentData<PrefabGUID>(vBlood);

                if (IsBossSoftlocked(playerCharacter, vBloodGuid))
                {
                    events.Clear(); // Clear the events list to prevent processing the same events again
                    return false; // Skip original method execution
                }
            }
        }
        catch (Exception e)
        {
            Core.Log.LogWarning($"Error in VBloodSystemPatch Prefix: {e}");
        }

        return true; // Continue with the original method execution
    }
}
