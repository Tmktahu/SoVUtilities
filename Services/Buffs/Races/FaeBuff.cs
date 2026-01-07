using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;
using ProjectM;

namespace SoVUtilities.Services.Buffs;

internal static class FaeBuff
{
  static EntityManager EntityManager => Core.EntityManager;
  public static readonly PrefabGUID FaeBuffPrefabGUID = PrefabGUIDs.EquipBuff_ShroudOfTheForest;

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, FaeBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, FaeBuffPrefabGUID, out Entity buffEntity))
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


      ModifyUnitStatBuff_DOTS newSilverResStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.SilverResistance,
        ModificationType = ModificationType.Add,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = -15f,
        Modifier = 1f,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = Core.ModificationIdGenerator.NewModificationId()
      };
      buffer.Add(newSilverResStatBuff);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, FaeBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, FaeBuffPrefabGUID);

    return hasBuff;
  }
}