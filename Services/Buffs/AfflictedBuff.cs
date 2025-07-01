using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class AfflictedBuff
{
  public static readonly PrefabGUID AfflictedBuffPrefabGUID = PrefabGUIDs.Buff_General_ChanceToCorruptBlood;

  public static bool ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, AfflictedBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, AfflictedBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    return true;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, AfflictedBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, AfflictedBuffPrefabGUID);

    return hasBuff;
  }
}