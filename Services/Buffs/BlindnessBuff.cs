using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;

namespace SoVUtilities.Services.Buffs;

internal static class BlindnessBuff
{
  public static readonly PrefabGUID BlindnessBuffPrefabGUID = PrefabGUIDs.AB_Undead_Infiltrator_MasterOfDisguise_BlindBuff;

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, BlindnessBuffPrefabGUID);
    if (BuffService.TryGetBuff(targetEntity, BlindnessBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, BlindnessBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, BlindnessBuffPrefabGUID);

    return hasBuff;
  }
}