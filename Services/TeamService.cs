using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Gameplay.Systems;
using ProjectM.Scripting;
using Stunlock.Core;
using System.Collections.Concurrent;
using Unity.Collections;
using Unity.Entities;
using static SoVUtilities.Services.BuffService;
using SoVUtilities.Resources;
using UnityEngine;
using BepInEx.Unity.IL2CPP;

namespace SoVUtilities.Services;

public class TeamService
{
  static EntityManager EntityManager => Core.EntityManager;
  static ServerGameManager ServerGameManager => Core.ServerGameManager;

  public static Dictionary<string, PrefabGUID> FactionTagToPrefabGUID = new Dictionary<string, PrefabGUID>
  {
    { TagService.Tags.FACTION_NOCTUM, PrefabGUIDs.Faction_Legion },
    { TagService.Tags.FACTION_CULT, PrefabGUIDs.Faction_Undead }
  };

  public static void ResetTeam(Entity targetEntity)
  {
    // so the original faction exists in FactionReference component
    if (EntityManager.HasComponent<FactionReference>(targetEntity))
    {
      // Reset the faction reference
      FactionReference factionReference = EntityManager.GetComponentData<FactionReference>(targetEntity);
      PrefabGUID originalFactionPrefabGuid = factionReference.FactionGuid;

      var playerData = PlayerDataService.GetPlayerData(targetEntity);
      if (playerData.HasTag(TagService.Tags.FACTION_NOCTUM))
      {
        originalFactionPrefabGuid = FactionTagToPrefabGUID[TagService.Tags.FACTION_NOCTUM];
      }

      if (playerData.HasTag(TagService.Tags.FACTION_CULT))
      {
        originalFactionPrefabGuid = FactionTagToPrefabGUID[TagService.Tags.FACTION_CULT];
      }

      // Reset the faction index
      int originalFactionIndex = GetFactionIndexByPrefabGUID(originalFactionPrefabGuid);
      Core.Log.LogInfo($"Resetting faction for entity {targetEntity} to original faction index {originalFactionIndex} ({originalFactionPrefabGuid})");
      SetFactionIndex(targetEntity, originalFactionIndex);
      EntityService.ResetNearbyAggro(targetEntity);
    }
  }

  public static void SetFaction(Entity targetEntity, PrefabGUID factionPrefabGuid, bool friendlyFireEnabled = true)
  {
    if (friendlyFireEnabled)
    {
      // switch faction while allowing friendly fire
      int factionIndex = GetFactionIndexByPrefabGUID(factionPrefabGuid);
      SetFactionIndex(targetEntity, factionIndex);
    }
    else
    {
      // switch faction without allowing friendly fire
      // TODO
    }
  }

  public static void SetToPlayerFaction(Entity targetEntity, bool friendlyFireEnabled = true)
  {
    SetFaction(targetEntity, PrefabGUIDs.Faction_Players, friendlyFireEnabled);
  }

  public static void SetFactionIndex(Entity entity, int factionIndex)
  {
    if (EntityManager.HasComponent<Team>(entity))
    {
      var team = EntityManager.GetComponentData<Team>(entity);
      team.FactionIndex = factionIndex;
      Core.Log.LogInfo($"Setting faction index for entity {entity} to {factionIndex}");
      EntityManager.SetComponentData(entity, team);
    }
    else
    {
      Core.Log.LogWarning($"Entity {entity.Index}.{entity.Version} does not have a Team component.");
    }
  }

  public void SetFactionIndex(EntityManager entityManager, Entity entity, int factionIndex)
  {
    if (entityManager.HasComponent<Team>(entity))
    {
      var team = entityManager.GetComponentData<Team>(entity);
      team.FactionIndex = factionIndex;
      entityManager.SetComponentData(entity, team);
    }
    else
    {
      Core.Log.LogWarning($"Entity {entity.Index}.{entity.Version} does not have a Team component.");
    }
  }

  public static int GetFactionIndexByPrefabGUID(PrefabGUID prefabGuid)
  {
    int factionIndex = -1;

    FactionLookupSystem factionLookupSystem = Core.FactionLookupSystem;
    FactionLookupSingleton factionLookupSingleton = factionLookupSystem.GetSingleton<FactionLookupSingleton>();

    for (int index = 0; index < factionLookupSingleton.FactionPrefabEntityLookup.Length; index++)
    {
      Entity entity = factionLookupSingleton.FactionPrefabEntityLookup[index];
      PrefabGUID entityPrefabGuid = entity.GetPrefabGuid();

      if (entityPrefabGuid.Equals(prefabGuid))
      {
        factionIndex = index;
        break;
      }
    }

    if (factionIndex == -1)
    {
      Core.Log.LogWarning($"[TeamService] Faction with PrefabGUID {prefabGuid} not found in FactionLookupSingleton.");
    }

    return factionIndex;
  }

  public static PrefabGUID GetFactionPrefabGUIDByIndex(int factionIndex)
  {
    FactionLookupSystem factionLookupSystem = Core.FactionLookupSystem;
    FactionLookupSingleton factionLookupSingleton = factionLookupSystem.GetSingleton<FactionLookupSingleton>();

    Entity entity;
    factionLookupSingleton.TryGetPrefabEntity(factionIndex, out entity);

    PrefabGUID prefabGUID = entity.GetPrefabGuid();

    return prefabGUID;
  }
}