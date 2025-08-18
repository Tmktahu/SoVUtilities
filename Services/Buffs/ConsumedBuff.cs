using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class ConsumedBuff
{
  public static readonly PrefabGUID EncasedBuffPrefabGUID = PrefabGUIDs.Buff_General_ChanceToCorruptBlood; // 1524978405
  public static readonly PrefabGUID EncasedBuffPrefabGUID2 = PrefabGUIDs.Unholy_Vampire_Buff_Bane; // 1688799287
  public static readonly PrefabGUID EncasedBuffPrefabGUID3 = PrefabGUIDs.AB_Blackfang_Shared_DrinkCorruption_Buff; // -1664518297
  public static readonly PrefabGUID EncasedBuffPrefabGUID4 = PrefabGUIDs.Buff_Militia_InkCrawler_TrailEffect; // -1124645803

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, EncasedBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, EncasedBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, EncasedBuffPrefabGUID2);

    if (BuffService.TryGetBuff(targetEntity, EncasedBuffPrefabGUID2, out Entity buff2Entity))
    {
      BuffService.makeBuffPermanent(buff2Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, EncasedBuffPrefabGUID3);

    if (BuffService.TryGetBuff(targetEntity, EncasedBuffPrefabGUID3, out Entity buff3Entity))
    {
      BuffService.makeBuffPermanent(buff3Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, EncasedBuffPrefabGUID4);

    if (BuffService.TryGetBuff(targetEntity, EncasedBuffPrefabGUID4, out Entity buff4Entity))
    {
      BuffService.makeBuffPermanent(buff4Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, EncasedBuffPrefabGUID);
    removed |= BuffService.RemoveBuff(entity, EncasedBuffPrefabGUID2);
    removed |= BuffService.RemoveBuff(entity, EncasedBuffPrefabGUID3);
    removed |= BuffService.RemoveBuff(entity, EncasedBuffPrefabGUID4);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, EncasedBuffPrefabGUID);
    hasBuff |= BuffService.HasBuff(entity, EncasedBuffPrefabGUID2);
    hasBuff |= BuffService.HasBuff(entity, EncasedBuffPrefabGUID3);
    hasBuff |= BuffService.HasBuff(entity, EncasedBuffPrefabGUID4);

    return hasBuff;
  }
}