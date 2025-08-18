using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class BeholdenBuff
{
  public static readonly PrefabGUID BeholdenBuffPrefabGUID = PrefabGUIDs.Buff_General_ChanceToCorruptBlood; // 1524978405

  public static readonly PrefabGUID BeholdenBuffPrefabGUID2 = PrefabGUIDs.Unholy_Vampire_Buff_Bane; // 1688799287
  public static readonly PrefabGUID BeholdenBuffPrefabGUID3 = PrefabGUIDs.AB_Nun_AoE_Light_Buff; // -1466712470

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, BeholdenBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, BeholdenBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, BeholdenBuffPrefabGUID2);

    if (BuffService.TryGetBuff(targetEntity, BeholdenBuffPrefabGUID2, out Entity buff2Entity))
    {
      BuffService.makeBuffPermanent(buff2Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, BeholdenBuffPrefabGUID3);

    if (BuffService.TryGetBuff(targetEntity, BeholdenBuffPrefabGUID3, out Entity buff3Entity))
    {
      BuffService.makeBuffPermanent(buff3Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, BeholdenBuffPrefabGUID);
    removed |= BuffService.RemoveBuff(entity, BeholdenBuffPrefabGUID2);
    removed |= BuffService.RemoveBuff(entity, BeholdenBuffPrefabGUID3);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, BeholdenBuffPrefabGUID);
    hasBuff |= BuffService.HasBuff(entity, BeholdenBuffPrefabGUID2);
    hasBuff |= BuffService.HasBuff(entity, BeholdenBuffPrefabGUID3);

    return hasBuff;
  }
}