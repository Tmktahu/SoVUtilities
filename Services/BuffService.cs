using Stunlock.Core;
using SoVUtilities.Resources;
using ProjectM.Network;
using Unity.Entities;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
using System.Collections;
using SoVUtilities.Services.Buffs;
using UnityEngine;

namespace SoVUtilities.Services;

internal static class BuffService
{
  static SystemService SystemService => Core.SystemService;
  static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;
  static ServerGameManager ServerGameManager => Core.ServerGameManager;
  static EntityManager EntityManager => Core.EntityManager;

  public static readonly PrefabGUID HumanBuffBase = PrefabGUIDs.SetBonus_Silk_Twilight;
  public static readonly PrefabGUID HideNameplateBuffGuid = PrefabGUIDs.AB_Cursed_ToadKing_Spit_HideHUDCastBuff;

  public static string HumanBuffId = "human_buff";
  public static string HideNameplateBuffId = "hide_nameplate_buff";
  public static string AfflictedBuffId = "afflicted_buff";
  public static string BeholdenBuffId = "beholden_buff";
  public static string EncasedBuffId = "encased_buff";
  public static readonly Dictionary<string, ICustomBuff> AvailableBuffs = new()
  {
    // { HumanBuffId, new HumanCustomBuff() },
    { HideNameplateBuffId, new HideNameplateCustomBuff() },
    { AfflictedBuffId, new AfflictedCustomBuff() },
    { BeholdenBuffId, new BeholdenCustomBuff() },
    { EncasedBuffId, new EncasedCustomBuff() }
  };

  public static void ApplyBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    // Create buff application event
    ApplyBuffDebugEvent applyBuffDebugEvent = new()
    {
      BuffPrefabGUID = buffPrefabGuid,
      Who = entity.GetNetworkId()
    };

    // Create character reference
    FromCharacter fromCharacter = new()
    {
      Character = entity,
      User = entity.IsPlayer() ? entity.GetUserEntity() : entity
    };

