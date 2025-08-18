using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class AfflictedBuff
{
  public static readonly PrefabGUID AfflictedBuffPrefabGUID = PrefabGUIDs.Buff_General_ChanceToCorruptBlood; // 1524978405
  public static readonly PrefabGUID AfflictedBuffPrefabGUID2 = PrefabGUIDs.AB_Nun_AoE_Light_Buff; // -1466712470

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, AfflictedBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, AfflictedBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return new WaitForSeconds(0.1f);

    BuffService.ApplyBuff(targetEntity, AfflictedBuffPrefabGUID2);

    if (BuffService.TryGetBuff(targetEntity, AfflictedBuffPrefabGUID2, out Entity buff2Entity))
    {
      BuffService.makeBuffPermanent(buff2Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, AfflictedBuffPrefabGUID);
    removed |= BuffService.RemoveBuff(entity, AfflictedBuffPrefabGUID2);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, AfflictedBuffPrefabGUID);
    hasBuff |= BuffService.HasBuff(entity, AfflictedBuffPrefabGUID2);

    return hasBuff;
  }
}