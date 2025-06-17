using System.Text.Json;
using SoVUtilities.Models;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace SoVUtilities.Services;

public static class PlayerDataService
{
  // Path for saving player data
  private static readonly string SaveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BepInEx", "config", "DevTools");
  private static readonly string SavePath = Path.Combine(SaveDirectory, "sov_utilities_player_data.json");

  // Main data store - maps Steam IDs to player data
  private static Dictionary<ulong, PlayerData> _playerData = new Dictionary<ulong, PlayerData>();
  // an array of valid tags that can be used
  public static readonly string[] ValidTags =
  {
    SoftlockService.ADMIN_TAG,
    SoftlockService.COMPASS_TAG,
    SoftlockService.SHEPHERDS_TAG,
    SoftlockService.AEGIS_TAG,
    SoftlockService.OAKSONG_TAG
  };

  public static void Initialize()
  {
    // Create directory if it doesn't exist
    Directory.CreateDirectory(SaveDirectory);

    // Load existing data
    LoadData();
  }

  public static PlayerData GetPlayerData(ulong steamId, string playerName = null)
  {
    if (!_playerData.TryGetValue(steamId, out var data))
    {
      data = new PlayerData
      {
        SteamId = steamId,
        PlayerName = playerName ?? $"Player_{steamId}"
      };
      _playerData[steamId] = data;
    }

    // Update name if provided
    if (playerName != null && data.PlayerName != playerName)
    {
      data.PlayerName = playerName;
      data.LastUpdated = DateTime.UtcNow;
    }

    return data;
  }

  public static PlayerData GetPlayerData(Entity entity)
  {
    ulong steamId = entity.GetSteamId();
    if (steamId == 0)
      return null;

    string name = GetPlayerName(entity);
    return GetPlayerData(steamId, name);
  }

  public static bool AddPlayerTag(ulong steamId, string tag)
  {
    var data = GetPlayerData(steamId);
    bool added = data.AddTag(tag);
    if (added)
      SaveData();
    return added;
  }

  public static bool AddPlayerTag(Entity entity, string tag)
  {
    ulong steamId = entity.GetSteamId();
    if (steamId == 0)
      return false;

    return AddPlayerTag(steamId, tag);
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

  private static string GetPlayerName(Entity entity)
  {
    var entityManager = Core.EntityManager;

    // For character entities, get the character name if possible
    if (entityManager.HasComponent<PlayerCharacter>(entity))
    {
      Entity userEntity = entityManager.GetComponentData<PlayerCharacter>(entity).UserEntity;

      if (entityManager.HasComponent<User>(userEntity))
      {
        var user = entityManager.GetComponentData<User>(userEntity);
        return user.CharacterName.ToString();
      }
    }

    // For user entities, get the user name
    if (entityManager.HasComponent<User>(entity))
    {
      var user = entityManager.GetComponentData<User>(entity);
      return user.CharacterName.ToString();
    }

    return null;
  }

  public static string[] GetValidTags()
  {
    return ValidTags;
  }

  public static bool IsValidTag(string tag)
  {
    return ValidTags.Contains(tag);
  }
}
