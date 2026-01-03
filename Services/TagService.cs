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

    // Region Tags
    public const string CURSED_FOREST = "cursed_forest";
    public const string DUNLEY_FARMLANDS = "dunley_farmlands";
    public const string FARBANE_WOODS = "farbane_woods";
    public const string GLOOMROT_NORTH = "gloomrot_north";
    public const string GLOOMROT_SOUTH = "gloomrot_south";
    public const string HALLOWED_MOUNTAINS = "hallowed_mountains";
    public const string NONE_REGION = "none_region";
    public const string OTHER_REGION = "other_region";
    public const string RUINS_OF_MORTIUM = "ruins_of_mortium";
    public const string SILVERLIGHT_HILLS = "silverlight_hills";
    public const string START_CAVE = "start_cave";
    public const string OAKVEIL_FOREST = "oakveil_forest";

    // Faction Tags
    public const string FACTION_NOCTUM = "noctum"; // will be faction index 16
    public const string FACTION_CULT = "cult_of_the_damned";
    // leandra is faction index 29, same with skeleboys in the area
    // undead dudes in forest are also 29
    // cursed forest mobs are faction index 25

    public const string ROTLING = "rotling";
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
    if (IsValidRegionTag(tag))
      return true;
    if (IsValidFactionTag(tag))
      return true;
    return tag switch
    {
      Tags.ADMIN => true,
      Tags.ROTLING => true,
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

  // Check if a tag is a valid region tag
  public static bool IsValidRegionTag(string tag)
  {
    return tag switch
    {
      Tags.CURSED_FOREST => true,
      Tags.DUNLEY_FARMLANDS => true,
      Tags.FARBANE_WOODS => true,
      Tags.GLOOMROT_NORTH => true,
      Tags.GLOOMROT_SOUTH => true,
      Tags.HALLOWED_MOUNTAINS => true,
      Tags.NONE_REGION => true,
      Tags.OTHER_REGION => true,
      Tags.RUINS_OF_MORTIUM => true,
      Tags.SILVERLIGHT_HILLS => true,
      Tags.START_CAVE => true,
      Tags.OAKVEIL_FOREST => true,
      _ => false
    };
  }

  public static bool IsValidFactionTag(string tag)
  {
    return tag switch
    {
      Tags.FACTION_NOCTUM => true,
      Tags.FACTION_CULT => true,
      _ => false
    };
  }

  // Get all valid tags
  public static IEnumerable<string> GetAllTags()
  {
    yield return Tags.ADMIN;

    // Faction Tags
    yield return Tags.FACTION_NOCTUM;
    yield return Tags.FACTION_CULT;

    yield return Tags.ROTLING;
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
    { Tags.SPIRIT_CHOSEN, new[] { BuffService.SpiritChosenBuffId } },
    { Tags.WEREWOLF, new[] { BuffService.WerewolfStatsBuffId } },
    { Tags.ROTLING, new[] { BuffService.RotlingBuffId } }
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