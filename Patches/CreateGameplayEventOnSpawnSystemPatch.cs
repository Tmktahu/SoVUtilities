using SoVUtilities.Resources;
using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using SoVUtilities.Services.Buffs;
using ProjectM.Network;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class CreateGameplayEventOnSpawnSystemPatch
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

    [HarmonyPatch(typeof(CreateGameplayEventOnSpawnSystem), nameof(CreateGameplayEventOnSpawnSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(CreateGameplayEventOnSpawnSystem __instance)
    {
        if (!Core._initialized) return;

        NativeArray<Entity> entities = __instance.__query_1606952066_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity entity in entities)
            {
                if (!entity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists() || !entity.TryGetComponent(out PrefabGUID prefabGUID)) continue;
                else
                {
                    if (Core.EntityManager.TryGetComponentData<PrefabGUID>(entity, out var entityPrefabGUID))
                    {
                        // Core.Log.LogInfo($"[CreateGameplayEventOnSpawnSystemPatch Prefix] - Spawned Buff Entity: {entity} with Prefab: {PrefabGUIDsExtensions.GetPrefabGUIDName(entityPrefabGUID)} for Owner: {entityOwner.Owner}");
                        if (entityPrefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Wolf_Howl_Trigger))
                        {
                            var playerData = PlayerDataService.GetPlayerData(entityOwner.Owner);

                            // the player has howled in wolf form
                            if (playerData.HasTag(TagService.Tags.WEREWOLF))
                            {
                                User user = entityOwner.Owner.GetUser();

                                if (!BuffService.HasBuff(entityOwner.Owner, PrefabGUIDs.Buff_InCombat) && !BuffService.HasBuff(entityOwner.Owner, PrefabGUIDs.Buff_InCombat_PvPVampire))
                                {

                                    if (WolfSpeedBuff.HasBuff(entityOwner.Owner))
                                    {
                                        WolfSpeedBuff.RemoveBuff(entityOwner.Owner);
                                        FixedString512Bytes message = new FixedString512Bytes("Your speed returns to normal as the wolf within calms.");
                                        ServerChatUtils.SendSystemMessageToClient(__instance.EntityManager, user, ref message);
                                    }
                                    else
                                    {
                                        // Core.Log.LogInfo($"[CreateGameplayEventOnSpawnSystemPatch Prefix] - Player {entityOwner.Owner} has howled in wolf form, applying wolf form speed boost buff.");
                                        // we want to boost their speed TODO
                                        // BuffService.RefreshPlayerBuffs(entityOwner.Owner, [GlobalStatBuffFlags.WolfFormSpeedBoost]).Start();
                                        WolfSpeedBuff.ApplyCustomBuff(entityOwner.Owner).Start();
                                        FixedString512Bytes message = new FixedString512Bytes("You let out a mighty howl, your speed increasing as the wolf within takes over.");
                                        ServerChatUtils.SendSystemMessageToClient(__instance.EntityManager, user, ref message);
                                    }
                                }
                                else
                                {
                                    FixedString512Bytes message = new FixedString512Bytes("Your inner wolf would rather fight than run right now!");
                                    ServerChatUtils.SendSystemMessageToClient(__instance.EntityManager, user, ref message);
                                }
                            }
                        }

                        // Entity playerCharacter = entityOwner.Owner;
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