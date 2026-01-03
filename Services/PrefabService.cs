using System.Collections;
using System.Linq;
using Il2CppInterop.Runtime;
using SoVUtilities.Models;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Stunlock.Core;
using SoVUtilities.Resources;
using ProjectM.Gameplay.Scripting;

namespace SoVUtilities.Services;

internal class PrefabService
{
  readonly EntityManager EntityManager = Core.EntityManager;
  readonly SystemService SystemService = Core.SystemService;

  internal PrefabService()
  {

  }

  public void ModifyPrefabs()
  {
    // Modify prefabs as needed
    // if (ConfigService.DisableBatForm)
    // {
    //   if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.Tech_Collection_VBlood_T08_BatVampire, out Entity prefabEntity))
    //   {
    //     // it has a ProgressionBookShapeshiftElement buffer
    //     if (EntityManager.TryGetBuffer<ProgressionBookShapeshiftElement>(prefabEntity, out var shapeshiftBuffer))
    //     {
    //       // it only has one, so we just clear the buffer
    //       shapeshiftBuffer.Clear();
    //     }
    //   }
    // }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.Buff_Hunger_For_Power_Effect, out Entity hungerForPowerEffectBuffEntity))
    {
      // it has a ModifyMovementSpeedBuff component we want to delete
      if (EntityManager.TryGetComponentData<ModifyMovementSpeedBuff>(hungerForPowerEffectBuffEntity, out var modifyMovementSpeedBuff))
      {
        modifyMovementSpeedBuff.MoveSpeed = 1.05f; // change to 5% increase instead of removing
        EntityManager.SetComponentData(hungerForPowerEffectBuffEntity, modifyMovementSpeedBuff);
      }

      // it has an EmpowerBuff component we want to edit
      if (EntityManager.TryGetComponentData<EmpowerBuff>(hungerForPowerEffectBuffEntity, out var empowerBuff))
      {
        empowerBuff.EmpowerModifier = 0.1f; // original is 0.2f for 20% global damage increase
        EntityManager.SetComponentData(hungerForPowerEffectBuffEntity, empowerBuff);
      }
    }

    /// ===============================
    /// Shapeshift Buff Modifications
    /// ===============================

    PrefabGUID[] shapeshiftBuffs =
    [
      PrefabGUIDs.AB_Shapeshift_Wolf_Buff,
      PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff,
      PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff,
      PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff,
      PrefabGUIDs.AB_Shapeshift_Toad_Buff,
      PrefabGUIDs.AB_Shapeshift_Toad_PMK_Skin01_Buff,
      PrefabGUIDs.AB_Shapeshift_Rat_Buff,
    ];

    // this modification makes it so that shapeshift forms are not removed on damage taken
    foreach (var shapeshiftBuff in shapeshiftBuffs)
    {
      if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(shapeshiftBuff, out Entity shapeshiftBuffEntity))
      {
        if (EntityManager.TryGetComponentData<Script_Buff_Shapeshift_DataShared>(shapeshiftBuffEntity, out var shapeshiftData))
        {
          shapeshiftData.RemoveOnDamageTaken = false; // change to not remove on damage taken
          EntityManager.SetComponentData(shapeshiftBuffEntity, shapeshiftData);
        }
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Shapeshift_Wolf_Buff, out Entity wolfShapeshiftBuffEntity))
    {
      if (EntityManager.TryGetComponentData<BuffModificationFlagData>(wolfShapeshiftBuffEntity, out var buffModificationFlagData))
      {
        var modTypes = (BuffModificationTypes)buffModificationFlagData.ModificationTypes;

        // we want to remove the FeedImpaired flag
        modTypes &= ~BuffModificationTypes.FeedImpaired;
        buffModificationFlagData.ModificationTypes = (int)modTypes;
        EntityManager.SetComponentData(wolfShapeshiftBuffEntity, buffModificationFlagData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Shapeshift_Bat_TakeFlight_Buff, out Entity batShapeshiftBuffEntity))
    {
      if (EntityManager.TryGetComponentData<BuffModificationFlagData>(batShapeshiftBuffEntity, out var buffModificationFlagData))
      {
        var modTypes = (BuffModificationTypes)buffModificationFlagData.ModificationTypes;

        // Default ModificationTypes: ImmuneToHazards, BuildMenuImpair, IsFlying, ConsumableImpair, DisableDynamicCollision, TargetSpellImpaired, AlwaysInSun, FlyOnlyMapCollision (92275425)

        // we want to remove several of them
        modTypes &= ~BuffModificationTypes.DisableDynamicCollision;
        modTypes &= ~BuffModificationTypes.FlyOnlyMapCollision;
        modTypes &= ~BuffModificationTypes.AlwaysInSun;
        modTypes &= ~BuffModificationTypes.IsFlying;
        modTypes &= ~BuffModificationTypes.ImmuneToHazards;
        modTypes &= ~BuffModificationTypes.TargetSpellImpaired;

        buffModificationFlagData.ModificationTypes = (int)modTypes;
        EntityManager.SetComponentData(batShapeshiftBuffEntity, buffModificationFlagData);
      }

      EntityManager.RemoveComponent<DisableAggroBuff>(batShapeshiftBuffEntity);
      EntityManager.RemoveComponent<ShapeshiftImpairBuff>(batShapeshiftBuffEntity);
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Shapeshift_Bat_Land_AbilityGroup, out Entity batShapeshiftLandAbilityEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetBuffer<AbilityGroupStartAbilitiesBuffer>(batShapeshiftLandAbilityEntity, out var abilityGroupStartAbilitiesBuffer))
      {
        for (int i = 0; i < abilityGroupStartAbilitiesBuffer.Length; i++)
        {
          var ability = abilityGroupStartAbilitiesBuffer[i];
          if (ability.PrefabGUID.Equals(PrefabGUIDs.AB_Shapeshift_Bat_Land_PreCast))
          {
            ability.PrefabGUID = PrefabGUIDs.AB_Shapeshift_Bat_Landing_Travel;
            abilityGroupStartAbilitiesBuffer[i] = ability; // assignment back is required
          }
        }
      }

      // then we get the buffer again and check to see if it was changed
      if (EntityManager.TryGetBuffer<AbilityGroupStartAbilitiesBuffer>(batShapeshiftLandAbilityEntity, out var verifyAbilityGroupStartAbilitiesBuffer))
      {
        for (int i = 0; i < verifyAbilityGroupStartAbilitiesBuffer.Length; i++)
        {
          var ability = verifyAbilityGroupStartAbilitiesBuffer[i];
          Core.Log.LogInfo($"- Bat Land Ability Group Start Ability {i}: {ability.PrefabGUID}");
        }
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.Buff_General_Shapeshift_Werewolf_Standard, out Entity werewolfShapeshiftBuffEntity))
    {
      if (EntityManager.TryGetComponentData<BuffModificationFlagData>(werewolfShapeshiftBuffEntity, out var buffModificationFlagData))
      {
        Core.Log.LogInfo($"- Werewolf BuffModificationFlagData ModificationTypes: {buffModificationFlagData.ModificationTypes}");
        var modTypes = (ProjectM.BuffModificationTypes)buffModificationFlagData.ModificationTypes;
        Core.Log.LogInfo($"Werewolf ModificationTypes: {modTypes} ({buffModificationFlagData.ModificationTypes})");

        // we want to remove the FeedImpaired flag
        // modTypes &= ~ProjectM.BuffModificationTypes.FeedImpaired;
        // buffModificationFlagData.ModificationTypes = (int)modTypes;
        // EntityManager.SetComponentData(wolfShapeshiftBuffEntity, buffModificationFlagData);
      }
      else
      {
        Core.Log.LogWarning($"- Werewolf BuffModificationFlagData component not found on werewolf shapeshift buff entity.");
      }
    }

