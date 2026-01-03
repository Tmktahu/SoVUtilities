using ProjectM;
using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class GlobalStatsBuff
{
  public static readonly PrefabGUID GlobalStatsBuffBase = PrefabGUIDs.SetBonus_AllLeech_T09;
  static readonly PrefabGUID wolfFormPrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Buff;
  static readonly PrefabGUID wolfFormSkin1PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff;
  static readonly PrefabGUID wolfFormSkin2PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff;
  static readonly PrefabGUID wolfFormSkin3PrefabGUID = PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff;

  static EntityManager EntityManager => Core.EntityManager;
  public static IEnumerator ApplyCustomBuff(Entity targetEntity, GlobalStatBuffFlags[] flags = null)
  {
    yield return null;
    // BuffService.ApplyBuff(targetEntity, GlobalStatsBuffBase); // human buff handles the sunlight and stat changes

    // if (BuffService.TryGetBuff(targetEntity, GlobalStatsBuffBase, out var buffEntity))
    // {
    //   BuffService.SetupSyncBuffer(buffEntity, targetEntity);

    //   // First check if the buff entity exists
    //   if (!buffEntity.Exists())
    //   {
    //     Core.Log.LogError($"[WerewolfBuff.ApplyCustomBuffStats] - Buff entity {buffEntity} does not exist!");
    //     yield break;
    //   }

    //   if (!buffEntity.TryGetBuffer<ModifyUnitStatBuff_DOTS>(out var buffer))
    //   {
    //     buffer = EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
    //   }

    //   buffer.Clear();

    //   BuffService.makeBuffPermanent(buffEntity);

    //   // if they have the werewolf model buff, they are in werewolf form
    //   // if they have any of the wolf buffs, they are in wolf form
    //   // otherwise they are in human form

    //   // silver resistance set to 0
    //   // ModifyUnitStatBuff_DOTS newSilverResStatBuff = new ModifyUnitStatBuff_DOTS
    //   // {
    //   //   StatType = UnitStatType.SilverResistance,
    //   //   ModificationType = ModificationType.Set,
    //   //   AttributeCapType = AttributeCapType.Uncapped,
    //   //   Value = 0f,
    //   //   Modifier = 1,
    //   //   IncreaseByStacks = false,
    //   //   ValueByStacks = 0,
    //   //   Priority = 0,
    //   //   Id = Core.ModificationIdGenerator.NewModificationId()
    //   // };
    //   // buffer.Add(newSilverResStatBuff);

    //   // var playerData = PlayerDataService.GetPlayerData(targetEntity);

    //   // if (playerData.HasTag(TagService.Tags.WEREWOLF) && (
    //   //   BuffService.HasBuff(targetEntity, wolfFormPrefabGUID) ||
    //   //   BuffService.HasBuff(targetEntity, wolfFormSkin1PrefabGUID) ||
    //   //   BuffService.HasBuff(targetEntity, wolfFormSkin2PrefabGUID) ||
    //   //   BuffService.HasBuff(targetEntity, wolfFormSkin3PrefabGUID)) &&
    //   //   flags.Contains(GlobalStatBuffFlags.WolfFormSpeedBoost) &&
    //   //   !BuffService.HasBuff(targetEntity, PrefabGUIDs.Buff_InCombat))
    //   // {
    //   //   // werewolf form speed buff
    //   //   ModifyUnitStatBuff_DOTS speedBuff = new ModifyUnitStatBuff_DOTS
    //   //   {
    //   //     StatType = UnitStatType.BonusShapeshiftMovementSpeed,
    //   //     ModificationType = ModificationType.Set,
    //   //     AttributeCapType = AttributeCapType.Uncapped,
    //   //     Value = 0.78f, // this gets it to horse target of 11.6 speed
    //   //     Modifier = 1,
    //   //     Id = Core.ModificationIdGenerator.NewModificationId()
    //   //   };
    //   //   buffer.Add(speedBuff);
    //   // }

    //   yield break;
    // }
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
    removed |= BuffService.RemoveBuff(entity, GlobalStatsBuffBase);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, GlobalStatsBuffBase);

    return hasBuff;
  }
}