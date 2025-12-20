using System.Text.Json;
using Stunlock.Core;
using SoVUtilities.Resources;
using static SoVUtilities.Services.TagService;
using SoVUtilities.Services;

namespace SoVUtilities.Services;

public static class ModDataService
{
  // Path for saving mod data
  private static readonly string SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "config", "SoVUtilities");
  private static readonly string SavePath = Path.Combine(SaveDirectory, "sov_utilities_mod_data.json");

  // Main data store - generic dictionary for mod data
  public static Dictionary<string, object> ModData = new Dictionary<string, object>();

  // Data keys for backwards compatibility
  public const string SOFTLOCK_MAPPING_KEY = "softlock_mapping";
  public const string REGION_BUFF_MAPPING_KEY = "region_buff_mapping";

  public static void Initialize()
  {
    // Create directory if it doesn't exist
    Directory.CreateDirectory(SaveDirectory);

    // Load existing data
    LoadData();

    // Ensure backwards compatibility
    EnsureBackwardsCompatibility();
  }

  /// <summary>
  /// Ensures backwards compatibility by initializing any missing required fields
  /// </summary>
  private static void EnsureBackwardsCompatibility()
  {
    bool needsSave = false;

    // Initialize softlock mapping if it doesn't exist
    if (!ModData.ContainsKey(SOFTLOCK_MAPPING_KEY))
    {
      // Default softlock mapping for backwards compatibility
      var defaultSoftlockMapping = new Dictionary<string, PrefabGUID[]>
      {
        { Tags.COMPASS, new PrefabGUID[] { PrefabGUIDs.CHAR_ChurchOfLight_Cardinal_VBlood } },
        { Tags.SHEPHERDS, new PrefabGUID[] { PrefabGUIDs.CHAR_Militia_Nun_VBlood } },
        { Tags.AEGIS, new PrefabGUID[] { PrefabGUIDs.CHAR_Undead_ZealousCultist_VBlood } },
        { Tags.OAKSONG, new PrefabGUID[] { PrefabGUIDs.CHAR_Villager_CursedWanderer_VBlood } },
        { Tags.STYX, new PrefabGUID[] { PrefabGUIDs.CHAR_BatVampire_VBlood } }
      };

      ModData[SOFTLOCK_MAPPING_KEY] = defaultSoftlockMapping;
      needsSave = true;
      Core.Log.LogInfo("Initialized default softlock mapping for backwards compatibility");
    }

    // Initialize region buff mapping if it doesn't exist
    if (!ModData.ContainsKey(REGION_BUFF_MAPPING_KEY))
    {
      ModData[REGION_BUFF_MAPPING_KEY] = new Dictionary<int, RegionBuffConfig>();
      needsSave = true;
      Core.Log.LogInfo("Initialized default region buff mapping for backwards compatibility");
    }

    // Add other backwards compatibility initializations here as needed
    // Example:
    // if (!ModData.ContainsKey("some_other_key"))
    // {
    //   ModData["some_other_key"] = defaultValue;
    //   needsSave = true;
    // }

    // Save if any changes were made
    if (needsSave)
    {
      SaveData();
      Core.Log.LogInfo("Saved backwards compatibility updates to mod data");
    }
  }

  public static T GetData<T>(string key, T defaultValue = default(T))
  {
    if (ModData.TryGetValue(key, out var data) && data is JsonElement element)
    {
      try
      {
        return JsonSerializer.Deserialize<T>(element);
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to deserialize mod data for key '{key}': {ex.Message}");
        return defaultValue;
      }
    }
    return defaultValue;
  }

  public static void SetData<T>(string key, T value)
  {
    ModData[key] = value;
  }

  public static void SaveData()
  {
    try
    {
      // Create a copy of ModData for saving with translated softlock data
      var dataToSave = new Dictionary<string, object>(ModData);

      // Translate softlock mapping from PrefabGUID[] to boss key names for JSON storage
      if (dataToSave.ContainsKey(SOFTLOCK_MAPPING_KEY))
      {
        var softlockMapping = (Dictionary<string, PrefabGUID[]>)dataToSave[SOFTLOCK_MAPPING_KEY];
        var translatedMapping = TranslateSoftlockMappingForSave(softlockMapping);
        dataToSave[SOFTLOCK_MAPPING_KEY] = translatedMapping;
      }

      string json = JsonSerializer.Serialize(dataToSave);
      File.WriteAllText(SavePath, json);
    }
    catch (Exception ex)
    {
      Core.Log.LogError($"Failed to save mod data: {ex.Message}");
    }
  }

  private static void LoadData()
  {
    if (File.Exists(SavePath))
    {
      try
      {
        string json = File.ReadAllText(SavePath);
        var loadedData = JsonSerializer.Deserialize<Dictionary<string, object>>(json)
            ?? new Dictionary<string, object>();

        // Translate softlock mapping from boss key names back to PrefabGUID[] for runtime use
        if (loadedData.ContainsKey(SOFTLOCK_MAPPING_KEY))
        {
          var rawSoftlockData = loadedData[SOFTLOCK_MAPPING_KEY];
          var translatedMapping = TranslateSoftlockMappingForLoad(rawSoftlockData);
          loadedData[SOFTLOCK_MAPPING_KEY] = translatedMapping;
        }

        ModData = loadedData;
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to load mod data: {ex.Message}");
        ModData = new Dictionary<string, object>();
      }
    }
    else
    {
      // File doesn't exist, create it with empty data
      ModData = new Dictionary<string, object>();
      SaveData();
      Core.Log.LogInfo("Created new mod data file on first run");
    }
  }

  /// <summary>
  /// Translates softlock mapping from PrefabGUID[] to boss key names for JSON storage
  /// </summary>
  /// <param name="mapping">Runtime mapping with PrefabGUID arrays</param>
  /// <returns>Mapping with boss key name arrays for JSON storage</returns>
  private static Dictionary<string, string[]> TranslateSoftlockMappingForSave(Dictionary<string, PrefabGUID[]> mapping)
  {
    var result = new Dictionary<string, string[]>();

    // Get available bosses from SoftlockService
    var availableBosses = SoftlockService.AvailableBosses;

    foreach (var kvp in mapping)
    {
      var bossKeys = new List<string>();

      foreach (var prefabGuid in kvp.Value)
      {
        // Find the boss key for this PrefabGUID
        var bossKey = availableBosses.FirstOrDefault(x => x.Value.Equals(prefabGuid)).Key;
        if (!string.IsNullOrEmpty(bossKey))
        {
          bossKeys.Add(bossKey);
        }
        else
        {
          Core.Log.LogWarning($"Could not find boss key for PrefabGUID when saving: {prefabGuid}");
        }
      }

      result[kvp.Key] = bossKeys.ToArray();
    }

    return result;
  }

  /// <summary>
  /// Translates softlock mapping from boss key names back to PrefabGUID[] for runtime use
  /// </summary>
  /// <param name="rawData">Raw JSON data that might be boss key names or PrefabGUIDs</param>
  /// <returns>Runtime mapping with PrefabGUID arrays</returns>
  private static Dictionary<string, PrefabGUID[]> TranslateSoftlockMappingForLoad(object rawData)
  {
    var result = new Dictionary<string, PrefabGUID[]>();

    try
    {
      // Get available bosses from SoftlockService
      var availableBosses = SoftlockService.AvailableBosses;

      if (rawData is JsonElement jsonElement)
      {
        var stringMapping = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonElement);

        if (stringMapping != null)
        {
          foreach (var kvp in stringMapping)
          {
            var prefabGuids = new List<PrefabGUID>();

            foreach (var bossKey in kvp.Value)
            {
              if (availableBosses.TryGetValue(bossKey, out PrefabGUID prefabGuid))
              {
                prefabGuids.Add(prefabGuid);
              }
              else
              {
                Core.Log.LogWarning($"Could not find PrefabGUID for boss key when loading: {bossKey}");
              }
            }

            result[kvp.Key] = prefabGuids.ToArray();
          }
        }
      }
      else
      {
        // Fallback: try to deserialize as the old format (Dictionary<string, PrefabGUID[]>)
        Core.Log.LogInfo("Attempting to load softlock mapping as legacy PrefabGUID format");
        var legacyMapping = (Dictionary<string, PrefabGUID[]>)rawData;
        result = legacyMapping;
      }
    }
    catch (Exception ex)
    {
      Core.Log.LogError($"Failed to translate softlock mapping on load: {ex.Message}");
    }

    return result;
  }

  // Region Buff Mapping helpers
  public static Dictionary<int, RegionBuffConfig> GetRegionBuffMapping()
  {
    if (ModData.TryGetValue(REGION_BUFF_MAPPING_KEY, out var data) && data is JsonElement element)
    {
      try
      {
        return JsonSerializer.Deserialize<Dictionary<int, RegionBuffConfig>>(element)
               ?? new Dictionary<int, RegionBuffConfig>();
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to deserialize region buff mapping: {ex.Message}");
        return new Dictionary<int, RegionBuffConfig>();
      }
    }
    else if (ModData.TryGetValue(REGION_BUFF_MAPPING_KEY, out var dictObj) && dictObj is Dictionary<int, RegionBuffConfig> dict)
    {
      return dict;
    }
    return new Dictionary<int, RegionBuffConfig>();
  }

  public static void SetRegionBuffConfig(int regionId, RegionBuffConfig config)
  {
    var mapping = GetRegionBuffMapping();
    mapping[regionId] = config;
    ModData[REGION_BUFF_MAPPING_KEY] = mapping;
    SaveData();
  }

  public static void RemoveRegionBuffConfig(int regionId)
  {
    var mapping = GetRegionBuffMapping();
    if (mapping.Remove(regionId))
    {
      ModData[REGION_BUFF_MAPPING_KEY] = mapping;
      SaveData();
    }
  }
}