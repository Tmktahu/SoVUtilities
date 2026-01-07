using System.Collections;
using System.Linq;
using Il2CppInterop.Runtime;
using SoVUtilities.Models;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Services;

public struct PlayerCacheData(FixedString64Bytes characterName = default, ulong steamID = 0, bool isOnline = false, Entity userEntity = default, Entity charEntity = default)
{
  public FixedString64Bytes CharacterName { get; set; } = characterName;
  public ulong SteamID { get; set; } = steamID;
  public bool IsOnline { get; set; } = isOnline;
  public Entity UserEntity { get; set; } = userEntity;
  public Entity CharEntity { get; set; } = charEntity;
}


internal class PlayerService
{
  readonly Dictionary<FixedString64Bytes, PlayerCacheData> namePlayerCache = [];
  readonly Dictionary<ulong, PlayerCacheData> steamPlayerCache = [];
  readonly Dictionary<NetworkId, PlayerData> idPlayerCache = [];
  readonly HashSet<string> playersUnderCorrection = new();
  readonly EntityManager EntityManager = Core.EntityManager;

  internal bool TryFindSteam(ulong steamId, out PlayerCacheData playerData)
  {
    return steamPlayerCache.TryGetValue(steamId, out playerData);
  }

  internal bool TryFindName(FixedString64Bytes name, out PlayerCacheData playerData)
  {
    return namePlayerCache.TryGetValue(name, out playerData);
  }

  internal PlayerService()
  {
    namePlayerCache.Clear();
    steamPlayerCache.Clear();

    var userEntities = EntityService.GetEntitiesByComponentType<User>(includeDisabled: true);
    foreach (var entity in userEntities)
    {
      var userData = Core.EntityManager.GetComponentData<User>(entity);
      var playerData = new PlayerCacheData(userData.CharacterName, userData.PlatformId, userData.IsConnected, entity, userData.LocalCharacter._Entity);

      namePlayerCache.TryAdd(userData.CharacterName.ToString().ToLower(), playerData);
      steamPlayerCache.TryAdd(userData.PlatformId, playerData);

      // var charEntity = userData.LocalCharacter.GetEntityOnServer();
      // if (!charEntity.Equals(Entity.Null) &&
      // 	Core.ConfigSettings.EveryoneDaywalker ^ Core.BoostedPlayerService.IsDaywalker(charEntity))
      // {
      // 	Core.BoostedPlayerService.ToggleDaywalker(charEntity);
      // 	Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
      // }
    }


    // var onlinePlayers = namePlayerCache.Values.Where(p => p.IsOnline).Select(p => $"\t{p.CharacterName}");
    // Core.Log.LogWarning($"Player Cache Created with {namePlayerCache.Count} entries total, listing {onlinePlayers.Count()} online:");
    // Core.Log.LogWarning(string.Join("\n", onlinePlayers));
    Core.StartCoroutine(PlayerLoop());
  }

  internal void UpdatePlayerCache(Entity userEntity, string oldName, string newName, bool forceOffline = false)
  {
    var userData = Core.EntityManager.GetComponentData<User>(userEntity);
    namePlayerCache.Remove(oldName.ToLower());

    if (forceOffline) userData.IsConnected = false;
    var playerData = new PlayerCacheData(newName, userData.PlatformId, userData.IsConnected, userEntity, userData.LocalCharacter._Entity);

    namePlayerCache[newName.ToLower()] = playerData;
    steamPlayerCache[userData.PlatformId] = playerData;
    // idPlayerCache[userEntity.Read<NetworkId>()] = playerData;
  }

  IEnumerator PlayerLoop()
  {
    while (true)
    {
      // handling for our global stats buff
      foreach (var userEntity in GetUsersOnline())
      {
        var userData = Core.EntityManager.GetComponentData<User>(userEntity);
        var charEntity = userData.LocalCharacter._Entity;

        // if (!charEntity.Equals(Entity.Null))
        // {
        //   if (!BuffService.HasBuff(charEntity, BuffService.globalStatsBuff))
        //   {
        //     BuffService.ApplyPermanentBuff(charEntity, BuffService.globalStatsBuff);
        //   }
        // }

        // for regional buffs and whatnot
        if (Core.RegionService != null)
        {
          Core.RegionService.CheckPlayerRegions(userEntity);
        }
      }

      if (Core.RegionService != null)
      {
        Core.RegionService.ResetForceUpdateFlag();
      }

      yield return null;
    }
  }

  // internal void UpdatePlayerCache(Entity userEntity, string oldName, string newName, bool forceOffline = false)
  // {
  // 	var userData = Core.EntityManager.GetComponentData<User>(userEntity);
  // 	namePlayerCache.Remove(oldName.ToLower());

  // 	if (forceOffline) userData.IsConnected = false;
  // 	var playerData = new PlayerData(newName, userData.PlatformId, userData.IsConnected, userEntity, userData.LocalCharacter._Entity);

