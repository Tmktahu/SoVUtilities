using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using SoVUtilities.Resources;
using SoVUtilities.Services.Buffs;
using ProjectM.Network;
using System.Diagnostics;

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
                Entity entity = entities[i];





                Entity buffTarget = buffs[i].Target;
                PrefabGUID buffPrefabGuid = prefabGuids[i];

                bool isPlayerTarget = playerCharacterLookup.HasComponent(buffTarget);
                if (isPlayerTarget && (buffPrefabGuid.Equals(PrefabGUIDs.AB_Shapeshift_Wolf_Buff) || buffPrefabGuid.Equals(PrefabGUIDs.AB_Shapeshift_Bear_Buff)))
                {
                    // Core.Log.LogInfo($"[UpdateBuffsBuffer_Destroy] - Wolf Or Bear Buff Removed: {buffPrefabGuid} from {buffTarget}");
                    Entity userEntity = buffTarget.GetUserEntity();

                    var componentTypes = EntityManager.GetComponentTypes(entity);
                    // Core.Log.LogInfo($"[UpdateBuffsBuffer_Destroy] - Components on Entity {entity}:");
                    // foreach (var componentType in componentTypes)
                    // {
                    //     Core.Log.LogInfo($"- {componentType.GetManagedType().Name}");
                    // }

                    // if (EntityManager.TryGetComponentData<DestroyOnGameplayEvent>(entity, out var destroyOnGameplayEvent))
                    // {
                    //     DestroyOnGameplayEventWho who = destroyOnGameplayEvent.Who;
                    //     DestroyOnGameplayEventType type = destroyOnGameplayEvent.Type;
                    //     DestroyReason reason = destroyOnGameplayEvent.DestroyReason;
                    //     bool setTranslationToEventTranslation = destroyOnGameplayEvent.SetTranslationToEventTranslation;
                    //     Core.Log.LogInfo($"- DestroyOnGameplayEvent Who: {who}, Type: {type}, Reason: {reason}, SetTranslationToEventTranslation: {setTranslationToEventTranslation}");
                    // }

                    // if (EntityManager.TryGetComponentData<DestroyData>(entity, out var destroyData))
                    // {
                    //     Core.Log.LogInfo($"- DestroyData Reason: {destroyData.DestroyReason}");
                    // }

                    // if (EntityManager.TryGetComponentData<RemoveBuffOnGameplayEvent>(entity, out var removeBuffOnGameplayEvent))
                    // {
                    //     RemoveBuffTarget target = removeBuffOnGameplayEvent.BuffTarget;
                    //     Core.Log.LogInfo($"- RemoveBuffOnGameplayEvent Target: {target}");
                    // }

                    // if (EntityManager.TryGetBuffer<RemoveBuffOnGameplayEventEntry>(entity, out var removeBuffBuffer))
                    // {
                    //     Core.Log.LogInfo($"- RemoveBuffOnGameplayEvent Entries:");
                    //     foreach (var entry in removeBuffBuffer)
                    //     {
                    //         int eventIndex = entry.EventIndex;
                    //         PrefabIdentifier buff = entry.Buff;
                    //         BuffCategoryFlag buffCategoryFlag = entry.BuffCategoryFlag;
                    //         bool includeSelf = entry.IncludeSelf;
                    //         Core.Log.LogInfo($"  - EventIndex: {eventIndex}, Buff: {buff}, BuffCategoryFlag: {buffCategoryFlag}, IncludeSelf: {includeSelf}");
                    //     }
                    // }

                    // if (EntityManager.TryGetComponentData<BuffModificationFlagData>(entity, out var buffModificationFlagData))
                    // {
                    //     Core.Log.LogInfo($"- BuffModificationFlagData ModificationTypes: {buffModificationFlagData.ModificationTypes}");
                    //     var modTypes = (ProjectM.BuffModificationTypes)buffModificationFlagData.ModificationTypes;
                    //     Core.Log.LogInfo($"ModificationTypes: {modTypes} ({buffModificationFlagData.ModificationTypes})");
                    // }

                    // var stacktrack = new StackTrace(true);
                    // Core.Log.LogInfo($"[UpdateBuffsBuffer_Destroy] - StackTrace captured:\n{stacktrack}");
                    // Core.Log.LogInfo($"[UpdateBuffsBuffer_Destroy] - StackTrace:\n{stacktrack.ToString()}");
                }

                // if (buffPrefabGuid.Equals(PrefabGUIDs.Buff_InCombat_PvPVampire))
                // // if (buffPrefabGuid.Equals(PrefabGUIDs.Buff_InCombat))
                // {
                //     Core.Log.LogInfo($"[UpdateBuffsBuffer_Destroy] - InCombat Buff Removed from {buffTarget}");
                //     foreach (PrefabGUID transformationBuff in AbilityService.combatShapeshiftForms)
                //     {
                //         Core.Log.LogInfo($"[UpdateBuffsBuffer_Destroy] - Checking transformation buff {transformationBuff} for {buffTarget}");
                //         if (BuffService.TryGetBuff(buffTarget, transformationBuff, out Entity buffEntity))
                //         {
                //             Core.Log.LogInfo($"[UpdateBuffsBuffer_Destroy] - Found transformation buff {transformationBuff} on {buffTarget}, making it stay on damage");
                //             BuffService.MakeShapeshiftBuffStayOnDamage(buffEntity);
                //             break;
                //         }
                //     }
                // }


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
