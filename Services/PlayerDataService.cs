using System.Text.Json;
using SoVUtilities.Models;
using Unity.Entities;

namespace SoVUtilities.Services;

public static class PlayerDataService
{
  // Path for saving player data
  private static readonly string SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "config", "SoVUtilities");
  private static readonly string SavePath = Path.Combine(SaveDirectory, "sov_utilities_player_data.json");
  public static readonly string HUMAN_TAG = "human";
  // Main data store - list of all player data
  private static List<PlayerData> _playerDataList = new List<PlayerData>();

  private static void MigrateLegacyPlayerData()
  {
    // Only run if the file exists
    if (File.Exists(SavePath))
    {
      string json = File.ReadAllText(SavePath);
      bool isMigrated = false;
      try
      {
        // Try to deserialize as array (new format)
        var arrayCheck = JsonSerializer.Deserialize<List<PlayerData>>(json);
        if (arrayCheck != null && arrayCheck.Count > 0)
        {
          // Already migrated, skip migration silently
          isMigrated = true;
        }
      }
      catch { /* Ignore, try legacy next */ }

      if (isMigrated) return;

      try
      {
        // Try to deserialize as dictionary (legacy format)
        var legacyDict = JsonSerializer.Deserialize<Dictionary<ulong, PlayerData>>(json);
        if (legacyDict != null && legacyDict.Count > 0)
        {
          Core.Log.LogInfo($"Found legacy player data. Records: {legacyDict.Count}");
          // Convert to list
          var playerList = legacyDict.Values.ToList();
          // Save as array
          string newJson = JsonSerializer.Serialize(playerList);
          File.WriteAllText(SavePath, newJson);
          Core.Log.LogInfo($"Migrated legacy player data to array format. Records: {playerList.Count}");
          return;
        }
      }
      catch { /* Ignore, check for truly invalid file below */ }

      // If neither deserialization worked, log error
      Core.Log.LogError("Player data file is invalid or corrupt. Migration failed.");
    }
  }

  // Call migration before loading data
  public static void Initialize()
  {
    // Create directory if it doesn't exist
    Directory.CreateDirectory(SaveDirectory);
    MigrateLegacyPlayerData();

    // Load existing data
    LoadData();
  }

  public static PlayerData GetPlayerData(Entity characterEntity)
  {
    ulong steamId = characterEntity.GetSteamId();
    string characterName = characterEntity.GetUser().CharacterName.ToString();
    if (string.IsNullOrEmpty(characterName)) characterName = "Unknown DAFUQ HAPPENED";

    int guidHash = 0;
    if (Core.EntityManager.HasComponent<ProjectM.SequenceGUID>(characterEntity))
    {
      var seqGuid = Core.EntityManager.GetComponentData<ProjectM.SequenceGUID>(characterEntity);
      guidHash = seqGuid.GuidHash;
    }

    // Try to find by GUID hash
    if (guidHash != 0)
    {
      var guidData = _playerDataList.Find(pd => pd.GuidHash == guidHash);
      if (guidData != null)
        return guidData;
    }

    // Try to find legacy by CharacterName and GuidHash == 0
    var legacyByName = _playerDataList.Find(pd => pd.CharacterName == characterName && pd.GuidHash == 0);
    if (legacyByName != null)
    {
      var newGuid = new ProjectM.SequenceGUID(System.Guid.NewGuid().GetHashCode());
      Core.EntityManager.AddComponentData(characterEntity, newGuid);
      legacyByName.GuidHash = newGuid.GuidHash;
      guidHash = newGuid.GuidHash;
      SaveData();
      return legacyByName;
    }

    // If neither found, create new player data
    var newData = new PlayerData
    {
      SteamId = steamId,
      CharacterName = characterName,
      GuidHash = guidHash != 0 ? guidHash : System.Guid.NewGuid().GetHashCode()
    };
    if (guidHash == 0)
    {
      var newGuid = new ProjectM.SequenceGUID(newData.GuidHash);
      Core.EntityManager.AddComponentData(characterEntity, newGuid);
    }
    _playerDataList.Add(newData);
    SaveData();
    return newData;
  }

  // function to get all player data
  public static Dictionary<ulong, PlayerData> GetAllPlayerData()
  {
    // Return all player data as a dictionary by SteamId
    var all = new Dictionary<ulong, PlayerData>();
    foreach (var pd in _playerDataList)
    {
      all[pd.SteamId] = pd;
    }
    return all;
  }

  public static void SaveData()
  {
    try
    {
      // Save as a single JSON array
      string json = JsonSerializer.Serialize(_playerDataList);
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
        _playerDataList = JsonSerializer.Deserialize<List<PlayerData>>(json)
          ?? new List<PlayerData>();
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to load player data: {ex.Message}");
        _playerDataList = new List<PlayerData>();
      }
    }
  }
}
