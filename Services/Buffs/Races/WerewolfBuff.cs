using ProjectM;
using Unity.Entities;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;

namespace SoVUtilities.Services.Buffs;

internal static class WerewolfBuff
{
  public static readonly PrefabGUID WerewolfBuffBase = PrefabGUIDs.Buff_General_Shapeshift_Werewolf_Standard;
  public static readonly PrefabGUID WerewolfAbilitiesBuff = PrefabGUIDs.EquipBuff_Weapon_DualHammers_Base;
  public static readonly int[] abilitySlotPrefabGUIDs = new int[]
  {
    PrefabGUIDs.AB_Werewolf_MeleeAttack_Group._Value, // Primary auto attack slot
    PrefabGUIDs.AB_Werewolf_Dash_AbilityGroup._Value, // Secondary Q slot
    PrefabGUIDs.AB_Shapeshift_Wolf_Leap_Travel_AbilityGroup._Value, // Travel slot, spacebar
    0, // shift slot
    PrefabGUIDs.AB_WerewolfChieftain_Knockdown_AbilityGroup._Value, // Power slot E
    PrefabGUIDs.AB_Werewolf_Bite_AbilityGroup._Value, // first spell slot R
    PrefabGUIDs.AB_WerewolfChieftain_MultiBiteBuff_Hard_AbilityGroup._Value, // second spell slot C
    PrefabGUIDs.AB_WerewolfChieftain_ShadowDash_Clone_AbilityGroup._Value // ultimate slot t
  };
  static EntityManager EntityManager => Core.EntityManager;
  public static IEnumerator ApplyCustomBuff(Entity targetEntity)
  {
    BuffService.ApplyBuff(targetEntity, WerewolfBuffBase); // werewolf buff handles the model

    // if (BuffService.TryGetBuff(targetEntity, WerewolfBuffBase, out var buffEntity))
    // {
    // BuffService.SetupSyncBuffer(buffEntity, targetEntity);

    // First check if the buff entity exists
    // if (!buffEntity.Exists())
    // {
    //   Core.Log.LogError($"[WerewolfBuff.ApplyCustomBuffStats] - Buff entity {buffEntity} does not exist!");
    //   yield break;
    // }

    // if (!buffEntity.TryGetBuffer<ModifyUnitStatBuff_DOTS>(out var buffer))
    // {
    //   buffer = EntityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(buffEntity);
    // }

    // BuffService.makeBuffPermanent(buffEntity);

    //   yield return null;
    // }

    BuffService.ApplyBuff(targetEntity, WerewolfAbilitiesBuff); // werewolf abilities buff handles the abilities
    if (BuffService.TryGetBuff(targetEntity, WerewolfAbilitiesBuff, out var abilitiesBuffEntity))
    {
      BuffService.makeBuffPermanent(abilitiesBuffEntity);
    }

    yield break;
  }

  public static bool RemoveBuff(Entity entity)
  {
    bool removed = false;
    removed |= BuffService.RemoveBuff(entity, WerewolfBuffBase);
    removed |= BuffService.RemoveBuff(entity, WerewolfAbilitiesBuff);

    return removed;
  }

  public static bool HasBuff(Entity entity)
  {
    bool hasBuff = false;
    hasBuff |= BuffService.HasBuff(entity, WerewolfBuffBase);
    hasBuff |= BuffService.HasBuff(entity, WerewolfAbilitiesBuff);

    return hasBuff;
  }
}