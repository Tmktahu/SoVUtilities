using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backtrace.Unity.Model;
using ProjectM;
using ProjectM.Network;
using ProjectM.Terrain;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace SoVUtilities.Services;

internal class RegionService
{
  List<WorldRegionType> lockedRegions = [];
  Dictionary<string, int> gatedRegions = [];
  Dictionary<Entity, (WorldRegionType, Vector3)> lastValidPos = [];
  Dictionary<Entity, float> lastSentMessage = [];
  Dictionary<string, int> maxPlayerLevels = [];
  List<string> allowPlayers = [];
  Dictionary<string, List<string>> banPlayers = [];

  // Tracks each player's last known region (non-persistent)
  private readonly Dictionary<Entity, WorldRegionType> _playerLastRegion = new();
  // Flag to force update buffs for all players
  private static bool _forceUpdateAllPlayers = false;

  public IEnumerable<WorldRegionType> LockedRegions => lockedRegions;
  public IEnumerable<KeyValuePair<string, int>> GatedRegions => gatedRegions;
  public IEnumerable<string> AllowedPlayers => allowPlayers;
  public IEnumerable<KeyValuePair<string, List<string>>> BannedPlayers => banPlayers;

  struct RegionPolygon
  {
    public WorldRegionType Region;
    public Aabb Aabb;
    public float2[] Vertices;
  };

  List<RegionPolygon> regionPolygons = new();

  struct RegionFile
  {
    public WorldRegionType[] LockedRegions { get; set; }
    public Dictionary<string, int> GatedRegions { get; set; }
    public Dictionary<string, int> MaxPlayerLevels { get; set; }
    public string[] AllowPlayers { get; set; }
    public Dictionary<string, string[]> BanPlayers { get; set; }
  }

  // mapping of region type to tag
  public static readonly Dictionary<WorldRegionType, string> RegionTypeToTag = new()
  {
    { WorldRegionType.CursedForest, TagService.Tags.CURSED_FOREST },
    { WorldRegionType.DunleyFarmlands, TagService.Tags.DUNLEY_FARMLANDS },
    { WorldRegionType.FarbaneWoods, TagService.Tags.FARBANE_WOODS },
    { WorldRegionType.Gloomrot_North, TagService.Tags.GLOOMROT_NORTH },
    { WorldRegionType.Gloomrot_South, TagService.Tags.GLOOMROT_SOUTH },
    { WorldRegionType.HallowedMountains, TagService.Tags.HALLOWED_MOUNTAINS },
    { WorldRegionType.None, TagService.Tags.NONE_REGION },
    { WorldRegionType.Other, TagService.Tags.OTHER_REGION },
    { WorldRegionType.RuinsOfMortium, TagService.Tags.RUINS_OF_MORTIUM },
    { WorldRegionType.SilverlightHills, TagService.Tags.SILVERLIGHT_HILLS },
    { WorldRegionType.StartCave, TagService.Tags.START_CAVE },
    { WorldRegionType.Strongblade, TagService.Tags.OAKVEIL_FOREST }
  };

  // mapping of tag to region type
  public static readonly Dictionary<string, WorldRegionType> TagToRegionType = RegionTypeToTag.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

  public RegionService()
  {
    // LoadRegions();

    foreach (var worldRegionPolygonEntity in EntityService.GetEntitiesByComponentType<WorldRegionPolygon>(true))
    {
      var wrp = worldRegionPolygonEntity.Read<WorldRegionPolygon>();
      var vertices = Core.EntityManager.GetBuffer<WorldRegionPolygonVertex>(worldRegionPolygonEntity);

      regionPolygons.Add(
        new RegionPolygon
        {
          Region = wrp.WorldRegion,
          Aabb = wrp.PolygonBounds,
          Vertices = vertices.ToNativeArray(allocator: Allocator.Temp).ToArray().Select(x => x.VertexPos).ToArray()
        });
    }

    // Core.StartCoroutine(CheckPlayerRegions());
  }

  // Call this method to trigger a force update on all players
  public void ForceUpdateAllPlayers()
  {
    _forceUpdateAllPlayers = true;
  }

  public void ResetForceUpdateFlag()
  {
    _forceUpdateAllPlayers = false;
  }

