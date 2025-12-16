using ProjectM;
using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class WerewolfBuff
{
  public static readonly PrefabGUID WerewolfBuffBase = PrefabGUIDs.Buff_General_Shapeshift_Werewolf_Standard;

  static EntityManager EntityManager => Core.EntityManager;
  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, WerewolfBuffBase); // werewolf buff handles the model

    if (BuffService.TryGetBuff(targetEntity, WerewolfBuffBase, out var buffEntity))
    {
      BuffService.SetupSyncBuffer(buffEntity, targetEntity);

      // First check if the buff entity exists
      if (!buffEntity.Exists())
      {
        Core.Log.LogError($"[WerewolfBuff.ApplyCustomBuffStats] - Buff entity {buffEntity} does not exist!");
        yield break;
      }

      if (!buffEntity.TryGetBuffer<ModifyUnitStatBuff_DOTS>(out var buffer))
      {
        buffer = EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
      }

      // BuffService.makeBuffPermanent(buffEntity);

      yield return null;
    }

    yield break;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, WerewolfBuffBase);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, WerewolfBuffBase);

    return hasBuff;
  }
}