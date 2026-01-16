using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class KalldenBuff
{
  public static readonly PrefabGUID KalldenBuffPrefabGUID = PrefabGUIDs.DiminishingReturn_Buff_Freeze;
  public static readonly PrefabGUID KalldenBuffPrefabGUID2 = PrefabGUIDs.DiminishingReturn_Buff_Fear;
  public static readonly PrefabGUID KalldenBuffPrefabGUID3 = PrefabGUIDs.AB_Frost_ColdBlood_Buff;
  public static readonly PrefabGUID KalldenBuffPrefabGUID4 = PrefabGUIDs.Buff_BloodBuff_Brute_Tier4_Proc;

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, KalldenBuffPrefabGUID);
    if (BuffService.TryGetBuff(targetEntity, KalldenBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, KalldenBuffPrefabGUID2);
    if (BuffService.TryGetBuff(targetEntity, KalldenBuffPrefabGUID2, out Entity buff2Entity))
    {
      BuffService.makeBuffPermanent(buff2Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, KalldenBuffPrefabGUID3);
    if (BuffService.TryGetBuff(targetEntity, KalldenBuffPrefabGUID3, out Entity buff3Entity))
    {
      BuffService.makeBuffPermanent(buff3Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, KalldenBuffPrefabGUID4);
    if (BuffService.TryGetBuff(targetEntity, KalldenBuffPrefabGUID4, out Entity buff4Entity))
    {
      BuffService.makeBuffPermanent(buff4Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, KalldenBuffPrefabGUID);
    removed |= BuffService.RemoveBuff(entity, KalldenBuffPrefabGUID2);
    removed |= BuffService.RemoveBuff(entity, KalldenBuffPrefabGUID3);
    removed |= BuffService.RemoveBuff(entity, KalldenBuffPrefabGUID4);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, KalldenBuffPrefabGUID);
    hasBuff |= BuffService.HasBuff(entity, KalldenBuffPrefabGUID2);
    hasBuff |= BuffService.HasBuff(entity, KalldenBuffPrefabGUID3);
    hasBuff |= BuffService.HasBuff(entity, KalldenBuffPrefabGUID4);

    return hasBuff;
  }
}