using Unity.Collections;
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
using ProjectM.Gameplay.Scripting;
using Unity.Transforms;
using Unity.Mathematics;

namespace SoVUtilities.Services;

internal static class BuffService
{
  static SystemService SystemService => Core.SystemService;
  static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;
  static ServerGameManager ServerGameManager => Core.ServerGameManager;
  static EntityManager EntityManager => Core.EntityManager;

  // public static readonly PrefabGUID globalStatsBuff = PrefabGUIDs.SetBonus_AllLeech_T09;
  static readonly PrefabGUID razerHood = PrefabGUIDs.Item_Headgear_RazerHood;
  public static readonly PrefabGUID HideNameplateBuffGuid = PrefabGUIDs.AB_Cursed_ToadKing_Spit_HideHUDCastBuff;

  public static string GlobalStatsBuffId = "global_stats_buff";
  public static string HumanBuffId = "human_buff";
  public static string HideNameplateBuffId = "hide_nameplate_buff";
  public static string AfflictedBuffId = "afflicted_buff";
  public static string BeholdenBuffId = "beholden_buff";
  public static string EncasedBuffId = "encased_buff";
  public static string ConsumedBuffId = "consumed_buff";
  public static string SeededBuffId = "seeded_buff";
  public static string RelicBuffId = "relic_buff";
  public static string SpiritChosenBuffId = "spirit_chosen_buff";
  public static string WerewolfBuffId = "werewolf_buff";
  public static string WerewolfStatsBuffId = "werewolf_stats_buff";
  public static string RotlingBuffId = "rotling_buff";
  public static string WolfSpeedBuffId = "wolf_speed_buff";
  public static string FaeBuffId = "fae_buff";
  public static string DaemonBuffId = "daemon_buff";
  public static string RotlingGlowBuffId = "rotling_glow_buff";
  public static string KalldenBuffId = "kallden_buff";
  public static string BlindnessBuffId = "blindness_buff";

