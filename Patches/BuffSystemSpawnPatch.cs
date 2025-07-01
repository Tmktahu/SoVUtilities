using SoVUtilities.Resources;
using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class BuffSystemSpawnPatch
{
    static EntityManager EntityManager => Core.EntityManager;
    static readonly PrefabGUID InCombatBuff = PrefabGUIDs.Buff_InCombat;
    static readonly PrefabGUID CombatStanceBuff = PrefabGUIDs.Buff_CombatStance;
    static readonly PrefabGUID ShapeshiftNormalFormBuff = PrefabGUIDs.AB_Shapeshift_NormalForm_Buff;
    static readonly PrefabGUID WaygateSpawnBuff = PrefabGUIDs.AB_Interact_WaypointSpawn_Travel;
    static readonly PrefabGUID WoodenCoffinSpawnBuff = PrefabGUIDs.AB_Interact_WoodenCoffinSpawn_Travel;
    static readonly PrefabGUID StoneCoffinSpawnBuff = PrefabGUIDs.AB_Interact_StoneCoffinSpawn_Travel;
    static readonly PrefabGUID TombCoffinSpawnBuff = PrefabGUIDs.AB_Interact_TombCoffinSpawn_Travel;

    static readonly EntityQuery _query = QueryService.BuffSpawnServerQuery;

    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(BuffSystem_Spawn_Server __instance)
    {
        if (!Core._initialized) return;

        using NativeAccessor<Entity> entities = _query.ToEntityArrayAccessor();
        using NativeAccessor<PrefabGUID> prefabGuids = _query.ToComponentDataArrayAccessor<PrefabGUID>();
        using NativeAccessor<Buff> buffs = _query.ToComponentDataArrayAccessor<Buff>();

        ComponentLookup<PlayerCharacter> playerCharacterLookup = __instance.GetComponentLookup<PlayerCharacter>(true);

        try
        {
            for (int i = 0; i < entities.Length; i++)
            {
                // Entity buffEntity = entities[i];
                Entity buffTarget = buffs[i].Target;
                PrefabGUID buffPrefabGUID = prefabGuids[i];

                bool isPlayerTarget = playerCharacterLookup.HasComponent(buffTarget);

                if (isPlayerTarget)
                {
                    // Core.Log.LogInfo($"[BuffSystem_Spawn_Server] - BuffPrefabGuid: {buffPrefabGUID}");

                    if (BuffService.HasHideNameplateBuff(buffTarget))
                    {
                        if (buffPrefabGUID.Equals(InCombatBuff) || buffPrefabGUID.Equals(CombatStanceBuff))
                        {
                            // Handle InCombatBuff logic
                            Entity userEntity = buffTarget.GetUserEntity();
                            Shapeshift shapeshift = EntityManager.GetComponentData<Shapeshift>(userEntity);
                            bool isShapeshifted = shapeshift.IsShapeshifted;
                            // Core.Log.LogInfo($"[BuffSystem_Spawn_Server] - IsShapeshifted: {isShapeshifted} | UserEntity: {userEntity} | BuffTarget: {buffTarget}");

                            if (isShapeshifted)
                            {
                                // If this says yes, that means that are in combat in a shapeshifted form
                                // we do NOT want to remove the nameplate thing because they are still in a form
                            }
                            else
                            {
                                // If this says no, that means that are in combat in a normal form
                                // We need to remove the nameplate buff
                                // Core.Log.LogInfo($"[BuffSystem_Spawn_Server] - Removing nameplate buff from shapeshifted form for user {userEntity}");
                                BuffService.RemoveHideNameplateBuff(buffTarget);
                            }
                        }

                        if (buffPrefabGUID.Equals(ShapeshiftNormalFormBuff))
                        {
                            BuffService.RemoveHideNameplateBuff(buffTarget);
                        }
                    }

                    if (buffPrefabGUID.Equals(WaygateSpawnBuff) || buffPrefabGUID.Equals(WoodenCoffinSpawnBuff) || buffPrefabGUID.Equals(StoneCoffinSpawnBuff) || buffPrefabGUID.Equals(TombCoffinSpawnBuff))
                    {
                        // we want to refresh the buffs for the player character
                        BuffService.RefreshPlayerBuffs(buffTarget).Start();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Core.Log.LogWarning($"[BuffSystem_Spawn_Server] - Exception: {e}");
        }
        finally
        {
            entities.Dispose();
            prefabGuids.Dispose();
            buffs.Dispose();
        }
    }
}
