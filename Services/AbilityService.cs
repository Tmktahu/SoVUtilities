using Unity.Entities;
using Stunlock.Core;
using ProjectM;
using SoVUtilities.Models;
using SoVUtilities.Services;
using Il2CppSystem;
using Unity.Collections;
using SoVUtilities.Resources;
using UnityEngine;
using System.Collections;

namespace SoVUtilities.Services;

// AbilityTypeEnum values for reference:
// None = -1
// Primary = 0
// Secondary = 1
// Travel = 2
// Dash = 3
// Power = 4
// Offensive = 5
// SpellSlot1 = 5
// Defensive = 6
// SpellSlot2 = 6
// Ultimate = 7
public static class AbilityService
{
  static EntityManager EntityManager => Core.EntityManager;

  public static PrefabGUID DEFAULT_AXE_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Axe_Base;
  public static PrefabGUID DEFAULT_CLAWS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Claws_Base;
  public static PrefabGUID DEFAULT_CROSSBOW_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Crossbow_Base;
  public static PrefabGUID DEFAULT_DAGGERS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Daggers_Base;
  public static PrefabGUID DEFAULT_FISHING_POLE_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_FishingPole_Base;
  public static PrefabGUID DEFAULT_GREATSWORD_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_GreatSword_Base;
  public static PrefabGUID DEFAULT_LONGBOW_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Longbow_Base;
  public static PrefabGUID DEFAULT_MACE_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Mace_Base;
  public static PrefabGUID DEFAULT_PISTOLS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Pistols_Base;
  public static PrefabGUID DEFAULT_REAPER_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Reaper_Base;
  public static PrefabGUID DEFAULT_SLASHERS_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Slashers_Base;
  public static PrefabGUID DEFAULT_SPEAR_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Spear_Base;
  public static PrefabGUID DEFAULT_SWORD_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Sword_Base;
  public static PrefabGUID DEFAULT_TWINBLADES_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_TwinBlades_Base;
  public static PrefabGUID DEFAULT_WHIP_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Whip_Base;
  public static PrefabGUID DEFAULT_UNARMED_EQUIPBUFF = PrefabGUIDs.EquipBuff_Weapon_Unarmed_Start01;

  // define category constants
  public const string CATEGORY_AXE = "Axe";
  public const string CATEGORY_CLAWS = "Claws";
  public const string CATEGORY_CROSSBOW = "Crossbow";
  public const string CATEGORY_DAGGERS = "Daggers";
  public const string CATEGORY_FISHING_POLE = "FishingPole";
  public const string CATEGORY_GREATSWORD = "GreatSword";
  public const string CATEGORY_LONGBOW = "Longbow";
  public const string CATEGORY_MACE = "Mace";
  public const string CATEGORY_PISTOLS = "Pistols";
  public const string CATEGORY_REAPER = "Reaper";
  public const string CATEGORY_SLASHERS = "Slashers";
  public const string CATEGORY_SPEAR = "Spear";
  public const string CATEGORY_SWORD = "Sword";
  public const string CATEGORY_TWINBLADES = "TwinBlades";
  public const string CATEGORY_WHIP = "Whip";
  public const string CATEGORY_UNARMED = "Unarmed";

  public static List<string> weaponCategories = new List<string>
  {
      CATEGORY_UNARMED,
      CATEGORY_AXE,
      CATEGORY_CLAWS,
      CATEGORY_CROSSBOW,
      CATEGORY_DAGGERS,
      CATEGORY_FISHING_POLE,
      CATEGORY_GREATSWORD,
      CATEGORY_LONGBOW,
      CATEGORY_MACE,
      CATEGORY_PISTOLS,
      CATEGORY_REAPER,
      CATEGORY_SLASHERS,
      CATEGORY_SPEAR,
      CATEGORY_SWORD,
      CATEGORY_TWINBLADES,
      CATEGORY_WHIP
  };