  public void CheckPlayerRegions(Entity userEntity)
  {
    if (Core.PlayerService == null)
      return;

    if (_forceUpdateAllPlayers)
    {
      // Remove and re-apply buffs for all players
      // foreach (var userEntity in )
      // {
      if (!userEntity.Has<User>()) return;
      var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();
      if (!charEntity.Has<Equipment>()) return;
      var pos = charEntity.Read<Translation>().Value;
      WorldRegionType currentWorldRegion = GetRegion(pos);

      // Remove all region buffs
      var regionBuffs = ModDataService.GetRegionBuffMapping();
      foreach (var kvp in regionBuffs)
      {
        RemoveRegionBuffs(charEntity, kvp.Value.BuffIds);
      }

      // Apply buffs for current region if enabled
      if (regionBuffs.TryGetValue((int)currentWorldRegion, out RegionBuffConfig buffConfig) && buffConfig.Enabled)
      {
        ApplyRegionBuffs(charEntity, buffConfig.BuffIds);
      }

      // Update last known region
      _playerLastRegion[charEntity] = currentWorldRegion;

      // _forceUpdateAllPlayers = false;
    }
    else
    {
      // foreach (var userEntity in Core.PlayerService.GetCachedUsersOnline())
      // {
      if (!userEntity.Has<User>()) return;
      var charName = userEntity.Read<User>().CharacterName.ToString();
      if (string.IsNullOrEmpty(charName)) return;

      var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();
      if (!charEntity.Has<Equipment>()) return;
      var pos = charEntity.Read<Translation>().Value;
      WorldRegionType currentWorldRegion = GetRegion(pos);

      // Track last known region for this player
      WorldRegionType lastRegion = WorldRegionType.None;
      _playerLastRegion.TryGetValue(charEntity, out lastRegion);

      // Only act if region changed
      if (lastRegion != currentWorldRegion)
      {
        // Remove buffs from previous region if any
        if (lastRegion != WorldRegionType.None)
        {
          var regionBuffs = ModDataService.GetRegionBuffMapping();
          if (regionBuffs.TryGetValue((int)lastRegion, out RegionBuffConfig prevBuffConfig))
          {
            RemoveRegionBuffs(charEntity, prevBuffConfig.BuffIds);
          }
        }

        // Apply buffs for new region if any
        if (currentWorldRegion != WorldRegionType.None)
        {
          var regionBuffs = ModDataService.GetRegionBuffMapping();
          if (regionBuffs.TryGetValue((int)currentWorldRegion, out RegionBuffConfig newBuffConfig))
          {
            if (newBuffConfig.Enabled)
            {
              ApplyRegionBuffs(charEntity, newBuffConfig.BuffIds);
            }
          }
        }

        // Update last known region
        _playerLastRegion[charEntity] = currentWorldRegion;
      }
      // }
    }
  }

  void ApplyRegionBuffs(Entity characterEntity, List<int> buffIds)
  {
    foreach (var buffId in buffIds)
    {
      PrefabGUID currentBuffPrefabGUID = new PrefabGUID(buffId);
      BuffService.ApplyPermanentBuff(characterEntity, currentBuffPrefabGUID);
    }
  }

  void RemoveRegionBuffs(Entity characterEntity, List<int> buffIds)
  {
    foreach (var buffId in buffIds)
    {
      PrefabGUID currentBuffPrefabGUID = new PrefabGUID(buffId);
      BuffService.RemoveBuff(characterEntity, currentBuffPrefabGUID);
    }
  }

  public void AddBuffToRegion(WorldRegionType region, PrefabGUID buffPrefabGuid)
  {
    var regionBuffs = ModDataService.GetRegionBuffMapping();
    if (!regionBuffs.TryGetValue((int)region, out RegionBuffConfig buffConfig))
    {
      buffConfig = new RegionBuffConfig
      {
        Enabled = true,
        BuffIds = new List<int>()
      };
      regionBuffs[(int)region] = buffConfig;
    }

    if (!buffConfig.BuffIds.Contains(buffPrefabGuid._Value))
    {
      buffConfig.BuffIds.Add(buffPrefabGuid._Value);
      ModDataService.SetRegionBuffConfig((int)region, buffConfig);
      ForceUpdateAllPlayers();
    }
  }

  public void RemoveBuffFromRegion(WorldRegionType region, PrefabGUID buffPrefabGuid)
  {
    var regionBuffs = ModDataService.GetRegionBuffMapping();
    if (regionBuffs.TryGetValue((int)region, out RegionBuffConfig buffConfig))
    {
      if (buffConfig.BuffIds.Contains(buffPrefabGuid._Value))
      {
        buffConfig.BuffIds.Remove(buffPrefabGuid._Value);
        ModDataService.SetRegionBuffConfig((int)region, buffConfig);
        ForceUpdateAllPlayers();
      }
    }
  }

  public void EnableRegionBuffs(WorldRegionType region)
  {
    var regionBuffs = ModDataService.GetRegionBuffMapping();
    if (regionBuffs.TryGetValue((int)region, out RegionBuffConfig buffConfig))
    {
      buffConfig.Enabled = true;
      ModDataService.SetRegionBuffConfig((int)region, buffConfig);
      ForceUpdateAllPlayers();
    }
  }

  public void DisableRegionBuffs(WorldRegionType region)
  {
    var regionBuffs = ModDataService.GetRegionBuffMapping();
    if (regionBuffs.TryGetValue((int)region, out RegionBuffConfig buffConfig))
    {
      buffConfig.Enabled = false;
      ModDataService.SetRegionBuffConfig((int)region, buffConfig);
      ForceUpdateAllPlayers();
    }
  }

  public WorldRegionType GetRegion(float3 pos)
  {
    foreach (var worldRegionPolygon in regionPolygons)
    {
      if (worldRegionPolygon.Aabb.Contains(pos))
      {
        if (IsPointInPolygon(worldRegionPolygon.Vertices, pos.xz))
        {
          return worldRegionPolygon.Region;
        }
      }
    }
    return WorldRegionType.None;
  }

  static bool IsPointInPolygon(float2[] polygon, Vector2 point)
  {
    int intersections = 0;
    int vertexCount = polygon.Length;

    for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
    {
      if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
        (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
      {
        intersections++;
      }
    }

    return intersections % 2 != 0;
  }
}
