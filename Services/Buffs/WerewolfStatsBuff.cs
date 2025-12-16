using ProjectM;
using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class WerewolfStatsBuff
{
  public static readonly PrefabGUID HumanBuffBase = PrefabGUIDs.SetBonus_Silk_Twilight;
  static readonly PrefabGUID wolfFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Buff;
  static readonly PrefabGUID wolfFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff;
  static readonly PrefabGUID wolfFormSkin2PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff;
  static readonly PrefabGUID wolfFormSkin3PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff;

  static EntityManager EntityManager => Core.EntityManager;
  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, HumanBuffBase); // human buff handles the sunlight and stat changes

    if (BuffService.TryGetBuff(targetEntity, HumanBuffBase, out var buffEntity))
    {
      BuffService.SetupSyncBuffer(buffEntity, targetEntity);

      // First check if the buff entity exists
      if (!buffEntity.Exists())
      {
        Core.Log.LogError($"[WerewolfBuff.ApplyCustomBuffStats] - Buff entity {buffEntity} does not exist!");
        yield break;
      }

      if (!buffEntity.TryGetBuffer<ModifyUnitStatBuff_DOTS>(out var buffer))
      {
        buffer = EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
      }

      buffer.Clear();

      BuffService.makeBuffPermanent(buffEntity);

      // if they have the werewolf model buff, they are in werewolf form
      // if they have any of the wolf buffs, they are in wolf form
      // otherwise they are in human form

      // silver resistance set to 0
      ModifyUnitStatBuff_DOTS newSilverResStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.SilverResistance,
        ModificationType = ModificationType.Set,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = 0f,
        Modifier = 1,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = Core.ModificationIdGenerator.NewModificationId()
      };
      buffer.Add(newSilverResStatBuff);

      if (BuffService.HasBuff(targetEntity, WerewolfBuff.WerewolfBuffBase))
      {
        // physical damage +30
        ModifyUnitStatBuff_DOTS newPhysicalDamageStatBuff = new ModifyUnitStatBuff_DOTS
        {
          StatType = UnitStatType.PhysicalPower,
          ModificationType = ModificationType.Add,
          AttributeCapType = AttributeCapType.Uncapped,
          Value = 30f,
          Modifier = 1,
          IncreaseByStacks = false,
          ValueByStacks = 0,
          Priority = 0,
          Id = Core.ModificationIdGenerator.NewModificationId()
        };
        buffer.Add(newPhysicalDamageStatBuff);

        // attack speed +40%
        ModifyUnitStatBuff_DOTS newAttackSpeedStatBuff = new ModifyUnitStatBuff_DOTS
        {
          StatType = UnitStatType.PrimaryAttackSpeed,
          ModificationType = ModificationType.Multiply,
          AttributeCapType = AttributeCapType.Uncapped,
          Value = 1.4f,
          Modifier = 1,
          IncreaseByStacks = false,
          ValueByStacks = 0,
          Priority = 0,
          Id = Core.ModificationIdGenerator.NewModificationId()
        };
        buffer.Add(newAttackSpeedStatBuff);

        // max health +75%
        ModifyUnitStatBuff_DOTS newHealthStatBuff = new ModifyUnitStatBuff_DOTS
        {
          StatType = UnitStatType.MaxHealth,
          ModificationType = ModificationType.Multiply,
          AttributeCapType = AttributeCapType.Uncapped,
          Value = 1.75f,
          Modifier = 1,
          IncreaseByStacks = false,
          ValueByStacks = 0,
          Priority = 0,
          Id = Core.ModificationIdGenerator.NewModificationId()
        };
        buffer.Add(newHealthStatBuff);

        // movement speed +20%
        ModifyUnitStatBuff_DOTS speedBuff = new ModifyUnitStatBuff_DOTS
        {
          StatType = UnitStatType.MovementSpeed,
          ModificationType = ModificationType.Multiply,
          AttributeCapType = AttributeCapType.Uncapped,
          Value = 1.2f,
          Modifier = 1,
          IncreaseByStacks = false,
          ValueByStacks = 0,
          Priority = 0,
          Id = Core.ModificationIdGenerator.NewModificationId()
        };
        buffer.Add(speedBuff);
      }
      else if (BuffService.HasBuff(targetEntity, wolfFormPrefabGUID) || BuffService.HasBuff(targetEntity, wolfFormSkin1PrefabGUID) || BuffService.HasBuff(targetEntity, wolfFormSkin2PrefabGUID) || BuffService.HasBuff(targetEntity, wolfFormSkin3PrefabGUID))
      {
        // movement speed +20%
        ModifyUnitStatBuff_DOTS speedBuff = new ModifyUnitStatBuff_DOTS
        {
          StatType = UnitStatType.BonusShapeshiftMovementSpeed,
          ModificationType = ModificationType.Set,
          AttributeCapType = AttributeCapType.Uncapped,
          Value = 0.78f, // this gets it to horse target of 11.6 speed
          Modifier = 1,
          Id = Core.ModificationIdGenerator.NewModificationId()
        };
        buffer.Add(speedBuff);
      }
      else
      {
        // Ensure only human form buffs are applied
        // ModifyUnitStatBuff_DOTS speedBuff = new ModifyUnitStatBuff_DOTS
        // {
        //   StatType = UnitStatType.MovementSpeed,
        //   ModificationType = ModificationType.Add,
        //   AttributeCapType = AttributeCapType.Uncapped,
        //   Value = 5f,
        //   Modifier = 1,
        //   IncreaseByStacks = false,
        //   ValueByStacks = 0,
        //   Priority = 10,
        //   Id = Core.ModificationIdGenerator.NewModificationId()
        // };
        // buffer.Add(speedBuff);
      }

      // Blood drain buff
      // this needs to happen at the end because adding it marks the buff as 'complete' and we can't add more ModifyUnitStatBuff_DOTS after that
      buffEntity.Add<ModifyBloodDrainBuff>();
      var modifyBloodDrainBuff = new ModifyBloodDrainBuff()
      {
        AffectBloodValue = true,
        AffectIdleBloodValue = true,
        BloodValue = 0,
        BloodIdleValue = 0,

        ModificationId = Core.ModificationIdGenerator.NewModificationId(),
        ModificationIdleId = Core.ModificationIdGenerator.NewModificationId(),
        IgnoreIdleDrainModId = Core.ModificationIdGenerator.NewModificationId(),

        ModificationPriority = 1000,
        ModificationIdlePriority = 1000,

        ModificationType = ModificationType.Set,
        ModificationIdleType = ModificationType.Set,

        IgnoreIdleDrainWhileActive = true,
      };
      buffEntity.Write(modifyBloodDrainBuff);

      yield break;
    }
  }

  public static IEnumerator ReapplyCustomBuff(Entity targetEntity)
  {
    if (HasBuff(targetEntity))
    {
      RemoveBuff(targetEntity);
    }

    yield return new WaitForSeconds(1f);

    yield return ApplyCustomBuff(targetEntity);
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, HumanBuffBase);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, HumanBuffBase);

    return hasBuff;
  }
}