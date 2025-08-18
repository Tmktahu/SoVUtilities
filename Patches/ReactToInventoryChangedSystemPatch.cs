using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class ReactToInventoryChangedSystemPatch
{
    static EntityManager EntityManager => Core.EntityManager;
    static readonly PrefabGUID razerHood = PrefabGUIDs.Item_Headgear_RazerHood;

    [HarmonyPatch(typeof(ReactToInventoryChangedSystem), nameof(ReactToInventoryChangedSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(ReactToInventoryChangedSystem __instance)
    {
        if (!Core._initialized) return;

        NativeArray<InventoryChangedEvent> inventoryChangedEvents = __instance.EntityQueries[0].ToComponentDataArray<InventoryChangedEvent>(Allocator.Temp);
        try
        {
            foreach (InventoryChangedEvent inventoryChangedEvent in inventoryChangedEvents)
            {
                Entity itemEntity = inventoryChangedEvent.ItemEntity;
                if (itemEntity.Exists())
                {
                    if (EntityManager.HasComponent<Equippable>(itemEntity))
                    {
                        Equippable equippable = itemEntity.Read<Equippable>();
                        NetworkedEntity networkedEntity = equippable.EquipTarget;
                        Entity equipTargetEntity = networkedEntity._Entity;
                        if (equipTargetEntity.Exists())
                        {
                            if (equipTargetEntity.IsPlayer())
                            {
                                // Check if the player has the razor hood equipped
                                Equipment equipment = equipTargetEntity.Read<Equipment>();
                                if (equipment.IsEquipped(razerHood, out _))
                                {
                                    BuffService.AddHideNameplateBuff(equipTargetEntity);
                                }
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            inventoryChangedEvents.Dispose();
        }
    }
}
