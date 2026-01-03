using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;
using ProjectM;

namespace SoVUtilities.Services.Buffs;

internal static class RotlingBuff
{
  static EntityManager EntityManager => Core.EntityManager;
  public static readonly PrefabGUID RotlingBuffPrefabGUID = PrefabGUIDs.Buff_General_ChanceToCorruptBlood; // 1524978405
  // public static readonly PrefabGUID RotlingBuffPrefabGUID2 = PrefabGUIDs.AB_Nun_AoE_Light_Buff; // -1466712470

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, RotlingBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, RotlingBuffPrefabGUID, out Entity buffEntity))
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


      ModifyUnitStatBuff_DOTS newFireResStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.FireResistance,
        ModificationType = ModificationType.Add,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = -15f,
        Modifier = 1f,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = Core.ModificationIdGenerator.NewModificationId()
      };
      buffer.Add(newFireResStatBuff);

      ModifyUnitStatBuff_DOTS newSunResStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.SunResistance,
        ModificationType = ModificationType.Add,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = 25f,
        Modifier = 1f,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = Core.ModificationIdGenerator.NewModificationId()
      };
      buffer.Add(newSunResStatBuff);
    }

    // yield return new WaitForSeconds(0.1f);

    // BuffService.ApplyBuff(targetEntity, AfflictedBuffPrefabGUID2);

    // if (BuffService.TryGetBuff(targetEntity, AfflictedBuffPrefabGUID2, out Entity buff2Entity))
    // {
    //   BuffService.makeBuffPermanent(buff2Entity);
    // }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, RotlingBuffPrefabGUID);
    // removed |= BuffService.RemoveBuff(entity, AfflictedBuffPrefabGUID2);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, RotlingBuffPrefabGUID);
    // hasBuff |= BuffService.HasBuff(entity, AfflictedBuffPrefabGUID2);

    return hasBuff;
  }
}