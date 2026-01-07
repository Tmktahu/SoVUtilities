using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;
using SoVUtilities.Models;
using SoVUtilities.Services.Buffs;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class ReplaceAbilityOnSlotSystemPatch
{
    static EntityManager EntityManager => Core.EntityManager;

    [HarmonyPatch(typeof(ReplaceAbilityOnSlotSystem), nameof(ReplaceAbilityOnSlotSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(ReplaceAbilityOnSlotSystem __instance)
    {
        if (!Core._initialized) return;

        NativeArray<Entity> entities = __instance.__query_1482480545_0.ToEntityArray(Allocator.Temp);

        try
        {
            foreach (Entity buffEntity in entities)
            {
                try
                {
                    PrefabGUID prefabGUID = EntityManager.GetComponentData<PrefabGUID>(buffEntity);
                    if (prefabGUID.IsEmpty()) continue;

                    if (prefabGUID.Equals(PrefabGUIDs.AB_Feed_01_EnemyTarget_Debuff))
                        continue; // Skip this specific buff

                    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(prefabGUID);
                    if (string.IsNullOrEmpty(prefabName)) continue; // Skip if prefabName is invalid
                    // Core.Log.LogInfo($"[ReplaceAbilityOnSlotSystemPatch Prefix] - Processing Buff Entity: {buffEntity} with Prefab: {prefabName}");

                    if (!buffEntity.TryGetComponent(out EntityOwner entityOwner) || !entityOwner.Owner.Exists()) continue;
                    else if (entityOwner.Owner.TryGetPlayer(out Entity character))
                    {
                        PlayerData playerData = PlayerDataService.GetPlayerData(character);
                        int[] abilitySlotPrefabGUIDs = null;

                        if (prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Wolf_Buff) ||
                            prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff) ||
                            prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff) ||
                            prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff))
                        {
                            abilitySlotPrefabGUIDs = AbilityService.WolfFormAbilitySlotPrefabGUIDs;
                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs, 9);
                            continue;
                        }

                        if (prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Toad_Buff)) // || BuffService.HasBuff(character, PrefabGUIDs.AB_Shapeshift_Toad_PMK_Skin01_Buff))
                        {
                            // the man toad form has no tongue
                            abilitySlotPrefabGUIDs = AbilityService.ToadFormAbilitySlotPrefabGUIDs;
                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs, 9);
                            continue;
                        }

                        if (prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Rat_Buff))
                        {
                            abilitySlotPrefabGUIDs = AbilityService.RatFormAbilitySlotPrefabGUIDs;
                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs, 9);
                            continue;
                        }

                        if (prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Spider_Buff))
                        {
                            abilitySlotPrefabGUIDs = AbilityService.SpiderFormAbilitySlotPrefabGUIDs;
                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs, 9);
                            continue;
                        }

                        if (prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Bear_Buff) ||
                            prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Bear_Skin01_Buff))
                        {
                            abilitySlotPrefabGUIDs = AbilityService.BearFormAbilitySlotPrefabGUIDs;
                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs, 9);
                            continue;
                        }

                        // if (prefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Bat_TakeFlight_Buff))
                        // {
                        //     abilitySlotPrefabGUIDs = AbilityService.BatFormAbilitySlotPrefabGUIDs;
                        //     AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs, 9);
                        //     continue;
                        // }

                        // if they have the werewolf tag and are in werewolf form, we apply the werewolf abilities instead
                        if (playerData.HasTag(TagService.Tags.WEREWOLF) && WerewolfBuff.HasBuff(character))
                        {
                            abilitySlotPrefabGUIDs = WerewolfBuff.abilitySlotPrefabGUIDs;
                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs, 9);
                            continue;
                        }

                        if (prefabName.Contains("unarmed", Il2CppSystem.StringComparison.OrdinalIgnoreCase) ||
                            prefabName.Contains("weapon", Il2CppSystem.StringComparison.OrdinalIgnoreCase))
                        {
                            // item abilities take priority over categorical abilities
                            // get the player's equipment component
                            Equipment equipment = EntityManager.GetComponentData<Equipment>(character);
                            EquipmentSlot weaponSlot = equipment.WeaponSlot;
                            Entity itemEntity = weaponSlot.SlotEntity._Entity;

                            if (EntityManager.Exists(itemEntity) && ItemDataService.HasItemData(itemEntity))
                            {
                                Services.ItemData itemData = ItemDataService.GetItemData(itemEntity);
                                abilitySlotPrefabGUIDs = itemData.AbilityGUIDs;
                            }

                            // for each category, check if the prefabName contains it
                            // and if so, get the corresponding abilitySlotPrefabGUIDs
                            if (abilitySlotPrefabGUIDs == null)
                            {
                                foreach (var category in AbilityService.weaponCategories)
                                {
                                    if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase) && abilitySlotPrefabGUIDs == null)
                                    {
                                        AbilityService.WeaponCategoryToDefaultEquipBuff.TryGetValue(category, out var defaultEquipBuff);
                                        playerData.AbilitySlotDefinitions.TryGetValue(defaultEquipBuff._Value, out abilitySlotPrefabGUIDs);
                                        break;
                                    }
                                }
                            }

                            AbilityService.ApplyAbilities(character, buffEntity, abilitySlotPrefabGUIDs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Core.Log.LogError($"ReplaceAbilityOnSlotSystemPatch Prefix: Exception processing entity {buffEntity.Index}: {ex.Message}");
                }
            }
        }
        finally
        {
            entities.Dispose();
        }
    }
}