using ProjectM;
using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class HumanBuff
{
  public static readonly PrefabGUID HumanBuffBase = PrefabGUIDs.SetBonus_Silk_Twilight;
  // This buff comes with prebuilt sun immunity. It use to make you twinkle, but no longer does.
  static EntityManager EntityManager => Core.EntityManager;
  public static bool ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, HumanBuffBase);

    if (BuffService.TryGetBuff(targetEntity, HumanBuffBase, out var buffEntity))
    {
      BuffService.SetupSyncBuffer(buffEntity, targetEntity);

      // Apply the custom buff stats to the buff entity
      // customBuff.ApplyCustomBuffStats(buffEntity, targetEntity);

      // First check if the buff entity exists
      if (!buffEntity.Exists())
      {
        Core.Log.LogError($"[HumanBuff.ApplyCustomBuffStats] - Buff entity {buffEntity} does not exist!");
        return false;
      }

      if (!buffEntity.TryGetBuffer<ModifyUnitStatBuff_DOTS>(out var buffer))
      {
        // Core.Log.LogInfo($"Creating new stat buffer for garlic resistance on {buffEntity}");
        buffer = EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
      }

      BuffService.makeBuffPermanent(buffEntity);

      // Core.Log.LogInfo($"[HumanBuff.ApplyCustomBuffStats] - Applying Garlic and Silver resistance to buff {buffEntity}");
      // Add garlic and silver resistance to the buffer
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
      // Core.Log.LogInfo($"[HumanBuff.ApplyCustomBuffStats] - Added Garlic Resistance: {newGarlicResStatBuff}");

      ModifyUnitStatBuff_DOTS newSilverResStatBuff = new ModifyUnitStatBuff_DOTS
      {
        StatType = UnitStatType.SilverResistance,
        ModificationType = ModificationType.Add,
        AttributeCapType = AttributeCapType.Uncapped,
        Value = 1000f,
        Modifier = 1,
        IncreaseByStacks = false,
        ValueByStacks = 0,
        Priority = 0,
        Id = ModificationIDs.Create().NewModificationId()
      };
      buffer.Add(newSilverResStatBuff);
      // Core.Log.LogInfo($"[HumanBuff.ApplyCustomBuffStats] - Added Silver Resistance: {newSilverResStatBuff}");
      return true;
    }

    Core.Log.LogError($"[HumanBuff.ApplyCustomBuff] - Failed to get buff entity for {HumanBuffBase} on player {targetEntity}");
    return false;
  }

  public static bool RemoveBuff(Entity entity)
  {
    return BuffService.RemoveBuff(entity, HumanBuffBase);
  }

  public static bool HasBuff(Entity entity)
  {
    return BuffService.HasBuff(entity, HumanBuffBase);
  }
}