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

  // function to get all player data
  public static Dictionary<ulong, PlayerData> GetAllPlayerData()
  {
    return new Dictionary<ulong, PlayerData>(_playerData);
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
}
