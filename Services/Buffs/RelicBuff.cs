using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class RelicBuff
{
  public static readonly PrefabGUID RelicBuffPrefabGUID = PrefabGUIDs.AB_Bandit_Deadeye_Idle_Buff; // -881914431

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, RelicBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, RelicBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, RelicBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, RelicBuffPrefabGUID);

    return hasBuff;
  }
}