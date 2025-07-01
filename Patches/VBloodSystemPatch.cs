using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using static SoVUtilities.Services.SoftlockService;

namespace SoVUtilities.Patches;

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

        // If no events, just continue with original method
        if (events.Length == 0) return true;

        try
        {
            // Create a new list to track which events to remove
            List<int> eventsToRemove = new List<int>();

            // Check each event and mark softlocked ones for removal
            for (int i = 0; i < events.Length; i++)
            {
                VBloodConsumed vBloodConsumed = events[i];
                Entity playerCharacter = vBloodConsumed.Target;
                Entity vBlood = PrefabCollectionSystem._PrefabGuidToEntityMap[vBloodConsumed.Source];
                PrefabGUID vBloodGuid = EntityManager.GetComponentData<PrefabGUID>(vBlood);

                if (IsBossSoftlocked(playerCharacter, vBloodGuid))
                {
                    eventsToRemove.Add(i);
                    // Core.Log.LogInfo($"Prevented softlocked VBlood consumption for player entity {playerCharacter}");
                }
            }

            // Remove marked events in reverse order to avoid index shifting issues
            eventsToRemove.Sort();
            for (int i = eventsToRemove.Count - 1; i >= 0; i--)
            {
                int indexToRemove = eventsToRemove[i];
                events.RemoveAt(indexToRemove);
            }

            // Continue with original method if there are still events to process
            return events.Length > 0;
        }
        catch (Exception e)
        {
            Core.Log.LogWarning($"Error in VBloodSystemPatch Prefix: {e}");
            return true; // On error, continue with original method for safety
        }
    }
}
