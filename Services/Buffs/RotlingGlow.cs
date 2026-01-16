using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class RotlingGlowBuff
{
  public static readonly PrefabGUID RotlingBuffPrefabGUID = PrefabGUIDs.Buff_General_ChanceToCorruptBlood; // 1524978405

  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, RotlingBuffPrefabGUID);

    if (BuffService.TryGetBuff(targetEntity, RotlingBuffPrefabGUID, out Entity buffEntity))
    {
      BuffService.SetupSyncBuffer(buffEntity, targetEntity);

      // First check if the buff entity exists
      if (!buffEntity.Exists())
      {
        Core.Log.LogError($"[RotlingGlowBuff.ApplyCustomBuff] - Buff entity {buffEntity} does not exist!");
        yield break;
      }

      BuffService.makeBuffPermanent(buffEntity);
    }

    yield return null;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, RotlingBuffPrefabGUID);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, RotlingBuffPrefabGUID);

    return hasBuff;
  }
}