  public static readonly Dictionary<string, PrefabGUID> WeaponCategoryToDefaultEquipBuff = new Dictionary<string, PrefabGUID>
  {
      { CATEGORY_AXE, DEFAULT_AXE_EQUIPBUFF },
      { CATEGORY_CLAWS, DEFAULT_CLAWS_EQUIPBUFF },
      { CATEGORY_CROSSBOW, DEFAULT_CROSSBOW_EQUIPBUFF },
      { CATEGORY_DAGGERS, DEFAULT_DAGGERS_EQUIPBUFF },
      { CATEGORY_FISHING_POLE, DEFAULT_FISHING_POLE_EQUIPBUFF },
      { CATEGORY_GREATSWORD, DEFAULT_GREATSWORD_EQUIPBUFF },
      { CATEGORY_LONGBOW, DEFAULT_LONGBOW_EQUIPBUFF },
      { CATEGORY_MACE, DEFAULT_MACE_EQUIPBUFF },
      { CATEGORY_PISTOLS, DEFAULT_PISTOLS_EQUIPBUFF },
      { CATEGORY_REAPER, DEFAULT_REAPER_EQUIPBUFF },
      { CATEGORY_SLASHERS, DEFAULT_SLASHERS_EQUIPBUFF },
      { CATEGORY_SPEAR, DEFAULT_SPEAR_EQUIPBUFF },
      { CATEGORY_SWORD, DEFAULT_SWORD_EQUIPBUFF },
      { CATEGORY_TWINBLADES, DEFAULT_TWINBLADES_EQUIPBUFF },
      { CATEGORY_WHIP, DEFAULT_WHIP_EQUIPBUFF },
      { CATEGORY_UNARMED, DEFAULT_UNARMED_EQUIPBUFF }
  };

  public static readonly Dictionary<int, AbilityTypeEnum> PlacementIdToAbilityType = new Dictionary<int, AbilityTypeEnum>
  {
      { 0, AbilityTypeEnum.Primary },
      { 1, AbilityTypeEnum.Secondary },
      { 2, AbilityTypeEnum.Power },
      { 3, AbilityTypeEnum.Travel },
      { 4, AbilityTypeEnum.Offensive },
      { 5, AbilityTypeEnum.Defensive },
      { 6, AbilityTypeEnum.Ultimate }
  };

  public static readonly string WOLF_COMBAT_FORM_TAG = "wolf_combat_form";
  public static readonly string BEAR_COMBAT_FORM_TAG = "bear_combat_form";
  public static readonly string TOAD_COMBAT_FORM_TAG = "toad_combat_form";
  public static readonly string RAT_COMBAT_FORM_TAG = "rat_combat_form";
  public static readonly string SPIDER_COMBAT_FORM_TAG = "spider_combat_form";
  public static readonly string BAT_COMBAT_FORM_TAG = "bat_combat_form";

  public static readonly Dictionary<PrefabGUID, string> CombatFormToTag = new Dictionary<PrefabGUID, string>
  {
    { PrefabGUIDs.AB_Shapeshift_Wolf_Buff, WOLF_COMBAT_FORM_TAG },
    { PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff, WOLF_COMBAT_FORM_TAG },
    { PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff, WOLF_COMBAT_FORM_TAG },
    { PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff, WOLF_COMBAT_FORM_TAG },
    // { ShapeshiftBearBuff, BEAR_COMBAT_FORM_TAG },
    // { ShapeshiftBearSkin01Buff, BEAR_COMBAT_FORM_TAG },
    { PrefabGUIDs.AB_Shapeshift_Toad_Buff, TOAD_COMBAT_FORM_TAG },
    // { ShapeshiftToadSkin01Buff, TOAD_COMBAT_FORM_TAG },
    { PrefabGUIDs.AB_Shapeshift_Rat_Buff, RAT_COMBAT_FORM_TAG },
    { PrefabGUIDs.AB_Shapeshift_Spider_Buff, SPIDER_COMBAT_FORM_TAG }
  };

