using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared;
using SoVUtilities.Resources;
using Stunlock.Core;
using Unity.Entities;
using Unity.Mathematics;
using static ProjectM.InventoryInstanceElement;

namespace SoVUtilities.Services;

internal static class ItemService
{
  static EntityManager EntityManager => Core.EntityManager;

  /// <summary>
  /// Gives a human blood potion to the specified player
  /// </summary>
  /// <param name="playerEntity">The player entity to give the potion to</param>
  /// <param name="timeRemaining">Time remaining until next potion is available (null if successful)</param>
  /// <returns>True if successful, false if on cooldown</returns>
  public static bool GiveHumanBloodPotion(Entity playerEntity, out TimeSpan? timeRemaining)
  {
    timeRemaining = null;

    // Get player data
    var playerData = PlayerDataService.GetPlayerData(playerEntity);

    // Check if they have used it before and if cooldown applies
    if (playerData.LastBloodPotionTime.HasValue)
    {
      var timeSinceLastUse = DateTime.Now - playerData.LastBloodPotionTime.Value;
      var cooldownPeriod = TimeSpan.FromHours(20);

      if (timeSinceLastUse < cooldownPeriod)
      {
        timeRemaining = cooldownPeriod - timeSinceLastUse;
        Core.Log.LogInfo($"Player {playerData.CharacterName} is on cooldown, {timeRemaining} remaining");
        return false;
      }
    }

    // the player entity has a Blood component
    if (EntityManager.TryGetComponentData<Blood>(playerEntity, out var bloodComponent))
    {
      float quality = bloodComponent.Quality;
      PrefabGUID bloodType = bloodComponent.BloodType;
      SecondaryBloodData secondaryBloodData = bloodComponent.SecondaryBlood;

      Entity itemEntity = AddItemToInventory(playerEntity, PrefabGUIDs.Item_Consumable_PrisonPotion, 1);
      if (itemEntity == Entity.Null)
      {
        // in this case the inventory is full. WHAT DO?
        return false;
      }

      StoredBlood blood = new StoredBlood()
      {
        BloodQuality = quality,
        PrimaryBloodType = bloodType,
        SecondaryBlood = new()
        {
          Quality = secondaryBloodData.Quality,
          Type = secondaryBloodData.Type,
          BuffIndex = secondaryBloodData.BuffIndex
        }
      };

      EntityManager.SetComponentData(itemEntity, blood);

      // Update the last blood potion time to now
      playerData.LastBloodPotionTime = DateTime.Now;

      // Save the updated player data
      PlayerDataService.SaveData();

      Core.Log.LogInfo($"GiveHumanBloodPotion successful for player: {playerData.CharacterName} (Steam ID: {playerData.SteamId})");

      return true;
    }

    return false;
  }

  public static Entity AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
  {
    try
    {
      ServerGameManager serverGameManager = Core.ServerGameManager;
      var inventoryResponse = serverGameManager.TryAddInventoryItem(recipient, guid, amount);

      return inventoryResponse.NewEntity;
    }
    catch (Exception e)
    {
      Core.Log.LogError($"Failed to add item {guid} to inventory of entity {recipient}: {e.Message}");
    }
    return new Entity();
  }

