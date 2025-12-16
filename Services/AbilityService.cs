using Unity.Entities;
using Stunlock.Core;
using ProjectM;
using SoVUtilities.Models;
using SoVUtilities.Services;
using Il2CppSystem;
using Unity.Collections;
using SoVUtilities.Resources;

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

  public static void ApplyAbilities(Entity playerEntity, Entity buffEntity, int[] abilitySlotPrefabGUIDs)
  {
    // first we want to get the abilities for the associated player
    if (abilitySlotPrefabGUIDs != null)
    {
      for (int i = 0; i < abilitySlotPrefabGUIDs.Length; i++)
      {
        if (abilitySlotPrefabGUIDs[i] == 0) continue;

        var abilityPrefab = new PrefabGUID(abilitySlotPrefabGUIDs[i]);
        if (abilityPrefab.HasValue())
        {
          // Apply the ability to the player
          ApplyAbilityBuff(buffEntity, (AbilityTypeEnum)i, abilityPrefab);
        }
      }
    }
  }

  public static void ApplyAbilityBuff(Entity buffEntity, AbilityTypeEnum abilityType, PrefabGUID abilityGUID)
  {
    var buffer = EntityManager.GetBuffer<ReplaceAbilityOnSlotBuff>(buffEntity);

    ReplaceAbilityOnSlotBuff buff = new()
    {
      Slot = (int)abilityType,
      NewGroupId = abilityGUID,
      CopyCooldown = true,
      Priority = 1,
    };

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
}