  // 	namePlayerCache[newName.ToLower()] = playerData;
  // 	steamPlayerCache[userData.PlatformId] = playerData;
  // 	idPlayerCache[userEntity.Read<NetworkId>()] = playerData;
  // }

  // public bool TryFindUserFromNetworkId(NetworkId networkId, out Entity userEntity)
  // {
  // 	if(idPlayerCache.TryGetValue(networkId, out var playerData))
  // 	{
  // 		userEntity = playerData.UserEntity;
  // 		return true;
  // 	}
  // 	userEntity = Entity.Null;
  // 	return false;
  // }

  // internal bool RenamePlayer(Entity userEntity, Entity charEntity, FixedString64Bytes newName)
  // {
  // 	var des = Core.Server.GetExistingSystemManaged<DebugEventsSystem>();
  // 	var networkId = Core.EntityManager.GetComponentData<NetworkId>(userEntity);
  // 	var userData = Core.EntityManager.GetComponentData<User>(userEntity);
  // 	var renameEvent = new RenameUserDebugEvent
  // 	{
  // 		NewName = newName,
  // 		Target = networkId
  // 	};
  // 	var fromCharacter = new FromCharacter
  // 	{
  // 		User = userEntity,
  // 		Character = charEntity
  // 	};

  // 	des.RenameUser(fromCharacter, renameEvent);
  // 	UpdatePlayerCache(userEntity, userData.CharacterName.ToString(), newName.ToString());

  // 	var attachedBuffer = Core.EntityManager.GetBuffer<AttachedBuffer>(charEntity);
  // 	foreach(var entry in attachedBuffer)
  // 	{
  // 		if (entry.PrefabGuid.GuidHash != -892362184) continue;
  // 		var pmi = entry.Entity.Read<PlayerMapIcon>();
  // 		pmi.UserName = newName;
  // 		entry.Entity.Write(pmi);
  // 	}

  // 	Core.Log.LogInfo($"Player {userData.CharacterName} renamed to {newName}");

  // 	return true;
  // }
  public static IEnumerable<Entity> GetUsersOnline()
  {
    var entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
      .AddAll(new(Il2CppType.Of<User>(), ComponentType.AccessMode.ReadOnly));
    NativeArray<Entity> _userEntities = Core.EntityManager.CreateEntityQuery(ref entityQueryBuilder).ToEntityArray(Allocator.Temp);
    entityQueryBuilder.Dispose();
    foreach (var entity in _userEntities)
    {
      if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
        yield return entity;
    }
    _userEntities.Dispose();
  }

  public IEnumerable<Entity> GetCachedUsersOnline()
  {
    foreach (var pd in namePlayerCache.Values.ToArray())
    {
      var entity = pd.UserEntity;
      if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
        yield return entity;
    }
  }

  public static Entity GetEquippedWeaponBuffEntity(Entity playerEntity)
  {
    // we get all buffs on the character and loop through them
    var buffEntities = EntityService.GetEntitiesByComponentTypes<Buff, PrefabGUID>();

    foreach (var buffEntity in buffEntities)
    {
      if (buffEntity.Read<EntityOwner>().Owner == playerEntity)
      {
        // we need to get the name of the buff, so we need its prefab GUID
        PrefabGUID buff = Core.EntityManager.GetComponentData<PrefabGUID>(buffEntity);
        string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(buff);

        foreach (var category in AbilityService.weaponCategories)
        {
          if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase))
          {
            return buffEntity;
          }
        }
      }
    }

    return Entity.Null;
  }

  public static PrefabGUID GetEquipBuffPrefabGUID(Entity playerEntity)
  {
    // we get all buffs on the character and loop through them
    var buffEntities = EntityService.GetEntitiesByComponentTypes<Buff, PrefabGUID>();

    foreach (var buffEntity in buffEntities)
    {
      PrefabGUID equippedWeaponPrefabGUID = default;
      if (buffEntity.Read<EntityOwner>().Owner == playerEntity)
      {
        // we need to get the name of the buff, so we need its prefab GUID
        PrefabGUID buff = Core.EntityManager.GetComponentData<PrefabGUID>(buffEntity);
        string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(buff);

        foreach (var category in AbilityService.weaponCategories)
        {
          if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase))
          {
            equippedWeaponPrefabGUID = buff;
            break;
          }
        }
      }

      if (!equippedWeaponPrefabGUID.IsEmpty())
        return equippedWeaponPrefabGUID;
    }

    return default;
  }

  public static string GetEquippedWeaponCategory(Entity playerEntity)
  {
    PrefabGUID equipBuffPrefabGUID = GetEquipBuffPrefabGUID(playerEntity);
    if (!equipBuffPrefabGUID.IsEmpty())
    {
      string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(equipBuffPrefabGUID);
      foreach (var category in AbilityService.weaponCategories)
      {
        if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase))
        {
          return category;
        }
      }
    }

    return "Unknown";
  }
}
