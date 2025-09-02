using Unity.Entities;
using Stunlock.Core;
using ProjectM;
using SoVUtilities.Models;
using SoVUtilities.Services;
using Il2CppSystem;
using Unity.Collections;

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

  public static void setAbility(Entity playerEntity, AbilityTypeEnum targetSlot, PrefabGUID abilityPrefab)
  {
    // Get player data
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    int slotIndex = (int)targetSlot;
    // Set the ability prefab GUID for the target slot
    if (playerData.AbilitySlotPrefabGUIDs != null && slotIndex >= 0 && slotIndex < playerData.AbilitySlotPrefabGUIDs.Length)
    {
      playerData.AbilitySlotPrefabGUIDs[slotIndex] = abilityPrefab.GuidHash;
      // Optionally, save data after change
      PlayerDataService.SaveData();
    }
  }

  public static void ClearAbilitySlot(Entity playerEntity, AbilityTypeEnum targetSlot)
  {
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    int slotIndex = (int)targetSlot;
    if (playerData.AbilitySlotPrefabGUIDs != null && slotIndex >= 0 && slotIndex < playerData.AbilitySlotPrefabGUIDs.Length)
    {
      playerData.AbilitySlotPrefabGUIDs[slotIndex] = 0; // 0 means empty
      PlayerDataService.SaveData();
    }
  }

  public static void ClearAllAbilitySlots(Entity playerEntity)
  {
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    if (playerData.AbilitySlotPrefabGUIDs != null)
    {
      for (int i = 0; i < playerData.AbilitySlotPrefabGUIDs.Length; i++)
      {
        playerData.AbilitySlotPrefabGUIDs[i] = 0; // 0 means empty
      }
      PlayerDataService.SaveData();
    }
  }

  public static void ApplyAbilities(Entity playerEntity, Entity buffEntity)
  {
    // first we want to get the abilities for the associated player
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    if (playerData.AbilitySlotPrefabGUIDs != null)
    {
      for (int i = 0; i < playerData.AbilitySlotPrefabGUIDs.Length; i++)
      {
        if (playerData.AbilitySlotPrefabGUIDs[i] == 0) continue;

        var abilityPrefab = new PrefabGUID(playerData.AbilitySlotPrefabGUIDs[i]);
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
}