  public static void TestItemFunctionality(Entity playerEntity)
  {
    if (EntityManager.TryGetBuffer<InventoryInstanceElement>(playerEntity, out var inventoryBuffer))
    {
      Core.Log.LogInfo($"Player {playerEntity} has {inventoryBuffer.Length} items in inventory:");
      foreach (var inventoryInstance in inventoryBuffer)
      {
        InstanceCategory category = inventoryInstance.Category;
        Entity inventoryEntity = inventoryInstance.ExternalInventoryEntity._Entity; // External_Inventory PrefabGuid(1183666186)
        PrefabGUID prefabGUID = EntityManager.GetComponentData<PrefabGUID>(inventoryEntity);

        Core.Log.LogInfo($"- Inventory Entity: {inventoryEntity}, PrefabGUID: {prefabGUID}, Category: {category}");

        if (EntityManager.TryGetBuffer<InventoryBuffer>(inventoryEntity, out var itemInventoryBuffer))
        {
          Core.Log.LogInfo($"  - Item has {itemInventoryBuffer.Length} inventory buffer elements:");
          foreach (InventoryBuffer itemElement in itemInventoryBuffer)
          {
            Entity itemEntity = itemElement.ItemEntity._Entity;
            PrefabGUID itemTypePrefabGUID = itemElement.ItemType;
            int amount = itemElement.Amount;
            int maxAmountOverride = itemElement.MaxAmountOverride;
            // Core.Log.LogInfo($"    - Item Entity: {itemEntity}, ItemType PrefabGUID: {PrefabGUIDsExtensions.GetPrefabGUIDName(itemTypePrefabGUID)}, Amount: {amount}, MaxAmountOverride: {maxAmountOverride}");

            if (!EntityManager.Exists(itemEntity))
            {
              // Core.Log.LogInfo($"      - Item Entity {itemEntity} does not exist.");
              continue;
            }

            PrefabGUID itemEntityPrefabGUID = EntityManager.GetComponentData<PrefabGUID>(itemEntity);
            // Core.Log.LogInfo($"      - Item Entity PrefabGUID: {PrefabGUIDsExtensions.GetPrefabGUIDName(itemEntityPrefabGUID)} ");
            if (itemEntityPrefabGUID.Equals(PrefabGUIDs.Item_Weapon_Sword_T01_Bone))
            {
              Core.Log.LogInfo($"        - This is a Bone Sword!");
              if (!EntityManager.Exists(itemEntity)) return;

              ItemData itemData = ItemDataService.GetItemData(itemEntity);
              Core.Log.LogInfo($"        - Retrieved ItemData: PrefabGUIDName={itemData.PrefabGUIDName}, SequenceGuidHash={itemData.SequenceGuidHash}");

              // var componentTypes = EntityManager.GetComponentTypes(itemEntity);
              // Core.Log.LogInfo($"Components on entity '{itemEntity}':");
              // foreach (var componentType in componentTypes)
              // {
              //   Core.Log.LogInfo($"- {componentType.GetManagedType().Name}");
              // }
            }
          }
        }
        else
        {
          Core.Log.LogInfo($"  - Item has no inventory buffer.");
        }
      }
    }
    else
    {
      Core.Log.LogInfo($"Player {playerEntity} has no inventory buffer.");
    }
  }

  public static Entity GetHeldWeaponEntity(Entity playerEntity)
  {
    if (EntityManager.TryGetComponentData<Equipment>(playerEntity, out var equipment))
    {
      EquipmentSlot weaponSlot = equipment.WeaponSlot;
      Entity weaponEntity = weaponSlot.SlotEntity._Entity;

      if (EntityManager.Exists(weaponEntity))
      {
        return weaponEntity;
      }
    }

    return Entity.Null;
  }

  public static int GetItemUniqueId(Entity itemEntity)
  {
    int guidHash = 0;
    if (EntityManager.TryGetComponentData(itemEntity, out SequenceGUID guid))
    {
      guidHash = guid.GuidHash;
      Core.Log.LogInfo($"Item Sequence GUID for entity {itemEntity}: {guidHash}");
      return guidHash;
    }
    else
    {
      Core.Log.LogInfo($"Entity {itemEntity} does not have an ItemUniqueID component.");
      guidHash = Guid.NewGuid().GetHashCode();
      // TODO CHECK IF WE ALREADY HAVE THIS GUID IN USE?

      guid = new SequenceGUID(guidHash);
      EntityManager.AddComponentData(itemEntity, guid);
      Core.Log.LogInfo($"Set Item Sequence GUID for entity {itemEntity} to: {guidHash}");
    }

    return guidHash;
  }
}