    // add a cooldown of 12 seconds to the wolf boss pounce ability
    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Wolf_Boss_Pounce_Cast, out Entity wolfBossPounceCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(wolfBossPounceCastEntity, out var abilityCooldownData))
      {
        abilityCooldownData.Cooldown._Value = 12f; // no cooldown by default
        EntityManager.SetComponentData(wolfBossPounceCastEntity, abilityCooldownData);
      }
    }

    // add a cooldown of 8 seconds to the spider form web shoot ability
    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Spider_Forest_Webbing_Cast, out Entity spiderFormWebShootCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(spiderFormWebShootCastEntity, out var abilityCooldownData))
      {
        abilityCooldownData.Cooldown._Value = 8f; // no cooldown by default
        EntityManager.SetComponentData(spiderFormWebShootCastEntity, abilityCooldownData);
      }
    }

    // make the rat bite execute twice as fast
    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Vermin_Rat_MeleeAttack_Cast, out Entity ratBiteCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      // if (EntityManager.TryGetComponentData<AbilityCastTimeData>(ratBiteCastEntity, out var abilityCooldownData))
      // {
      // abilityCooldownData.MaxCastTime._Value = 0.1f; // original is 1.0f
      // abilityCooldownData.PostCastTime._Value = 0f; // original is 0.6f
      // EntityManager.SetComponentData(ratBiteCastEntity, abilityCooldownData);
      // }
    }

    /// ===============================
    /// Werewolf Buff Modifications
    /// ============================

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.EquipBuff_Weapon_DualHammers_Base, out Entity dualHammersBuffEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<Buff>(dualHammersBuffEntity, out var buffData))
      {
        buffData.BuffType = BuffType.Block;
        EntityManager.SetComponentData(dualHammersBuffEntity, buffData);
      }

      EntityManager.RemoveComponent<EquippableBuff>(dualHammersBuffEntity);
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Werewolf_Bite_Cast, out Entity biteCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(biteCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf Bite cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 6f; // change to 6 seconds
        EntityManager.SetComponentData(biteCastEntity, abilityCooldownData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_WerewolfChieftain_Knockdown_Cast, out Entity knockdownCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(knockdownCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf Knockdown cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 10f; // change to 10 seconds
        EntityManager.SetComponentData(knockdownCastEntity, abilityCooldownData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_WerewolfChieftain_MultiBite_Cast, out Entity multibiteCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(multibiteCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf Knockdown cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 0.5f; // change to 0.5 seconds
        EntityManager.SetComponentData(multibiteCastEntity, abilityCooldownData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_WerewolfChieftain_ShadowDash_Clone_Cast, out Entity shadowDashCloneCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(shadowDashCloneCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf Knockdown cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 120f; // change to 120 seconds
        EntityManager.SetComponentData(shadowDashCloneCastEntity, abilityCooldownData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_WerewolfChieftain_MultiBiteBuff_Hard_Cast, out Entity multibiteBuffCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(multibiteBuffCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf MultiBite Buff cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 8f; // change to 8 seconds
        EntityManager.SetComponentData(multibiteBuffCastEntity, abilityCooldownData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Werewolf_Dash_Cast, out Entity werewolfDashCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(werewolfDashCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf Dash cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 8f; // change to 8 seconds
        EntityManager.SetComponentData(werewolfDashCastEntity, abilityCooldownData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_WerewolfChieftain_Knockdown_Cast, out Entity chieftainKnockdownCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(chieftainKnockdownCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf Dash cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 8f; // change to 8 seconds
        EntityManager.SetComponentData(chieftainKnockdownCastEntity, abilityCooldownData);
      }
    }

    // if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Werewolf_Bite_Hit, out Entity biteHitEntity))
    // {
    //   // it has a AbilityCooldownData component we want to edit
    //   if (EntityManager.TryGetComponentData<DealDamageOnGameplayEvent>(biteHitEntity, out var dealDamageOnGameplayEvent))
    //   {
    //     DealDamageParameters damageParameters = dealDamageOnGameplayEvent.Parameters;
    //     float mainFactor = damageParameters.MainFactor;
    //     float resourceModifier = damageParameters.ResourceModifier;
    //     float staggerFactor = damageParameters.StaggerFactor;
    //     float rawDamageValue = damageParameters.RawDamageValue;
    //     float rawDamagePercent = damageParameters.RawDamagePercent;
    //     int dealDamageFlags = damageParameters.DealDamageFlags;
    //     MainDamageType mainDamageType = damageParameters.MainType;

    //     Core.Log.LogInfo($"Original Werewolf Bite damage - MainFactor: {mainFactor}, ResourceModifier: {resourceModifier}, StaggerFactor: {staggerFactor}, RawDamageValue: {rawDamageValue}, RawDamagePercent: {rawDamagePercent}, DealDamageFlags: {dealDamageFlags}, MainDamageType: {mainDamageType}");

    //     float damageModifierPerHit = dealDamageOnGameplayEvent.DamageModifierPerHit;
    //     bool multiplyMainFactorWithStacks = dealDamageOnGameplayEvent.MultiplyMainFactorWithStacks;
    //     Core.Log.LogInfo($"Original Werewolf Bite DamageModifierPerHit: {damageModifierPerHit}, MultiplyMainFactorWithStacks: {multiplyMainFactorWithStacks}");

    //     dealDamageOnGameplayEvent.Parameters.RawDamagePercent = 50f;
    //     EntityManager.SetComponentData(biteHitEntity, dealDamageOnGameplayEvent);
    //   }
    // }

    // if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Dreadhorn_Trample_Channel, out Entity trampleChannelEntity))
    // {
    //   // it has a AbilityCooldownData component we want to edit
    //   if (EntityManager.TryGetBuffer<DealDamageOnGameplayEvent>(trampleChannelEntity, out var buffer))
    //   {
    //     for (int i = 0; i < buffer.Length; i++)
    //     {
    //       DealDamageOnGameplayEvent dealDamageOnGameplayEvent = buffer[i];
    //       DealDamageParameters damageParameters = dealDamageOnGameplayEvent.Parameters;
    //       float mainFactor = damageParameters.MainFactor;
    //       float resourceModifier = damageParameters.ResourceModifier;
    //       float staggerFactor = damageParameters.StaggerFactor;
    //       float rawDamageValue = damageParameters.RawDamageValue;
    //       float rawDamagePercent = damageParameters.RawDamagePercent;
    //       int dealDamageFlags = damageParameters.DealDamageFlags;
    //       MainDamageType mainDamageType = damageParameters.MainType;

    //       Core.Log.LogInfo($"Original Dreadhorn Trample Channel damage - MainFactor: {mainFactor}, ResourceModifier: {resourceModifier}, StaggerFactor: {staggerFactor}, RawDamageValue: {rawDamageValue}, RawDamagePercent: {rawDamagePercent}, DealDamageFlags: {dealDamageFlags}, MainDamageType: {mainDamageType}");

    //       // Modify the raw damage percent
    //       // damageParameters.RawDamagePercent = 30f;
    //       // dealDamageOnGameplayEvent.Parameters = damageParameters;

    //       // buffer[i] = dealDamageOnGameplayEvent;

    //       if (mainFactor == 0.5)
    //       {
    //         damageParameters.MainFactor = 30f;
    //         dealDamageOnGameplayEvent.Parameters = damageParameters;

    //         buffer[i] = dealDamageOnGameplayEvent;
    //       }
    //     }

    //     // EntityManager.SetComponentData<DealDamageOnGameplayEvent>(trampleChannelEntity, buffer);
    // }
  }
}
