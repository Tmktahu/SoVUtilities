using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class SpiritChosenBuff
{
  public static readonly PrefabGUID SpiritChosenBuffPrefabGUID = PrefabGUIDs.Buff_General_Ghost; // -1242403012

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, SpiritChosenBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, SpiritChosenBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, SpiritChosenBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, SpiritChosenBuffPrefabGUID);

    return hasBuff;
  }
}