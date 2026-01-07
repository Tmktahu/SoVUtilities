using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using User = ProjectM.Network.User;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class GearLevelPatches // WeaponLevelSystem_Spawn, WeaponLevelSystem_Destroy, ArmorLevelSystem_Spawn, ArmorLevelSystem_Destroy
{
    static SystemService SystemService => Core.SystemService;
    static EntityManager EntityManager => Core.EntityManager;
    static readonly Dictionary<ulong, int> playerWeaponLevelCache = new Dictionary<ulong, int>();

    [HarmonyPatch(typeof(WeaponLevelSystem_Spawn), nameof(WeaponLevelSystem_Spawn.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(WeaponLevelSystem_Spawn __instance)
    {
        if (!Core._initialized) return;

        // These are EquipBuffs
        NativeArray<Entity> entities = __instance.__query_1111682356_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity entity in entities)
            {
                if (!entity.Has<WeaponLevel>() || !entity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists()) continue;

                else if (entityOwner.Owner.TryGetPlayer(out Entity playerCharacter))
                {
                    ulong steamId = playerCharacter.GetSteamId();
                    PrefabGUID equipBuffPrefabGuid = EntityManager.GetComponentData<PrefabGUID>(entity);

                    if (equipBuffPrefabGuid.Equals(PrefabGUIDs.EquipBuff_Weapon_Unarmed_Start01))
                    {
                        if (playerWeaponLevelCache.TryGetValue(steamId, out var unarmedLevel))
                        {
                            entity.With((ref WeaponLevel weaponLevel) => weaponLevel.Level = unarmedLevel);
                        }
                    }
                    else
                    {
                        // on any other weapon equip, cache the current unarmed level
                        if (entity.Has<WeaponLevel>())
                        {
                            WeaponLevel weaponLevel = EntityManager.GetComponentData<WeaponLevel>(entity);
                            playerWeaponLevelCache[steamId] = (int)weaponLevel.Level;
                        }
                    }
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}
