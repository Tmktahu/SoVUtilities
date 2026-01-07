using System.Text.Json;
using ProjectM;
using SoVUtilities.Models;
using SoVUtilities.Resources;
using Stunlock.Core;
using Unity.Entities;

namespace SoVUtilities.Services;

public
struct ItemData(string prefabGUIDName = "", int itemPrefabGUID = 0, int sequenceGuidHash = 0, int[] abilityGUIDs = null)
{
  public string PrefabGUIDName { get; set; } = prefabGUIDName;
  public int PrefabGUID { get; set; } = itemPrefabGUID;
  public int SequenceGuidHash { get; set; } = sequenceGuidHash;
  public int[] AbilityGUIDs { get; set; } = abilityGUIDs;
}

public static class ItemDataService
{
  // Path for saving player data
  private static readonly string SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "config", "SoVUtilities");
  private static readonly string SavePath = Path.Combine(SaveDirectory, "sov_utilities_item_data.json");
  private static List<ItemData> _itemDataList = new List<ItemData>();
  private static Dictionary<int, ItemData> _itemDataCache = new Dictionary<int, ItemData>();

  public static void Initialize()
  {
    // Create directory if it doesn't exist
    Directory.CreateDirectory(SaveDirectory);

    // Load existing data
    LoadData();
  }

  public static bool HasItemData(Entity itemEntity)
  {
    int guidHash = 0;
    if (Core.EntityManager.HasComponent<SequenceGUID>(itemEntity))
    {
      var seqGuid = Core.EntityManager.GetComponentData<SequenceGUID>(itemEntity);
      guidHash = seqGuid.GuidHash;
      Core.Log.LogInfo($"Checking ItemData for entity {itemEntity} with Sequence GUID hash {guidHash}");
    }

    // Try to find by GUID hash
    if (guidHash != 0)
    {
      Core.Log.LogInfo($"Looking up ItemData in cache for GUID hash {guidHash}");
      var hasData = _itemDataCache.ContainsKey(guidHash);
      Core.Log.LogInfo($"Has ItemData: {hasData}");
      return hasData;
    }

    return false;
  }

  public static ItemData GetItemData(Entity itemEntity)
  {
    int guidHash = 0;
    if (Core.EntityManager.HasComponent<SequenceGUID>(itemEntity))
    {
      var seqGuid = Core.EntityManager.GetComponentData<SequenceGUID>(itemEntity);
      guidHash = seqGuid.GuidHash;
      Core.Log.LogInfo($"Getting ItemData for entity {itemEntity} with Sequence GUID hash {guidHash}");
    }

    // Try to find by GUID hash
    if (guidHash != 0)
    {
      Core.Log.LogInfo($"Looking up ItemData in cache for GUID hash {guidHash}");
      var guidData = _itemDataCache.ContainsKey(guidHash) ? _itemDataCache[guidHash] : default;
      Core.Log.LogInfo($"Found ItemData: PrefabGUIDName={guidData.PrefabGUIDName}, SequenceGuidHash={guidData.SequenceGuidHash}");
      if (guidData.PrefabGUIDName != "" && guidData.SequenceGuidHash != 0)
        return guidData;
    }

    // If neither found, create new item data
    PrefabGUID itemPrefabGUID = Core.EntityManager.GetComponentData<PrefabGUID>(itemEntity);
    string prefabName = PrefabGUIDsExtensions.GetPrefabGUIDName(itemPrefabGUID);

    guidHash = ItemService.GetItemUniqueId(itemEntity);

    var newData = new ItemData
    {
      PrefabGUIDName = prefabName,
      PrefabGUID = itemPrefabGUID._Value,
      SequenceGuidHash = guidHash,
      AbilityGUIDs = [0, 0, 0, 0, 0, 0, 0, 0]
    };

    _itemDataList.Add(newData);
    _itemDataCache[guidHash] = newData;
    SaveData();
    return newData;
  }

  // function to get all item data
  public static Dictionary<int, ItemData> GetAllItemData()
  {
    // Return all player data as a dictionary by SteamId
    var all = new Dictionary<int, ItemData>();
    foreach (var pd in _itemDataList)
    {
      all[pd.SequenceGuidHash] = pd;
    }
    return all;
  }

  public static void SaveData()
  {
    try
    {
      // Save as a single JSON array
      string json = JsonSerializer.Serialize(_itemDataList);
      File.WriteAllText(SavePath, json);
    }
    catch (Exception ex)
    {
      Core.Log.LogError($"Failed to save player data: {ex.Message}");
    }
  }

  private static void LoadData()
  {
    if (File.Exists(SavePath))
    {
      try
      {
        string json = File.ReadAllText(SavePath);
        _itemDataList = JsonSerializer.Deserialize<List<ItemData>>(json)
          ?? new List<ItemData>();

        // Populate cache
        _itemDataCache = new Dictionary<int, ItemData>();
        foreach (var itemData in _itemDataList)
        {
          _itemDataCache[itemData.SequenceGuidHash] = itemData;
        }

        Core.Log.LogInfo($"Loaded {_itemDataList.Count} item data entries from {SavePath}.");
        Core.Log.LogInfo($"Loaded {_itemDataCache.Count} item data entries into cache.");
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to load player data: {ex.Message}");
        _itemDataList = new List<ItemData>();
      }
    }
  }
}
