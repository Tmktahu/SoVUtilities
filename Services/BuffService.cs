using Stunlock.Core;
using SoVUtilities.Resources;
using ProjectM.Network;
using Unity.Entities;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;

namespace SoVUtilities.Services;

internal static class BuffService
{
  static SystemService SystemService => Core.SystemService;
  static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;
  static ServerGameManager ServerGameManager => Core.ServerGameManager;
  static EntityManager EntityManager => Core.EntityManager;

  public static readonly PrefabGUID HumanBuffBase = PrefabGUIDs.SetBonus_Silk_Twilight;
  public static readonly PrefabGUID HideNameplateBuffGuid = PrefabGUIDs.AB_Cursed_ToadKing_Spit_HideHUDCastBuff;

  public static bool ApplyBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    // Check if entity already has this buff
    bool hasBuff = HasBuff(entity, buffPrefabGuid);

    if (!hasBuff)
    {
      // Core.Log.LogInfo($"Applying buff {buffPrefabGuid} to entity {entity}.");
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

      // Core.Log.LogInfo($"FromCharacter: {fromCharacter.Character} | User: {fromCharacter.User}");
      // Apply the buff
      DebugEventsSystem.ApplyBuff(fromCharacter, applyBuffDebugEvent);

      return true;
    }
    else
    {
      Core.Log.LogWarning($"Entity {entity} already has buff {buffPrefabGuid}. Not applying again.");
    }

    return false;
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
    // Remove components that would cause the buff to be removed
    EntityManager.RemoveComponent<RemoveBuffOnGameplayEvent>(buffEntity);
    EntityManager.RemoveComponent<RemoveBuffOnGameplayEventEntry>(buffEntity);
    EntityManager.RemoveComponent<CreateGameplayEventsOnSpawn>(buffEntity);
    EntityManager.RemoveComponent<GameplayEventListeners>(buffEntity);
    EntityManager.RemoveComponent<DestroyOnGameplayEvent>(buffEntity);

    // Set lifetime to permanent
    if (EntityManager.HasComponent<LifeTime>(buffEntity))
    {
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
}