using SoVUtilities.Resources;
using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class CreateGameplayEventOnDestroySystemPatch
{
    static readonly PrefabGUID _fishingTravelToTarget = PrefabGUIDs.AB_Fishing_Draw_TravelToTarget;
    static readonly PrefabGUID _fishingQuestGoal = PrefabGUIDs.FakeItem_AnyFish;
    static readonly PrefabGUID wolfFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Buff;
    static readonly PrefabGUID wolfFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff;
    static readonly PrefabGUID wolfFormSkin2PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff;
    static readonly PrefabGUID wolfFormSkin3PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff;
    static readonly PrefabGUID bearFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Bear_Buff;
    static readonly PrefabGUID bearFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Bear_Skin01_Buff;
    static readonly PrefabGUID toadFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Toad_Buff;
    static readonly PrefabGUID toadFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Toad_PMK_Skin01_Buff;
    static readonly PrefabGUID spiderFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Spider_Buff;
    static readonly PrefabGUID humanFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Human_Buff;
    static readonly PrefabGUID humanFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Human_Grandma_Skin01_Buff;
    static readonly PrefabGUID humanFormSkin2PrefabGUID = PrefabGUIDs.AB_Shapeshift_Human_PMK_Skin02_Buff;
    static readonly PrefabGUID[] nameplateDisabledForms = new PrefabGUID[]
    {
        wolfFormPrefabGUID,
        wolfFormSkin1PrefabGUID,
        wolfFormSkin2PrefabGUID,
        wolfFormSkin3PrefabGUID,
        bearFormPrefabGUID,
        bearFormSkin1PrefabGUID,
        toadFormPrefabGUID,
        toadFormSkin1PrefabGUID,
        spiderFormPrefabGUID,
        humanFormPrefabGUID,
        humanFormSkin1PrefabGUID,
        humanFormSkin2PrefabGUID
    };

    [HarmonyPatch(typeof(CreateGameplayEventOnDestroySystem), nameof(CreateGameplayEventOnDestroySystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(CreateGameplayEventOnDestroySystem __instance)
    {
        if (!Core._initialized) return;

        NativeArray<Entity> entities = __instance.__query_1297357609_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity entity in entities)
            {
                if (!entity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists() || !entity.TryGetComponent(out PrefabGUID prefabGUID)) continue;
                else
                {
                    Entity playerCharacter = entityOwner.Owner;

                    bool isShapeshiftBuff = false;
                    foreach (PrefabGUID guid in nameplateDisabledForms)
                    {
                        if (prefabGUID.Equals(guid))
                        {
                            isShapeshiftBuff = true;
                            break;
                        }
                    }

                    if (isShapeshiftBuff)
                    {
                        BuffService.RemoveHideNameplateBuff(playerCharacter);
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