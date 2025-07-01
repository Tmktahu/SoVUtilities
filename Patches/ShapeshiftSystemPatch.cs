using SoVUtilities.Resources;
using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using ProjectM.Network;
using User = ProjectM.Network.User;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class ShapeshiftSystemPatch
{
    static EntityManager EntityManager => Core.EntityManager;
    static readonly PrefabGUID wolfFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Group;
    static readonly PrefabGUID wolfFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Group;
    static readonly PrefabGUID wolfFormSkin2PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Group;
    static readonly PrefabGUID wolfFormSkin3PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Group;
    static readonly PrefabGUID bearFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Bear_Group;
    static readonly PrefabGUID bearFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Bear_Skin01_Group;
    static readonly PrefabGUID ratFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Rat_Group; // this one doesn't have a nameplate
    static readonly PrefabGUID toadFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Toad_Group;
    static readonly PrefabGUID toadFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Toad_PMK_Skin01_Group;
    static readonly PrefabGUID spiderFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Spider_Group;
    static readonly PrefabGUID batFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Bat_Group;
    static readonly PrefabGUID humanFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Human_Group;
    static readonly PrefabGUID humanFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Human_Grandma_Skin01_Group;
    static readonly PrefabGUID humanFormSkin2PrefabGUID = PrefabGUIDs.AB_Shapeshift_Human_PMK_Skin02_Group;
    static readonly PrefabGUID dominateFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_DominatingPresence_PsychicForm_Group;
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

    [HarmonyPatch(typeof(ShapeshiftSystem), nameof(ShapeshiftSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(ShapeshiftSystem __instance)
    {
        if (!Core._initialized) return;
        // Core.Log.LogInfo("ShapeshiftSystemPatch: OnUpdatePrefix called.");

        NativeArray<Entity> entities = __instance._Query.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity entity in entities)
            {
                if (!entity.TryGetComponent(out FromCharacter fromCharacter)) continue;
                EnterShapeshiftEvent enterShapeshiftEvent = entity.Read<EnterShapeshiftEvent>();

                Entity playerCharacter = fromCharacter.Character;
                User user = playerCharacter.GetUser();
                ulong steamId = user.PlatformId;

                PrefabGUID shapeshiftPrefabGUID = enterShapeshiftEvent.Shapeshift;
                // Core.Log.LogInfo($"ShapeshiftSystemPatch: Player {steamId} entered shapeshift: {shapeshiftPrefabGUID}");

                // we want to loop over each disabled shapeshift form and check if the current shapeshift is one of them
                bool isNameplateDisabled = false;
                foreach (PrefabGUID disabledForm in nameplateDisabledForms)
                {
                    if (shapeshiftPrefabGUID.Equals(disabledForm))
                    {
                        isNameplateDisabled = true;
                        break;
                    }
                }

                // If the shapeshift is one of the forms that should not show a nameplate, apply the hide nameplate buff
                if (isNameplateDisabled)
                {
                    // Core.Log.LogInfo($"ShapeshiftSystemPatch: Disabling nameplate for shapeshift {shapeshiftPrefabGUID} for player {steamId}.");
                    BuffService.AddHideNameplateBuff(playerCharacter);
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}
