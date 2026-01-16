using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using SoVUtilities.Models;
using SoVUtilities.Resources;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class DeathEventPatch
{
  [HarmonyPatch(typeof(DeathEventListenerSystem), nameof(DeathEventListenerSystem.OnUpdate))]
  [HarmonyPostfix]
  static void OnUpdatePostfix(DeathEventListenerSystem __instance)
  {
    if (!Core._initialized) return;

    NativeArray<DeathEvent> deathEvents = __instance._DeathEventQuery.ToComponentDataArray<DeathEvent>(Allocator.Temp);

    try
    {
      for (int i = 0; i < deathEvents.Length; i++)
      {
        DeathEvent deathEvent = deathEvents[i];

        if (!ShouldProcessDeath(deathEvent)) continue;

        ProcessDrop(deathEvent);
      }
    }
    finally
    {
      deathEvents.Dispose();
    }
  }

  static bool ShouldProcessDeath(DeathEvent deathEvent)
  {
    if (deathEvent.Killer == deathEvent.Died) return false;

    Entity target = deathEvent.Died;
    Entity killer = deathEvent.Killer;

    if (!IsValidKiller(killer)) return false;
    if (!IsDropSource(target)) return false;

    return true;
  }

  static bool IsValidKiller(Entity entity)
  {
    if (entity.Has<PlayerCharacter>()) return true;

    if (entity.TryGetComponent(out EntityOwner owner) &&
        owner.Owner.Exists() &&
        owner.Owner.Has<PlayerCharacter>())
    {
      return true;
    }

    return false;
  }

  static bool IsDropSource(Entity entity)
  {
    return entity.Has<YieldResourcesOnDamageTaken>() || entity.Has<Movement>();
  }

  static void ProcessDrop(DeathEvent deathEvent)
  {
    Entity player = GetActualPlayer(deathEvent.Killer);

    List<DropRule> drops = GetExtraDrops(deathEvent.Died);
    if (drops == null || drops.Count == 0) return;

    foreach (DropRule drop in drops)
    {
      RollAndDistributeDrop(player, drop);
    }
  }

  static Entity GetActualPlayer(Entity killer)
  {
    if (killer.Has<PlayerCharacter>()) return killer;

    if (killer.TryGetComponent(out EntityOwner owner) &&
        owner.Owner.Has<PlayerCharacter>())
    {
      return owner.Owner;
    }

    return Entity.Null;
  }

  static List<DropRule> GetExtraDrops(Entity target)
  {
    DropSourceInfo sourceInfo = GetSourceInfo(target);

    List<DropRule> matchingRules = new();

    foreach (DropRule rule in ExtraDropConfig.DropRules)
    {
      string[] sources = sourceInfo.Type switch
      {
        "mob" => rule.DroppedByMobs,
        "resource" => rule.DroppedByResources,
        _ => null
      };

      if (sources == null || sources.Length == 0) continue;

      foreach (string sourceEntry in sources)
      {
        if (Matches(sourceEntry, sourceInfo))
        {
          matchingRules.Add(rule);
          break;
        }
      }
    }

    return matchingRules;
  }

  static DropSourceInfo GetSourceInfo(Entity target)
  {
    DropSourceInfo info = new();

    if (target.Has<Movement>())
    {
      info.Type = "mob";
      info.GUID = target.GetPrefabGuid();
      info.Name = PrefabGUIDsExtensions.GetPrefabGUIDName(info.GUID);
    }
    else if (target.Has<YieldResourcesOnDamageTaken>())
    {
      info.Type = "resource";

      if (target.TryGetBuffer(out DynamicBuffer<YieldResourcesOnDamageTaken> yieldBuffer))
      {
        if (yieldBuffer.IsCreated && !yieldBuffer.IsEmpty)
        {
          PrefabGUID itemPrefab = yieldBuffer[0].ItemType;
          info.GUID = itemPrefab;
          info.Name = PrefabGUIDsExtensions.GetPrefabGUIDName(itemPrefab);
        }
      }

      if (string.IsNullOrEmpty(info.Name))
      {
        info.GUID = target.GetPrefabGuid();
        info.Name = PrefabGUIDsExtensions.GetPrefabGUIDName(info.GUID) ?? string.Empty;
      }
    }
    else
    {
      info.Type = "unknown";
      info.GUID = PrefabGUID.Empty;
      info.Name = string.Empty;
    }

    return info;
  }

  static bool Matches(string sourceEntry, DropSourceInfo sourceInfo)
  {
    if (string.IsNullOrWhiteSpace(sourceEntry) || sourceInfo == null)
      return false;

    sourceEntry = sourceEntry.Trim();

    if (TryParsePrefabGUID(sourceEntry, out PrefabGUID entryGUID))
    {
      return sourceInfo.GUID.Equals(entryGUID);
    }
    else
    {
      if (string.IsNullOrEmpty(sourceInfo.Name))
        return false;

      return sourceInfo.Name.Contains(sourceEntry, StringComparison.OrdinalIgnoreCase);
    }
  }

  static bool TryParsePrefabGUID(string input, out PrefabGUID guid)
  {
    guid = PrefabGUID.Empty;

    if (string.IsNullOrWhiteSpace(input))
      return false;

    input = input.Trim();

    if (int.TryParse(input, out int hash))
    {
      guid = new PrefabGUID(hash);
      return true;
    }

    return false;
  }

  static void RollAndDistributeDrop(Entity player, DropRule drop)
  {
    float roll = System.Random.Shared.NextSingle();
    if (roll > drop.DropChance) return;

    int amount = System.Random.Shared.Next(drop.MinAmount, drop.MaxAmount + 1);
    if (amount <= 0) return;

    bool addedToInventory = Core.ServerGameManager.TryAddInventoryItem(player, drop.ItemPrefab, amount);

    if (!addedToInventory)
    {
      InventoryUtilitiesServer.CreateDropItem(
        Core.EntityManager,
        player,
        drop.ItemPrefab,
        amount,
        new Entity()
      );
    }
  }
}

class DropSourceInfo
{
  public string Type { get; set; }
  public PrefabGUID GUID { get; set; }
  public string Name { get; set; }
}
