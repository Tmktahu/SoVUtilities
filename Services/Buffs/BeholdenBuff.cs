using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class BeholdenBuff
{
  public static readonly PrefabGUID BeholdenBuffPrefabGUID = PrefabGUIDs.Unholy_Vampire_Buff_Bane;

  public static bool ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, BeholdenBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, BeholdenBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    return true;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, BeholdenBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, BeholdenBuffPrefabGUID);

    return hasBuff;
  }
}