using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class SeededBuff
{
  public static readonly PrefabGUID SeededBuffPrefabGUID = PrefabGUIDs.Buff_General_RelicCarryDebuff; // 1524978405

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, SeededBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, SeededBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, SeededBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, SeededBuffPrefabGUID);

    return hasBuff;
  }
}