using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class HideNameplateBuff
{
  public static readonly PrefabGUID HideNameplateBuffBase = PrefabGUIDs.AB_Cursed_ToadKing_Spit_HideHUDCastBuff;

  public static bool ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, HideNameplateBuffBase);

    if (BuffService.TryGetBuff(targetEntity, HideNameplateBuffBase, out Entity buff1Entity))
    {
      BuffService.makeBuffPermanent(buff1Entity);
    }

    return true;
  }

  public static bool RemoveBuff(Entity entity)
  {
    return BuffService.RemoveBuff(entity, HideNameplateBuffBase);
  }

  public static bool HasBuff(Entity entity)
  {
    return BuffService.HasBuff(entity, HideNameplateBuffBase);
  }
}