  public static readonly PrefabGUID[] combatShapeshiftForms = new PrefabGUID[]
  {
    PrefabGUIDs.AB_Shapeshift_Wolf_Buff,
    PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff,
    PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff,
    PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff,
    PrefabGUIDs.AB_Shapeshift_Rat_Buff,
    // ShapeshiftBearSkinBuff,
    // ShapeshiftBearSkin01Buff,
    PrefabGUIDs.AB_Shapeshift_Toad_Buff,
    // ShapeshiftToadSkin01Buff,
    PrefabGUIDs.AB_Shapeshift_Spider_Buff
  };

  // dictionary of prefabs to ability slot prefab GUID arrays
  // public static readonly Dictionary<string, PrefabGUID[]> ShapeshiftAbilitySlots = new Dictionary<string, PrefabGUID[]>
  // {
  //   { WOLF_COMBAT_FORM_TAG, [
  //     PrefabGUIDs.AB_Shapeshift_Wolf_Bite_Group,
  //     PrefabGUIDs.AB_Wolf_Boss_Pounce_AbilityGroup,
  //     PrefabGUIDs.AB_Shapeshift_Wolf_Leap_Travel_AbilityGroup,
  //     PrefabGUIDs.Zero,
  //     PrefabGUIDs.AB_Wolf_Shared_DashAttack_AbilityGroup,
  //     PrefabGUID.Empty,
  //     PrefabGUID.Empty,
  //     PrefabGUIDs.Zero
  //   ] },
  //   { TOAD_COMBAT_FORM_TAG, [
  //     PrefabGUIDs.AB_Cursed_MonsterToad_TongueSlap_AbilityGroup,
  //     PrefabGUIDs.AB_Cursed_MonsterToad_TongueGrab_AbilityGroup,
  //     PrefabGUIDs.Zero,
  //     PrefabGUIDs.Zero,
  //     PrefabGUIDs.AB_Cursed_MonsterToad_LeapAttack_Travel_AbilityGroup,
  //     PrefabGUID.Empty,
  //     PrefabGUID.Empty,
  //     PrefabGUID.Empty
  //   ] },
  //   { RAT_COMBAT_FORM_TAG, [
  //     PrefabGUIDs.AB_Vermin_Rat_MeleeAttack_AbilityGroup,
  //     PrefabGUIDs.AB_Vermin_DireRat_Gnaw_AbilityGroup,
  //     PrefabGUIDs.Zero,
  //     PrefabGUIDs.Zero,
  //     PrefabGUIDs.AB_Shapeshift_Spider_Burrow_AbilityGroup,
  //     PrefabGUID.Empty,
  //     PrefabGUID.Empty,
  //     PrefabGUID.Empty
  //   ] },
  //   { SPIDER_COMBAT_FORM_TAG, [
  //     PrefabGUIDs.AB_Spider_Melee_MeleeAttack_Group,
  //     PrefabGUIDs.AB_Spider_Forest_Webbing_AbilityGroup,
  //     PrefabGUIDs.Zero,
  //     PrefabGUIDs.Zero,
  //     PrefabGUIDs.AB_Spider_Forest_BackStep_AbilityGroup,
  //     PrefabGUID.Empty,
  //     PrefabGUID.Empty,
  //     PrefabGUID.Empty
  //   ] },
  //   { BAT_COMBAT_FORM_TAG, [
  //     // to be defined
  //   ] }
  // };