  public static readonly Dictionary<string, ICustomBuff> AvailableBuffs = new()
  {
    // { GlobalStatsBuffId, new GlobalStatsCustomBuff() },
    { HumanBuffId, new HumanCustomBuff() },
    { HideNameplateBuffId, new HideNameplateCustomBuff() },
    { AfflictedBuffId, new AfflictedCustomBuff() },
    { BeholdenBuffId, new BeholdenCustomBuff() },
    { EncasedBuffId, new EncasedCustomBuff() },
    { ConsumedBuffId, new ConsumedCustomBuff() },
    { SeededBuffId, new SeededCustomBuff() },
    { RelicBuffId, new RelicCustomBuff() },
    { SpiritChosenBuffId, new SpiritChosenCustomBuff() },
    { WerewolfStatsBuffId, new WerewolfStatsCustomBuff() },
    { RotlingBuffId, new RotlingCustomBuff() },
    { FaeBuffId, new FaeCustomBuff() },
    { DaemonBuffId, new DaemonCustomBuff() },
    { RotlingGlowBuffId, new RotlingGlowCustomBuff() },
    { KalldenBuffId, new KalldenCustomBuff() },
    { BlindnessBuffId, new BlindnessCustomBuff() }
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

  public static void ApplyPermanentBuff(Entity entity, PrefabGUID buffPrefabGuid)
  {
    // if they already have the buff we do nothing
    if (HasBuff(entity, buffPrefabGuid))
      return;

    ApplyBuff(entity, buffPrefabGuid);

    if (!TryGetBuff(entity, buffPrefabGuid, out Entity buffEntity))
    {
      Core.Log.LogWarning($"Failed to get buff entity for {buffPrefabGuid} on {entity}. Not making it permanent.");
      return;
    }
    makeBuffPermanent(buffEntity);
  }

  public static IEnumerator RefreshPlayerBuffs(Entity playerCharacter, GlobalStatBuffFlags[] flags = null)
  {
    // First, remove all buffs
    foreach (var buffId in AvailableBuffs.Keys)
    {
      RemoveBuffFromPlayer(playerCharacter, buffId);
    }

    // Wait 1 second for all cleanups to complete
    yield return new WaitForSeconds(1f);

    // Then add all buffs back
    foreach (var buffId in AvailableBuffs.Keys)
    {
      bool shouldHaveBuff = TagService.ShouldHaveBuff(playerCharacter, buffId);
      if (shouldHaveBuff || buffId == GlobalStatsBuffId)
      {
        // we only add the buff if the player should have it
        AddCustomBuffToPlayer(playerCharacter, buffId, flags);
      }
    }
  }

  public static void AddHideNameplateBuff(Entity entity)
  {
    var playerData = PlayerDataService.GetPlayerData(entity);
    if (playerData.DisableHideNameplate)
    {
      Entity userEntity = entity.GetUserEntity();
      User user = EntityManager.GetComponentData<User>(userEntity);
      FixedString512Bytes message = new FixedString512Bytes("Your nameplate is still visible because your '.sov alwaysreveal' is enabled.");
      ServerChatUtils.SendSystemMessageToClient(EntityManager, user, ref message);
      return;
    }

    ApplyBuff(entity, HideNameplateBuffGuid);

    if (!TryGetBuff(entity, HideNameplateBuffGuid, out Entity buffEntity))
    {
      Core.Log.LogWarning($"Failed to get buff entity for {HideNameplateBuffGuid} on {entity}. Not making it permanent.");
      return;
    }
    makeBuffPermanent(buffEntity);
  }

  public static void RemoveHideNameplateBuff(Entity entity, bool bypass = false)
  {
    // if they should have this buff via tags, we don't remove it
    if (TagService.ShouldHaveBuff(entity, HideNameplateBuffId))
    {
      return;
    }

    // we also want to check if they have the razor hood equipped
    if (EntityManager.HasComponent<Equipment>(entity) && !bypass)
    {
      Equipment equipment = EntityManager.GetComponentData<Equipment>(entity);
      if (equipment.IsEquipped(razerHood, out var _))
      {
        return; // If the player has the Razer Hood equipped, we do not remove the hide nameplate buff
      }
    }

    if (TryGetBuff(entity, HideNameplateBuffGuid, out Entity buffEntity))
    {
      DestroyBuff(buffEntity);
    }
  }

  public static bool HasHideNameplateBuff(Entity entity)
  {
    bool hasBuff = HasBuff(entity, HideNameplateBuffGuid);
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

  public static void AddCustomBuffToPlayer(Entity characterEntity, string buffId, GlobalStatBuffFlags[] flags = null)
  {
    if (TryGetCustomBuff(buffId, out var customBuff))
    {
      customBuff.ApplyCustomBuff(characterEntity, flags);
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
      if (customBuff.HasBuff(characterEntity))
      {
        customBuff.RemoveBuff(characterEntity);
        return true;
      }
    }

    return false;
  }

  public static bool TryGetCustomBuff(string buffId, out ICustomBuff customBuff)
  {
    return AvailableBuffs.TryGetValue(buffId, out customBuff);
  }

  // we want a function that gets a list of players that have the hide nameplate buff within a certain radius
  public static List<Entity> NearbyPlayersHaveHideNameplateBuff(Entity playerEntity, float radius = 10f)
  {
    List<Entity> nearbyUserEntities = EntityService.GetNearbyUserEntities(playerEntity, radius);
    List<Entity> playersWithBuff = new List<Entity>();
    foreach (var entity in nearbyUserEntities)
    {
      // we need to get the actual character entity from the user component
      if (!EntityManager.HasComponent<User>(entity))
      {
        continue;
      }

      User userData = EntityManager.GetComponentData<User>(entity);
      Entity characterEntity = userData.LocalCharacter._Entity;

      if (HasHideNameplateBuff(characterEntity))
      {
        playersWithBuff.Add(characterEntity);
      }
    }

    return playersWithBuff;
  }

  public static void MakeShapeshiftBuffStayOnDamage(Entity buffEntity)
  {
    if (EntityManager.TryGetComponentData<Script_Buff_Shapeshift_DataShared>(buffEntity, out var shapeshiftData))
    {
      shapeshiftData.RemoveOnDamageTaken = false; // change to not remove on damage taken
      EntityManager.SetComponentData(buffEntity, shapeshiftData);
    }
  }

  public static void MakeShapeshiftBuffRemoveOnDamage(Entity buffEntity)
  {
    if (EntityManager.TryGetComponentData<Script_Buff_Shapeshift_DataShared>(buffEntity, out var shapeshiftData))
    {
      shapeshiftData.RemoveOnDamageTaken = true; // change to remove on damage taken
      EntityManager.SetComponentData(buffEntity, shapeshiftData);
    }
  }

  public static void RemoveBatForm(Entity characterEntity)
  {
    if (HasBuff(characterEntity, PrefabGUIDs.AB_Shapeshift_Bat_TakeFlight_Buff))
    {
      ApplyBuff(characterEntity, PrefabGUIDs.AB_Shapeshift_Bat_Landing_Travel_End);
      RemoveBuff(characterEntity, PrefabGUIDs.AB_Shapeshift_Bat_TakeFlight_Buff);
    }
  }

  public static IEnumerator TurnSpidersIntoBushes(Entity characterEntity)
  {
    // we want to get all spiders around the player
    var spiderEntities = EntityService.GetNearbySpiders(characterEntity, 50f);
    foreach (var spiderEntity in spiderEntities)
    {
      // we want to apply the bush buff to the spider
      if (!HasBuff(spiderEntity, PrefabGUIDs.AB_Blackfang_Ambush_Buff))
      {
        ApplyPermanentBuff(spiderEntity, PrefabGUIDs.AB_Blackfang_Ambush_Buff);
      }
    }
    yield return null;
  }

  public static void TurnSpidersIntoBushesBatch(float radius = 50f)
  {
    var spiderQuery = QueryService.SpidersQuery;
    if (spiderQuery.CalculateEntityCount() == 0)
    {
      return;
    }

    var radiusSqued = radius * radius;
    var spiderKeywords = EntityService.SpiderKeywords;

    NativeArray<Entity> spiderEntites = spiderQuery.ToEntityArray(Allocator.Temp);
    var prefabLookup = EntityManager.GetComponentLookup<PrefabGUID>(true);
    var translonLookup = EntityManager.GetComponentLookup<Translation>(true);

    var onlineUsers = PlayerService.GetUsersOnline();

    var playerPosions = new System.Collections.Generic.List<(float3 position, Entity character)>();
    foreach (var userEntiy in onlineUsers)
    {
      if (EntityManager.Exists(userEntiy))
      {
        var userDta = EntityManager.GetComponentData<User>(userEntiy);
        var charEntiy = userDta.LocalCharacter._Entity;
        if (charEntiy.Exists() && EntityManager.HasComponent<Translation>(charEntiy))
        {
          var pos = EntityManager.GetComponentData<Translation>(charEntiy).Value;
          playerPosions.Add((pos, charEntiy));
        }
      }
    }

    for (int i = 0; i < spiderEntites.Length; i++)
    {
      var spiderEntiy = spiderEntites[i];

      if (PlayerService.ConvertedSpiders.Contains(spiderEntiy))
      {
        continue;
      }

      if (!prefabLookup.HasComponent(spiderEntiy) || !translonLookup.HasComponent(spiderEntiy))
      {
        continue;
      }

      var prefabGuid = prefabLookup[spiderEntiy];
      var prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(prefabGuid);

      bool isSpider = false;
      foreach (var keyword in spiderKeywords)
      {
        if (prefabName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
          isSpider = true;
          break;
        }
      }

      if (!isSpider)
      {
        continue;
      }

      var spiderPos = translonLookup[spiderEntiy].Value;

      bool nearPlayer = false;
      foreach (var player in playerPosions)
      {
        float3 playerPos = player.position;
        float distanceSqued =
          (spiderPos.x - playerPos.x) * (spiderPos.x - playerPos.x) +
          (spiderPos.y - playerPos.y) * (spiderPos.y - playerPos.y) +
          (spiderPos.z - playerPos.z) * (spiderPos.z - playerPos.z);

        if (distanceSqued <= radiusSqued)
        {
          nearPlayer = true;
          break;
        }
      }

      if (nearPlayer && !HasBuff(spiderEntiy, PrefabGUIDs.AB_Blackfang_Ambush_Buff))
      {
        ApplyPermanentBuff(spiderEntiy, PrefabGUIDs.AB_Blackfang_Ambush_Buff);
        PlayerService.ConvertedSpiders.Add(spiderEntiy);
      }
    }

    spiderEntites.Dispose();
  }

  public static void CleanupConvertedSpiders()
  {
    var entitesToRem = new System.Collections.Generic.List<Entity>();
    foreach (var spiderEntiy in PlayerService.ConvertedSpiders)
    {
      if (!EntityManager.Exists(spiderEntiy))
      {
        entitesToRem.Add(spiderEntiy);
      }
    }
    foreach (var entity in entitesToRem)
    {
      PlayerService.ConvertedSpiders.Remove(entity);
    }
  }
}
