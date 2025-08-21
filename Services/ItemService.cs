using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared;
using SoVUtilities.Resources;
using Stunlock.Core;
using Unity.Entities;

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
}