  public static readonly int[] WolfFormAbilitySlotPrefabGUIDs = new int[]
  {
    PrefabGUIDs.AB_Shapeshift_Wolf_Bite_Group._Value, // Primary auto attack slot
    PrefabGUIDs.AB_Wolf_Boss_Pounce_AbilityGroup._Value, // Secondary Q slot
    PrefabGUIDs.AB_Shapeshift_Wolf_Leap_Travel_AbilityGroup._Value, // Travel slot, spacebar
    0, // shift slot
    PrefabGUIDs.AB_Wolf_Shared_DashAttack_AbilityGroup._Value, // Power slot E
    -1, // first spell slot R, clear it completely
    -1, // second spell slot C, clear it completely
    0 // ultimate slot t, this is the normal wolf howl
  };

  public static readonly int[] ToadFormAbilitySlotPrefabGUIDs = new int[]
  {
    PrefabGUIDs.AB_Cursed_MonsterToad_TongueSlap_AbilityGroup._Value, // Primary auto attack slot
    PrefabGUIDs.AB_Cursed_MonsterToad_TongueGrab_AbilityGroup._Value, // Secondary Q slot
    0, // Travel slot, spacebar
    0, // shift slot
    PrefabGUIDs.AB_Cursed_MonsterToad_LeapAttack_Travel_AbilityGroup._Value, // Power slot E
    -1, // first spell slot R, clear it completely
    -1, // second spell slot C, clear it completely
    -1 // ultimate slot t, clear it completely
  };

  public static readonly int[] RatFormAbilitySlotPrefabGUIDs = new int[]
  {
    PrefabGUIDs.AB_Vermin_Rat_MeleeAttack_AbilityGroup._Value, // Primary auto attack slot
    PrefabGUIDs.AB_Vermin_DireRat_Gnaw_AbilityGroup._Value, // Secondary Q slot
    -1, // Travel slot, spacebar
    0, // shift slot
    PrefabGUIDs.AB_Shapeshift_Spider_Burrow_AbilityGroup._Value, // Power slot E
    -1, // first spell slot R, clear it completely
    -1, // second spell slot C, clear it completely
    -1 // ultimate slot t, clear it completely
  };

  public static readonly int[] SpiderFormAbilitySlotPrefabGUIDs = new int[]
  {
    PrefabGUIDs.AB_Spider_Melee_MeleeAttack_Group._Value, // Primary auto attack slot
    PrefabGUIDs.AB_Spider_Forest_Webbing_AbilityGroup._Value, // Secondary Q slot
    0, // Travel slot, spacebar
    0, // shift slot
    PrefabGUIDs.AB_Spider_Forest_BackStep_AbilityGroup._Value, // Power slot E
    -1, // first spell slot R, clear it completely
    -1, // second spell slot C, clear it completely
    -1 // ultimate slot t, clear it completely
  };

  public static readonly int[] BatFormAbilitySlotPrefabGUIDs = new int[]
  {
    PrefabGUIDs.AB_BatSwarm_Melee_AbilityGroup._Value, // Primary auto attack slot
    0, // Secondary Q slot
    0, // Travel slot, spacebar
    0, // shift slot
    PrefabGUIDs.AB_Dracula_ShadowBatSwarm_Dash_AbilityGroup._Value, // Power slot E
    0, // first spell slot R
    0, // second spell slot C
    0 // ultimate slot t
  };

  public static readonly int[] BearFormAbilitySlotPrefabGUIDs = new int[]
  {
    0, // Primary auto attack slot
    0, // Secondary Q slot
    PrefabGUIDs.AB_Shapeshift_Bear_Dash_Group._Value, // Travel slot, spacebar
    0, // shift slot
    PrefabGUIDs.AB_Bear_Dire_AreaAttack_AbilityGroup._Value, // Power slot E
    PrefabGUIDs.AB_Bear_FallAsleep_Group._Value, // first spell slot R
    PrefabGUIDs.AB_Bear_WakeUp_Group._Value, // second spell slot C
    0 // ultimate slot t
  };

