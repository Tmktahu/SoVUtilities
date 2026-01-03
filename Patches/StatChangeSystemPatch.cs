using static SoVUtilities.Services.PlayerDataService;
using static SoVUtilities.Services.TagService;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Stunlock.Core;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using SoVUtilities.Resources;
using ProjectM.Network;
using SoVUtilities.Services;
using System.Data;
using SoVUtilities.Services.Buffs;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class StatChangeSystemPatch
{
    static EntityManager EntityManager => Core.EntityManager;
    static readonly ConcurrentDictionary<Entity, float> entityBloodPoolValues = new();
    static readonly PrefabGUID VampireMale = PrefabGUIDs.CHAR_VampireMale;
    static readonly PrefabGUID HolyDebuffT1 = PrefabGUIDs.Buff_General_Holy_Area_T01; //  Prefab 'PrefabGuid(1593142604)': Buff_General_Holy_Area_T01
    static readonly PrefabGUID HolyDebuffT2 = PrefabGUIDs.Buff_General_Holy_Area_T02; //  Prefab 'PrefabGuid(-621774510)': Buff_General_Holy_Area_T02
    // static readonly PrefabGUID VampireFemale = PrefabGUIDs.CHAR_VampireFemale;

    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
    [HarmonyPrefix]
    static bool OnUpdatePrefix(StatChangeSystem __instance)
    {
        if (!Core._initialized) return true;

        var entityArray = __instance._Query.ToEntityArray(Allocator.Temp);

        try
        {
            EntityManager entityManager = Core.EntityManager;

            for (int i = 0; i < entityArray.Length; i++)
            {
                Entity statEntity = entityArray[i];
                StatChangeEvent statChangeEvent = entityManager.GetComponentData<StatChangeEvent>(statEntity);
                PrefabGUID sourcePrefabGuid = statChangeEvent.Source.GetPrefabGuid();
                Entity targetEntity = statChangeEvent.Entity;

                bool isPlayerCharacter = entityManager.HasComponent<PlayerCharacter>(targetEntity);

                if (statChangeEvent.StatType == StatType.Blood)
                {
                    // now we need to see if this is a human via its tag
                    if (HasPlayerTag(targetEntity, HUMAN_TAG))
                    {
                        if (entityManager.HasComponent<BloodQualityChange>(statEntity))
                        {
                            // the entity has a BloodQualityChange component
                            BloodQualityChange bloodQualityChange = entityManager.GetComponentData<BloodQualityChange>(statEntity);
                            PrefabGUID bloodTypePrefabGuid = bloodQualityChange.BloodType;
                            PrefabGUID bloodSourcePrefabGuid = bloodQualityChange.BloodSource;

                            if (bloodSourcePrefabGuid.Equals(VampireMale)) // && !bloodTypePrefabGuid.Equals(PrefabGUIDs.BloodType_None)) // we dont want this right now. it works, but not for our use case yet
                            {
                                // this happens when they drink a blood potion
                                // if they eat a heart, the blood type is none, so we check for that
                                // if they eat a heart we want to continue to lock the blood pool
                                // so this check means we do NOT lock the pool if they eat any kind of non-frail potion item
                                return true;
                            }

                            // it has a blood component
                            Blood bloodComponent = entityManager.GetComponentData<Blood>(targetEntity);
                            float bloodValue = bloodComponent.Value;

                            // Update the dictionary with the new blood value
                            entityBloodPoolValues[targetEntity] = bloodValue;
                        }
                        else
                        {
                            // the entity does not have a BloodQualityChange component
                            // that means this is likely from a blood soul
                            // we need to trigger a blood pool reset when this happens
                            // that means we need the target entity

                            Entity sourceEntity = statChangeEvent.Source;
                            PrefabGUID sourceEntityPrefabGuid = entityManager.GetComponentData<PrefabGUID>(sourceEntity);

                            if (sourceEntityPrefabGuid.Equals(VampireMale))
                            {
                                // this happens when they get a refresh from a blood soul
                                // return true; // we actually want things to continue because we WANT a reset in this case
                            }

                            Blood bloodComponent = entityManager.GetComponentData<Blood>(targetEntity);
                            float bloodValue = bloodComponent.Value;

                            // Update the dictionary with the new blood value
                            entityBloodPoolValues[targetEntity] = bloodValue;
                        }
                    }


                }

                // if it's either of the holy debuffs, we don't want to process it further
                if (sourcePrefabGuid.Equals(HolyDebuffT1) || sourcePrefabGuid.Equals(HolyDebuffT2))
                {
                    Entity target = statChangeEvent.Entity; // this is the target of the stat change
                    if (HasPlayerTag(targetEntity, HUMAN_TAG))
                    {
                        statEntity.Destroy(true);
                    }
                }

                if (EntityManager.TryGetComponentData<EntityOwner>(statChangeEvent.Source, out var entityOwner))
                {
                    Entity ownerEntity = entityOwner.Owner;

                    if (!ownerEntity.Exists() || !EntityManager.HasComponent<PrefabGUID>(ownerEntity)) continue;
                    PrefabGUID ownerPrefabGuid = ownerEntity.GetPrefabGuid(); // this is the owner of the attack that triggered the damage
                    PrefabGUID targetPrefabGuid = entityManager.GetComponentData<PrefabGUID>(targetEntity);

                    // Core.Log.LogInfo($"[StatChangeSystem] Processing attack from owner prefab: {ownerPrefabGuid.ToString()} to target prefab: {targetPrefabGuid.ToString()}");

                    if (statChangeEvent.StatType == StatType.Health && isPlayerCharacter && statChangeEvent.Change < 0)
                    {
                        // remove the wolf speed buff if they take damage
                        if (WolfSpeedBuff.HasBuff(targetEntity))
                        {
                            WolfSpeedBuff.RemoveBuff(targetEntity);
                        }
                    }

                    if (statChangeEvent.StatType == StatType.Health && isPlayerCharacter && ownerPrefabGuid.Equals(PrefabGUIDs.CHAR_VampireMale) && !targetEntity.Equals(ownerEntity) && statChangeEvent.Change < 0)
                    {

                        // debug info log statements
                        float change = statChangeEvent.Change;
                        float originalChange = statChangeEvent.OriginalChange;
                        int statChangeFlags = statChangeEvent.StatChangeFlags;
                        StatChangeReason statChangeReason = statChangeEvent.Reason;
                        var modTypes = (ProjectM.StatChangeFlag)statChangeFlags;
                        // Core.Log.LogInfo($"[StatChangeSystem] Health Change Event - Target: {targetEntity}, Change: {change}, OriginalChange: {originalChange}, StatChangeFlags: {statChangeFlags}, Reason: {statChangeReason}, SourcePrefabGuid: {sourcePrefabGuid}");
                        // Core.Log.LogInfo($"StatChangeFlag Types: {modTypes.ToString()}");

                        Entity userEntity = targetEntity.GetUserEntity();
                        if (EntityManager.TryGetComponentData<Shapeshift>(userEntity, out var shapeshift))
                        {
                            if (shapeshift.IsShapeshifted)
                            {
                                // Core.Log.LogInfo($"[StatChangeSystem] Target is shapeshifted.");
                                foreach (PrefabGUID transformationBuff in AbilityService.combatShapeshiftForms)
                                {
                                    if (BuffService.TryGetBuff(targetEntity, transformationBuff, out Entity buffEntity))
                                    {
                                        BuffService.DestroyBuff(buffEntity);
                                        // BuffService.RefreshPlayerBuffs(entityOwner.Owner).Start();
                                        break;
                                    }
                                }
                                BuffService.RemoveBatForm(targetEntity);
                            }
                        }

                        // statEntity.Destroy(true); // prevent the health change
                        // modTypes |= ProjectM.StatChangeFlag.IsDoT; // example modification: add IsDoT flag
                        // statChangeEvent.StatChangeFlags = (int)modTypes;
                        // entityManager.SetComponentData(statEntity, statChangeEvent);
                    }


                    if (ownerPrefabGuid.Equals(PrefabGUIDs.CHAR_VampireMale))
                    {
                        Team playerTeam;
                        Team targetTeam;

                        if (EntityManager.TryGetComponentData<Team>(ownerEntity, out playerTeam) &&
                            EntityManager.TryGetComponentData<Team>(targetEntity, out targetTeam))
                        {
                            int playerFactionIndex = playerTeam.FactionIndex;
                            int targetFactionIndex = targetTeam.FactionIndex;
                            // Core.Log.LogInfo($"[StatChangeSystem] Player Faction Index: {playerFactionIndex}");
                            // Core.Log.LogInfo($"[StatChangeSystem] Target Faction Index: {targetFactionIndex}");

                            Entity playerFactionEntity;
                            Entity targetFactionEntity;

                            FactionLookupSingleton factionLookupSingleton = Core.FactionLookupSystem.GetSingleton<FactionLookupSingleton>();

                            factionLookupSingleton.TryGetPrefabEntity(playerFactionIndex, out playerFactionEntity);
                            factionLookupSingleton.TryGetPrefabEntity(targetFactionIndex, out targetFactionEntity);

                            PrefabGUID playerFactionPrefabGuid = playerFactionEntity.GetPrefabGuid();
                            PrefabGUID targetFactionPrefabGuid = targetFactionEntity.GetPrefabGuid();

                            if (!targetFactionPrefabGuid.Equals(PrefabGUIDs.Faction_Players))
                            {
                                // if they are equal to each other, then we have a player allied to the target they are hitting
                                if (playerFactionPrefabGuid.Equals(targetFactionPrefabGuid))
                                {
                                    if (BuffService.HasBuff(ownerEntity, PrefabGUIDs.Buff_InCombat))
                                    {
                                        // Core.Log.LogInfo($"[StatChangeSystem] Player {playerFactionPrefabGuid} is allied with Target {targetFactionPrefabGuid}");
                                        TeamService.SetToPlayerFaction(ownerEntity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Core.Log.LogWarning($"[StatChangeSystem] Exception: {e}");
        }

        return true; // Continue with the original method
    }

    // Queue for pending blood changes
    static readonly Queue<KeyValuePair<Entity, float>> _pendingBloodChanges = new();
    static DateTime _lastProcessTime = DateTime.UtcNow;

    [HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
    [HarmonyPostfix]
    static void OnUpdatePostfix(StatChangeSystem __instance)
    {
        if (!Core._initialized) return;

        try
        {
            // Process any pending blood changes first (from previous frames)
            ProcessPendingBloodChanges();

            // Add new entities to the queue
            if (!entityBloodPoolValues.IsEmpty)
            {
                foreach (var kvp in entityBloodPoolValues)
                {
                    _pendingBloodChanges.Enqueue(kvp);
                }
                entityBloodPoolValues.Clear();
            }

            // Process the queued blood changes
            if (_pendingBloodChanges.Count > 0)
            {
                ProcessPendingBloodChanges();
            }
        }
        catch (Exception e)
        {
            Core.Log.LogError($"[StatChangeSystem] Postfix Exception: {e}");
            entityBloodPoolValues.Clear();
        }
    }

    static void ProcessPendingBloodChanges()
    {
        // Only process every 100ms to avoid overwhelming the system
        if ((DateTime.UtcNow - _lastProcessTime).TotalMilliseconds < 100) return;

        if (_pendingBloodChanges.Count == 0) return;

        try
        {
            var entityManager = Core.EntityManager;
            int processed = 0;

            // Process up to 5 entities per frame to avoid frame spikes
            while (_pendingBloodChanges.Count > 0 && processed < 5)
            {
                var kvp = _pendingBloodChanges.Dequeue();
                Entity entity = kvp.Key;
                float bloodValue = kvp.Value;

                if (entityManager.Exists(entity) && entityManager.HasComponent<Blood>(entity))
                {
                    var bloodComponent = entityManager.GetComponentData<Blood>(entity);
                    bloodComponent.Value = bloodValue;
                    entityManager.SetComponentData(entity, bloodComponent);
                }

                processed++;
            }

            _lastProcessTime = DateTime.UtcNow;
        }
        catch (Exception e)
        {
            Core.Log.LogError($"[StatChangeSystem] ProcessPending Exception: {e}");
            // Clear the queue on error to prevent infinite loops
            _pendingBloodChanges.Clear();
        }
    }
}