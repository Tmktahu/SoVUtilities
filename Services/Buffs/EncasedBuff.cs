using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class EncasedBuff
{
  public static readonly PrefabGUID EncasedBuffPrefabGUID = PrefabGUIDs.AB_Blackfang_Shared_DrinkCorruption_Buff;

  public static bool ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, EncasedBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, EncasedBuffPrefabGUID, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    return true;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, EncasedBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, EncasedBuffPrefabGUID);

    return hasBuff;
  }
}