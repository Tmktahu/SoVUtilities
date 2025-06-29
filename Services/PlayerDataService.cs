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
  // Main data store - maps Steam IDs to player data
  private static Dictionary<ulong, PlayerData> _playerData = new Dictionary<ulong, PlayerData>();
  // an array of valid tags that can be used
  public static readonly string[] ValidTags =
  {
    SoftlockService.ADMIN_TAG,
    SoftlockService.COMPASS_TAG,
    SoftlockService.SHEPHERDS_TAG,
    SoftlockService.AEGIS_TAG,
    SoftlockService.OAKSONG_TAG,
    HUMAN_TAG
  };

  public static void Initialize()
  {
    // Create directory if it doesn't exist
    Directory.CreateDirectory(SaveDirectory);

    // Load existing data
    LoadData();
  }

  public static PlayerData GetPlayerData(Entity characterEntity)
  {
    ulong steamId = characterEntity.GetSteamId();
    string characterName = characterEntity.GetUser().CharacterName.ToString();

    if (!_playerData.TryGetValue(steamId, out var data))
    {
      data = new PlayerData
      {
        SteamId = steamId,
        CharacterName = characterName
      };
      _playerData[steamId] = data;
    }

    return data;
  }

  public static bool AddPlayerTag(Entity characterEntity, string tag)
  {
    var data = GetPlayerData(characterEntity);
    bool added = data.AddTag(tag);
    if (added)
      SaveData();
    return added;
  }

  public static bool RemovePlayerTag(ulong steamId, string tag)
  {
    if (!_playerData.TryGetValue(steamId, out var data))
      return false;

    bool removed = data.RemoveTag(tag);
    if (removed)
      SaveData();
    return removed;
  }

  public static bool RemovePlayerTag(Entity entity, string tag)
  {
    ulong steamId = entity.GetSteamId();
    if (steamId == 0)
      return false;

    return RemovePlayerTag(steamId, tag);
  }

  public static bool HasPlayerTag(ulong steamId, string tag)
  {
    if (!_playerData.TryGetValue(steamId, out var data))
      return false;

    return data.HasTag(tag);
  }

  public static bool HasPlayerTag(Entity entity, string tag)
  {
    ulong steamId = entity.GetSteamId();
    if (steamId == 0)
      return false;

    return HasPlayerTag(steamId, tag);
  }

  public static List<PlayerData> GetPlayersWithTag(string tag)
  {
    return _playerData
        .Values
        .Where(data => data.HasTag(tag))
        .ToList();
  }

  public static void SaveData()
  {
    try
    {
      // Convert to a dictionary with string keys for serialization
      string json = JsonSerializer.Serialize(_playerData);
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
        _playerData = JsonSerializer.Deserialize<Dictionary<ulong, PlayerData>>(json)
            ?? new Dictionary<ulong, PlayerData>();
      }
      catch (Exception ex)
      {
        Core.Log.LogError($"Failed to load player data: {ex.Message}");
        _playerData = new Dictionary<ulong, PlayerData>();
      }
    }
  }

  public static string[] GetValidTags()
  {
    return ValidTags;
  }

  public static bool IsValidTag(string tag)
  {
    return ValidTags.Contains(tag);
  }

  public static Dictionary<string, List<string>> GetAllTags()
  {
    return _playerData.ToDictionary(
        kvp => kvp.Value.CharacterName,
        kvp => kvp.Value.Tags);
  }
}