  public static void setAbility(Entity playerEntity, AbilityTypeEnum targetSlot, PrefabGUID abilityPrefab, string currentWeaponCategory)
  {
    // Get player data
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    WeaponCategoryToDefaultEquipBuff.TryGetValue(currentWeaponCategory, out PrefabGUID defaultEquipBuff);
    int PrefabGUIDInt = defaultEquipBuff._Value;

    // we go to the hash and override its value with new data, but that means we gotta get th existing data if it exists first
    Dictionary<int, int[]> abilitySlotDefinitions = playerData.AbilitySlotDefinitions;

    // check to see if we have an entry for this equip buff already
    if (!abilitySlotDefinitions.ContainsKey(PrefabGUIDInt))
    {
      // if not, we create a new entry with empty slots
      abilitySlotDefinitions[PrefabGUIDInt] = new int[8]; // assuming 8 ability slots
    }

    playerData.AbilitySlotDefinitions[PrefabGUIDInt][(int)targetSlot] = abilityPrefab.GuidHash;
    PlayerDataService.SaveData();
  }

  public static void ClearAbilitySlot(Entity playerEntity, AbilityTypeEnum targetSlot, string currentWeaponCategory)
  {
    // Get player data
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    WeaponCategoryToDefaultEquipBuff.TryGetValue(currentWeaponCategory, out PrefabGUID defaultEquipBuff);
    int PrefabGUIDInt = defaultEquipBuff._Value;

    // we go to the hash and override its value with new data, but that means we gotta get th existing data if it exists first
    Dictionary<int, int[]> abilitySlotDefinitions = playerData.AbilitySlotDefinitions;

    // check to see if we have an entry for this equip buff already
    if (!abilitySlotDefinitions.ContainsKey(PrefabGUIDInt))
    {
      // if not, we create a new entry with empty slots
      abilitySlotDefinitions[PrefabGUIDInt] = new int[8]; // assuming 8 ability slots
    }

    playerData.AbilitySlotDefinitions[PrefabGUIDInt][(int)targetSlot] = 0; // 0 means empty
    PlayerDataService.SaveData();
  }

  public static void ClearAllAbilitySlots(Entity playerEntity, string currentWeaponCategory)
  {
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    WeaponCategoryToDefaultEquipBuff.TryGetValue(currentWeaponCategory, out PrefabGUID defaultEquipBuff);
    int PrefabGUIDInt = defaultEquipBuff._Value;

    // we go to the hash and override its value with new data, but that means we gotta get th existing data if it exists first
    Dictionary<int, int[]> abilitySlotDefinitions = playerData.AbilitySlotDefinitions;

    // check to see if we have an entry for this equip buff already
    if (!abilitySlotDefinitions.ContainsKey(PrefabGUIDInt))
    {
      // if not, we create a new entry with empty slots
      abilitySlotDefinitions[PrefabGUIDInt] = new int[8]; // assuming 8 ability slots
    }

    for (int i = 0; i < 8; i++)
    {
      playerData.AbilitySlotDefinitions[PrefabGUIDInt][i] = 0; // 0 means empty
    }
    PlayerDataService.SaveData();
  }

  public static void SetItemAbility(Entity itemEntity, AbilityTypeEnum targetSlot, PrefabGUID abilityPrefab)
  {
    // Get item data
    var itemData = ItemDataService.GetItemData(itemEntity);

    itemData.AbilityGUIDs[(int)targetSlot] = abilityPrefab.GuidHash;
    ItemDataService.SaveData();
  }

  public static void ClearItemAbility(Entity itemEntity, AbilityTypeEnum targetSlot)
  {
    // Get item data
    var itemData = ItemDataService.GetItemData(itemEntity);

    itemData.AbilityGUIDs[(int)targetSlot] = 0; // 0 means empty
    ItemDataService.SaveData();
  }

  public static void ClearAllItemAbilities(Entity itemEntity)
  {
    var itemData = ItemDataService.GetItemData(itemEntity);

    for (int i = 0; i < 8; i++)
    {
      itemData.AbilityGUIDs[i] = 0; // 0 means empty
    }
    ItemDataService.SaveData();
  }

