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
  public int UnarmedGearScore { get; set; } = -1;
}

internal class PlayerService
{
  readonly Dictionary<FixedString64Bytes, PlayerCacheData> namePlayerCache = [];
  readonly Dictionary<ulong, PlayerCacheData> steamPlayerCache = [];
  readonly Dictionary<NetworkId, PlayerData> idPlayerCache = [];
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

        HandlePlayerGearscore(userEntity);

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

  public void HandlePlayerGearscore(Entity userEntity)
  {
    // so this will be called whenever they swap weapons or change gear?
    // essentially when they flip to unarmed or fishing pole, we want their gearscore to stick
    // that means we'll need to buff them accordingly
    var userData = Core.EntityManager.GetComponentData<User>(userEntity);
    var charEntity = userData.LocalCharacter._Entity;

    // we use our global buff for this
    if (!charEntity.Equals(Entity.Null))
    {
      if (!EntityManager.HasComponent<Equipment>(charEntity))
        return;

      // Core.Log.LogInfo($"PlayerService: Handling gearscore for character entity {charEntity.Index}.");
      var equipment = charEntity.Read<Equipment>();
      string characterName = userData.CharacterName.ToString().ToLower();
      if (namePlayerCache.TryGetValue(characterName, out var playerCacheData))
      {
        if (equipment.WeaponLevel == 0)
        {
          // unarmed or fishing pole
          // in this case we want to update their buff to match their unarmed gearscore
          int unarmedGearScore = playerCacheData.UnarmedGearScore;
          if (unarmedGearScore > 0)
          {
            // apply buff with unarmed gearscore
            // BuffService.UpdateGlobalStatBuff(charEntity, unarmedGearScore);
            equipment.WeaponLevel._Value = unarmedGearScore;
            EntityManager.SetComponentData(charEntity, equipment);
          }
        }
        else
        {
          // armed, we want to keep track of their weapon gearscore for later
          playerCacheData.UnarmedGearScore = (int)Math.Round(equipment.WeaponLevel);
          namePlayerCache[characterName] = playerCacheData;
        }
      }
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
    // Core.Log.LogInfo($"PlayerService: Getting cached online users. Cache size: {namePlayerCache.Count}");
    foreach (var pd in namePlayerCache.Values.ToArray())
    {
      var entity = pd.UserEntity;
      if (Core.EntityManager.Exists(entity) && entity.Read<User>().IsConnected)
        yield return entity;
    }
  }

  public static string GetEquippedWeaponCategory(Entity playerEntity)
  {
    // we get all buffs on the character and loop through them
    var buffEntities = EntityService.GetEntitiesByComponentTypes<Buff, PrefabGUID>();

    foreach (var buffEntity in buffEntities)
    {
      string equippedCategory = null;
      if (buffEntity.Read<EntityOwner>().Owner == playerEntity)
      {
        // we need to get the name of the buff, so we need its prefab GUID
        PrefabGUID buff = Core.EntityManager.GetComponentData<PrefabGUID>(buffEntity);
        string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(buff);

        AbilityService.weaponCategories.ForEach(category =>
        {
          if (prefabName.Contains(category, Il2CppSystem.StringComparison.OrdinalIgnoreCase))
          {
            equippedCategory = category;
            return;
          }
        });
      }

      if (equippedCategory != null)
        return equippedCategory;
    }

    return "Unknown";
  }
}
