using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;
using ProjectM;

namespace SoVUtilities.Services.Buffs;

internal static class DaemonBuff
{
  static EntityManager EntityManager => Core.EntityManager;
  public static readonly PrefabGUID DaemonBuffPrefabGUID = PrefabGUIDs.SetBonus_AllLeech_T09;

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, DaemonBuffPrefabGUID);
    if (BuffService.TryGetBuff(targetEntity, DaemonBuffPrefabGUID, out Entity buffEntity))
    {
      BuffService.SetupSyncBuffer(buffEntity, targetEntity);

      // Apply the custom buff stats to the buff entity
      // customBuff.ApplyCustomBuffStats(buffEntity, targetEntity);

      // First check if the buff entity exists
      if (!buffEntity.Exists())
      {
        Core.Log.LogError($"[HumanBuff.ApplyCustomBuffStats] - Buff entity {buffEntity} does not exist!");
        yield break;
      }

      if (!buffEntity.TryGetBuffer<ModifyUnitStatBuff_DOTS>(out var buffer))
      {
        buffer = EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
      }

      BuffService.makeBuffPermanent(buffEntity);

      ModifyUnitStatBuff_DOTS newGarlicResStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.GarlicResistance,
        ModificationType = ModificationType.Add,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = 1000f,
        Modifier = 1,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = ModificationId.NewId(1000)
      };
      buffer.Add(newGarlicResStatBuff);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, DaemonBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, DaemonBuffPrefabGUID);

    return hasBuff;
  }
}