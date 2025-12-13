using Unity.Entities;
using static SoVUtilities.Services.PlayerDataService;
using SoVUtilities.Models;

namespace SoVUtilities.Services;

public static class TagService
{
  // Define all valid tags
  public static class Tags
  {
    public const string ADMIN = "admin";
    public const string AFFLICTED = "afflicted";
    public const string BEHOLDEN = "beholden";
    public const string ENCASED = "encased";
    public const string CONSUMED = "consumed";
    public const string SEEDED = "seeded";
    public const string HIDE_NAMEPLATE = "hide_nameplate";
    public const string HUMAN = "human";
    public const string COMPASS = "compass";
    public const string SHEPHERDS = "shepherds";
    public const string AEGIS = "aegis";
    public const string OAKSONG = "oaksong";
    public const string STYX = "styx";
    public const string RELIC = "relic";
    public const string SPIRIT_CHOSEN = "spiritchosen";
    public const string WEREWOLF = "werewolf";
  }

  // Check if a tag is valid
  public static bool IsValidTag(string tag)
  {
    return tag switch
    {
      Tags.ADMIN => true,
      Tags.AFFLICTED => true,
      Tags.BEHOLDEN => true,
      Tags.ENCASED => true,
      Tags.CONSUMED => true,
      Tags.SEEDED => true,
      Tags.HIDE_NAMEPLATE => true,
      Tags.HUMAN => true,
      Tags.COMPASS => true,
      Tags.SHEPHERDS => true,
      Tags.AEGIS => true,
      Tags.OAKSONG => true,
      Tags.STYX => true,
      Tags.RELIC => true,
      Tags.SPIRIT_CHOSEN => true,
      Tags.WEREWOLF => true,
      _ => false
    };
  }

  // Get all valid tags
  public static IEnumerable<string> GetAllTags()
  {
    yield return Tags.ADMIN;
    yield return Tags.AFFLICTED;
    yield return Tags.BEHOLDEN;
    yield return Tags.ENCASED;
    yield return Tags.CONSUMED;
    yield return Tags.SEEDED;
    yield return Tags.HIDE_NAMEPLATE;
    yield return Tags.HUMAN;
    yield return Tags.COMPASS;
    yield return Tags.SHEPHERDS;
    yield return Tags.AEGIS;
    yield return Tags.OAKSONG;
    yield return Tags.STYX;
    yield return Tags.RELIC;
    yield return Tags.SPIRIT_CHOSEN;
    yield return Tags.WEREWOLF;
  }

  // we need to map tags to buff string IDs array
  public static readonly Dictionary<string, string[]> TagToBuffMap = new()
  {
    { Tags.AFFLICTED, new[] { BuffService.AfflictedBuffId } },
    { Tags.BEHOLDEN, new[] { BuffService.BeholdenBuffId } },
    { Tags.ENCASED, new[] { BuffService.EncasedBuffId } },
    { Tags.CONSUMED, new[] { BuffService.ConsumedBuffId } },
    { Tags.SEEDED, new[] { BuffService.SeededBuffId } },
    { Tags.HIDE_NAMEPLATE, new[] { BuffService.HideNameplateBuffId } },
    { Tags.HUMAN, new[] { BuffService.HumanBuffId } },
    { Tags.RELIC, new[] { BuffService.RelicBuffId } },
    { Tags.SPIRIT_CHOSEN, new[] { BuffService.SpiritChosenBuffId } }
  };

  public static bool AddPlayerTag(Entity characterEntity, string tag)
  {
    var data = GetPlayerData(characterEntity);
    bool added = data.AddTag(tag);
    if (added)
      SaveData();
    return added;
  }

  public static bool RemovePlayerTag(Entity entity, string tag)
  {
    ulong steamId = entity.GetSteamId();
    if (steamId == 0)
      return false;

    var data = GetPlayerData(entity);

    bool removed = data.RemoveTag(tag);
    if (removed)
      SaveData();
    return removed;
  }

  public static bool HasPlayerTag(Entity entity, string tag)
  {
    ulong steamId = entity.GetSteamId();
    if (steamId == 0)
      return false;

    var data = GetPlayerData(entity);
    return data.HasTag(tag);
  }

  // we want a function to know if a player should have a target buff
  public static bool ShouldHaveBuff(Entity characterEntity, string buffId)
  {
    // Check if the character has the required tags for the buff
    // first we get the tags the player has
    var data = GetPlayerData(characterEntity);

    // then we loop through the tags the player has
    foreach (var tag in data.Tags)
    {
      // if the tag is in the TagToBuffMap, we check if the buffId is in the array
      if (TagToBuffMap.TryGetValue(tag, out var buffIds))
      {
        if (buffIds.Contains(buffId))
        {
          return true;
        }
      }
    }

    return false;
  }

  public static Dictionary<string, List<string>> GetAllTagInfo()
  {
    // so this function will return a dictionary of all players and their tags
    var allTags = new Dictionary<string, List<string>>();

    var data = GetAllPlayerData();
    foreach (var playerData in data.Values)
    {
      if (playerData.Tags.Count > 0)
      {
        // if the name is empty or null, we set it to "Unknown"
        if (string.IsNullOrEmpty(playerData.CharacterName))
        {
          playerData.CharacterName = "Unknown DAFUQ HAPPENED";
        }

        allTags[playerData.CharacterName] = playerData.Tags;
      }
    }

    return allTags;
  }

  // public static List<PlayerData> GetPlayersWithTag(string tag)
  // {
  //   return _playerData
  //       .Values
  //       .Where(data => data.HasTag(tag))
  //       .ToList();
  // }
}