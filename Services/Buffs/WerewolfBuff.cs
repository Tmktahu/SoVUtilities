using ProjectM;
using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class WerewolfBuff
{
  public static readonly PrefabGUID WerewolfBuffBase = PrefabGUIDs.Buff_General_Shapeshift_Werewolf_Standard;
  // This buff comes with prebuilt sun immunity. It use to make you twinkle, but no longer does.
  static EntityManager EntityManager => Core.EntityManager;
  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, WerewolfBuffBase);

    if (BuffService.TryGetBuff(targetEntity, WerewolfBuffBase, out var buffEntity))
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

      // Add garlic and silver resistance to the buffer
      // ModifyUnitStatBuff_DOTS newGarlicResStatBuff = new ModifyUnitStatBuff_DOTS
      // {
      //   StatType = UnitStatType.GarlicResistance,
      //   ModificationType = ModificationType.Add,
      //   AttributeCapType = AttributeCapType.Uncapped,
      //   Value = 1000f,
      //   Modifier = 1,
      //   IncreaseByStacks = false,
      //   ValueByStacks = 0,
      //   Priority = 0,
      //   Id = ModificationId.NewId(1000)
      // };
      // buffer.Add(newGarlicResStatBuff);

      ModifyUnitStatBuff_DOTS newPhysicalDamageStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.BonusPhysicalPower,
        ModificationType = ModificationType.Add,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = 500f,
        Modifier = 1,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = ModificationIDs.Create().NewModificationId()
      };
      buffer.Add(newPhysicalDamageStatBuff);

      ModifyUnitStatBuff_DOTS newSilverResStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.SilverResistance,
        ModificationType = ModificationType.Multiply,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = 0f,
        Modifier = 1,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = ModificationIDs.Create().NewModificationId()
      };
      buffer.Add(newSilverResStatBuff);

      // Blood drain buff
      buffEntity.Add<ModifyBloodDrainBuff>();
      var modifyBloodDrainBuff = new ModifyBloodDrainBuff()
      {
        AffectBloodValue = true,
        AffectIdleBloodValue = true,
        BloodValue = 0,
        BloodIdleValue = 0,

        ModificationId = new ModificationId(),
        ModificationIdleId = new ModificationId(),
        IgnoreIdleDrainModId = new ModificationId(),

        ModificationPriority = 1000,
        ModificationIdlePriority = 1000,

        ModificationType = ModificationType.Set,
        ModificationIdleType = ModificationType.Set,

        IgnoreIdleDrainWhileActive = true,
      };
      buffEntity.Write(modifyBloodDrainBuff);

      yield return null;
    }

    yield break;
  }

  public static bool RemoveBuff(Entity entity)
  {
    return BuffService.RemoveBuff(entity, WerewolfBuffBase);
  }

  public static bool HasBuff(Entity entity)
  {
    return BuffService.HasBuff(entity, WerewolfBuffBase);
  }
}