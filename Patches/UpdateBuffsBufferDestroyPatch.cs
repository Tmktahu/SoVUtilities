using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using SoVUtilities.Resources;
using SoVUtilities.Services.Buffs;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class UpdateBuffsBufferDestroyPatch
{
    static EntityManager EntityManager => Core.EntityManager;
    static readonly PrefabGUID razerHood = PrefabGUIDs.Item_Headgear_RazerHood;
    static readonly PrefabGUID HeadgearBaseBuff = PrefabGUIDs.EquipBuff_Headgear_Base;

    static readonly EntityQuery _query = QueryService.UpdateBuffsBufferDestroyQuery;

    [HarmonyPatch(typeof(UpdateBuffsBuffer_Destroy), nameof(UpdateBuffsBuffer_Destroy.OnUpdate))]
    [HarmonyPostfix]
    static void OnUpdatePostfix(UpdateBuffsBuffer_Destroy __instance)
    {
        if (!Core._initialized) return;

        NativeArray<Entity> entities = _query.ToEntityArray(Allocator.Temp);
        NativeArray<PrefabGUID> prefabGuids = _query.ToComponentDataArray<PrefabGUID>(Allocator.Temp);
        NativeArray<Buff> buffs = _query.ToComponentDataArray<Buff>(Allocator.Temp);

        ComponentLookup<PlayerCharacter> playerCharacterLookup = __instance.GetComponentLookup<PlayerCharacter>(true);

        try
        {
            for (int i = 0; i < entities.Length; i++)
            {
                Entity buffTarget = buffs[i].Target;
                PrefabGUID buffPrefabGuid = prefabGuids[i];

                // for werewolf shapeshift buff, we need to remove the associated human buff when the werewolf buff is removed
                if (buffPrefabGuid.Equals(PrefabGUIDs.Buff_General_Shapeshift_Werewolf_Standard))
                {
                    // TODO check for werewolf tag here?
                    // TransformationService.RefreshWerewolfBuffs(buffTarget);
                    WerewolfStatsBuff.ReapplyCustomBuff(buffTarget).Start();
                }

                // for razer hood, we need to remove the hide nameplate buff when the headgear buff is removed
                if (buffPrefabGuid.Equals(HeadgearBaseBuff) && buffTarget.Exists())
                {
                    bool isPlayerTarget = playerCharacterLookup.HasComponent(buffTarget);
                    if (isPlayerTarget)
                    {
                        if (EntityManager.HasComponent<Equipment>(buffTarget))
                        {
                            Equipment equipment = EntityManager.GetComponentData<Equipment>(buffTarget);
                            if (!equipment.IsEquipped(razerHood, out var _))
                            {
                                BuffService.RemoveHideNameplateBuff(buffTarget);
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            entities.Dispose();
            prefabGuids.Dispose();
            buffs.Dispose();
        }
    }
}