  public static void ApplyAbilities(Entity playerEntity, Entity buffEntity, int[] abilitySlotPrefabGUIDs, int groupPriority = 1)
  {
    // first we want to get the abilities for the associated player
    if (abilitySlotPrefabGUIDs != null)
    {
      var buffer = EntityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(buffEntity);
      // we want to loop over the buff and see if there are any abilities with a priority of 99
      // if there are, we change the priority to 11
      for (int i = 0; i < buffer.Length; i++)
      {
        var existingBuff = buffer[i];
        if (existingBuff.Priority == 99)
        {
          existingBuff.Priority = 8;
          buffer[i] = existingBuff;
        }
      }

      for (int i = 0; i < abilitySlotPrefabGUIDs.Length; i++)
      {
        if (abilitySlotPrefabGUIDs[i] == 0) continue; // 0 means skip

        var abilityPrefab = new PrefabGUID(abilitySlotPrefabGUIDs[i]);

        if (abilitySlotPrefabGUIDs[i] == -1)
          abilityPrefab = PrefabGUIDs.Zero; // -1 means we actually want to set the slot to nothing

        if (abilityPrefab.HasValue())
        {
          // Apply the ability to the player
          ApplyAbilityBuff(buffEntity, (AbilityTypeEnum)i, abilityPrefab, groupPriority);
        }
      }
    }
  }

  public static void ApplyAbilityBuff(Entity buffEntity, AbilityTypeEnum abilityType, PrefabGUID abilityGUID, int priority = 1)
  {
    var buffer = EntityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(buffEntity);

    ReplaceAbilityOnSlotBuff buff;

    if (abilityGUID.Equals(PrefabGUIDs.Zero))
    {
      buff = new()
      {
        Slot = (int)abilityType,
        CopyCooldown = false,
        CastBlockType = GroupSlotModificationCastBlockType.WholeCast,
        Priority = 99,
      };
    }
    else
    {
      buff = new()
      {
        Slot = (int)abilityType,
        NewGroupId = abilityGUID,
        CopyCooldown = true,
        Priority = priority,
      };
    }

    buffer.Add(buff);
  }

  public static PrefabGUID getEquippedAbilityPrefabGuid(Entity playerEntity, AbilityTypeEnum targetSlot)
  {
    // we get the actually equipped ability prefab GUID from the player game data, not our data
    var buffEntities = EntityService.GetEntitiesByComponentTypes<Buff, PrefabGUID>();
    foreach (var buffEntity in buffEntities)
    {
      if (buffEntity.Read<EntityOwner>().Owner == playerEntity)
      {
        PrefabGUID buff = EntityManager.GetComponentData<PrefabGUID>(buffEntity);
        if (buff == PrefabGUIDs.Buff_VBlood_Ability_Replace)
        {
          if (EntityManager.TryGetComponentData<VBloodAbilityReplaceBuff>(buffEntity, out var vBloodAbilityReplaceBuff))
          {
            if (vBloodAbilityReplaceBuff.AbilityType == targetSlot)
            {
              return vBloodAbilityReplaceBuff.AbilityGUID;
            }
          }
        }
      }
    }

    return PrefabGUID.Empty;
  }

  public static IEnumerator RefreshEquipBuff(Entity playerEntity)
  {
    PrefabGUID currentEquipBuff = PlayerService.GetEquipBuffPrefabGUID(playerEntity);
    // to refresh the equip buff, we remove it and re-apply it
    if (BuffService.HasBuff(playerEntity, currentEquipBuff))
    {
      BuffService.RemoveBuff(playerEntity, currentEquipBuff);
    }

    // yield return new WaitForSeconds(0.5f); // wait a short moment to ensure the buff is removed

    BuffService.ApplyBuff(playerEntity, currentEquipBuff);

    yield return null;
  }
}