    // Apply the buff
    DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);
  }

  public static IEnumerator RefreshPlayerBuffs(Entity playerCharacter)
  {
    // First, remove all buffs
    // Core.Log.LogInfo($"[BuffService.RefreshPlayerBuffs] - Removing all buffs from {playerCharacter}");
    foreach (var buffId in AvailableBuffs.Keys)
    {
      RemoveBuffFromPlayer(playerCharacter, buffId);
    }

    // Core.Log.LogInfo($"[BuffService.RefreshPlayerBuffs] - Waiting one second.");
    // Wait 1 second for all cleanups to complete
    yield return new WaitForSeconds(1f);

    // Then add all buffs back
    // Core.Log.LogInfo($"[BuffService.RefreshPlayerBuffs] - Adding all buffs back to {playerCharacter}");
    foreach (var buffId in AvailableBuffs.Keys)
    {
      // Core.Log.LogInfo($"[BuffService.RefreshPlayerBuffs] - Checking if player should have buff {buffId}.");
      bool shouldHaveBuff = TagService.ShouldHaveBuff(playerCharacter, buffId);
      // Core.Log.LogInfo($"[BuffService.RefreshPlayerBuffs] - Player {playerCharacter} should have buff {buffId}: {shouldHaveBuff}");

      if (shouldHaveBuff)
      {
        // Core.Log.LogInfo($"[BuffService.RefreshPlayerBuffs] - Adding buff {buffId} to {playerCharacter}");
        // we only add the buff if the player should have it
        AddCustomBuffToPlayer(playerCharacter, buffId);
      }
    }
  }

  public static void AddHideNameplateBuff(Entity entity)
  {
    // Core.Log.LogInfo($"Adding hide nameplate buff to entity {entity}.");
    ApplyBuff(entity, HideNameplateBuffGuid);

    if (!TryGetBuff(entity, HideNameplateBuffGuid, out Entity buffEntity))
    {
      Core.Log.LogWarning($"Failed to get buff entity for {HideNameplateBuffGuid} on {entity}. Not making it permanent.");
      return;
    }
    makeBuffPermanent(buffEntity);
  }

  public static void RemoveHideNameplateBuff(Entity entity)
  {
    // if they should have this buff via tags, we don't remove it
    if (TagService.ShouldHaveBuff(entity, HideNameplateBuffId))
    {
      // Core.Log.LogInfo($"Entity {entity} should have hide nameplate buff via tags. Not removing it.");
      return;
    }

    // Core.Log.LogInfo($"Removing hide nameplate buff from entity {entity}.");
    if (TryGetBuff(entity, HideNameplateBuffGuid, out Entity buffEntity))
    {
      DestroyBuff(buffEntity);
    }
  }

  public static bool HasHideNameplateBuff(Entity entity)
  {
    bool hasBuff = HasBuff(entity, HideNameplateBuffGuid);
    // Core.Log.LogInfo($"Entity {entity} has hide nameplate buff: {hasBuff}");
    return hasBuff;
  }

  public static void makeBuffPermanent(Entity buffEntity)
  {
    // Core.Log.LogInfo($"Making buff {buffEntity} permanent.");
    // Remove components that would cause the buff to be removed
    EntityManager.RemoveComponent<RemoveBuffOnGameplayEvent>(buffEntity);
    EntityManager.RemoveComponent<RemoveBuffOnGameplayEventEntry>(buffEntity);
    EntityManager.RemoveComponent<CreateGameplayEventsOnSpawn>(buffEntity);
    EntityManager.RemoveComponent<GameplayEventListeners>(buffEntity);
    EntityManager.RemoveComponent<DestroyOnGameplayEvent>(buffEntity);

    // Set lifetime to permanent
    if (EntityManager.HasComponent<LifeTime>(buffEntity))
    {
      // Core.Log.LogInfo($"Setting lifetime of buff {buffEntity} to permanent.");
      var lifeTime = EntityManager.GetComponentData<LifeTime>(buffEntity);
      lifeTime.Duration = 0f;
      lifeTime.EndAction = LifeTimeEndAction.None;
      EntityManager.SetComponentData(buffEntity, lifeTime);
    }
  }

  public static bool TryGetBuff(Entity entity, PrefabGUID buffPrefabGUID, out Entity buffEntity)
  {
    if (ServerGameManager.TryGetBuff(entity, buffPrefabGUID.ToIdentifier(), out buffEntity))
    {
      return true;
    }

    return false;
  }

  public static void TryRemoveBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    if (TryGetBuff(entity, buffPrefabGuid, out Entity buffEntity))
    {
      DestroyBuff(buffEntity);
    }
  }

  public static void DestroyBuff(Entity buffEntity)
  {
    if (buffEntity.Exists()) DestroyUtility.Destroy(EntityManager, buffEntity, DestroyDebugReason.TryRemoveBuff);
  }
  public static bool RemoveBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    TryRemoveBuff(entity, buffPrefabGuid);
    // Core.Log.LogInfo($"Removed buff of type {buffPrefabGuid} from entity {entity}.");
    return true;
  }

  public static bool HasBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    return ServerGameManager.HasBuff(entity, buffPrefabGuid.ToIdentifier());
  }

  public static void SetupSyncBuffer(Entity buffEntity, Entity playerCharacter)
  {
    if (buffEntity.TryGetBuffer<SyncToUserBuffer>(out var syncToUsers))
    {
      if (syncToUsers.IsEmpty)
      {
        SyncToUserBuffer syncToUserBuffer = new()
        {
          UserEntity = playerCharacter.GetUserEntity()
        };
        syncToUsers.Add(syncToUserBuffer);
      }
    }
  }

  public static void AddCustomBuffToPlayer(Entity characterEntity, string buffId)
  {
    if (TryGetCustomBuff(buffId, out var customBuff))
    {
      // Core.Log.LogInfo($"[BuffService.AddCustomBuffToPlayer] - Applying custom buff {buffId} to {characterEntity}");
      customBuff.ApplyCustomBuff(characterEntity);
    }
    else
    {
      Core.Log.LogError($"[BuffService.AddBuffToPlayer] - Buff ID {buffId} not found.");
    }
  }

  public static bool RemoveBuffFromPlayer(Entity characterEntity, string customBuffId)
  {
    if (TryGetCustomBuff(customBuffId, out var customBuff))
    {
      customBuff.RemoveBuff(characterEntity);
      return true;
    }

    return false;
  }

  public static bool TryGetCustomBuff(string buffId, out ICustomBuff customBuff)
  {
    return AvailableBuffs.TryGetValue(buffId, out customBuff);
  }
}