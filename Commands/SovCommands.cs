using SoVUtilities.Services;
using System.Text;
using VampireCommandFramework;
using static SoVUtilities.Services.EntityService;
using Unity.Entities;

namespace SoVUtilities.Commands;

[CommandGroup("sov")]
internal static class SovCommands
{

  [Command("tag add", "Add a tag to the target player", adminOnly: true)]
  public static void AddTagCommand(ChatCommandContext ctx, string tag, string playerName)
  {
    Entity playerEntity;
    Entity userEntity;

    if (string.IsNullOrEmpty(playerName))
    {
      // we apply it to the sender if no player name is provided
      playerEntity = ctx.Event.SenderCharacterEntity;
      playerName = playerEntity.GetUser().CharacterName.ToString();
    }
    else
    {
      if (!TryFindPlayer(playerName, out playerEntity, out userEntity))
      {
        ctx.Reply($"Player '{playerName}' not found or is offline.");
        return;
      }
    }

    if (TagService.IsValidTag(tag) == false)
    {
      ctx.Reply($"Invalid tag '{tag}'.");
      return;
    }

    if (TagService.AddPlayerTag(playerEntity, tag))
    {
      BuffService.RefreshPlayerBuffs(playerEntity).Start();
      ctx.Reply($"Added tag '{tag}' to character '{playerName}'.");
    }
    else
    {
      ctx.Reply($"Character '{playerName}' already has the tag '{tag}'.");
    }
  }

  [Command("tag remove", "Remove a tag from the target player", adminOnly: true)]
  public static void RemoveTagCommand(ChatCommandContext ctx, string tag, string playerName = null)
  {
    Entity playerEntity;
    Entity userEntity;

    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = ctx.Event.SenderCharacterEntity;
    }
    else
    {
      if (!TryFindPlayer(playerName, out playerEntity, out userEntity))
      {
        ctx.Reply($"Player '{playerName}' not found.");
        return;
      }
    }

    if (TagService.IsValidTag(tag) == false)
    {
      ctx.Reply($"Invalid tag '{tag}'.");
      return;
    }

    if (TagService.RemovePlayerTag(playerEntity, tag))
    {
      BuffService.RefreshPlayerBuffs(playerEntity).Start();
      ctx.Reply($"Removed tag '{tag}' from character '{playerName}'.");
    }
    else
    {
      ctx.Reply($"Character '{playerName}' does not have the tag '{tag}'.");
    }
  }

  [Command("tag list", "List all tags of the target player", adminOnly: true)]
  public static void ListTagsCommand(ChatCommandContext ctx, string playerName = null)
  {
    Entity playerEntity;
    Entity userEntity;

    if (string.IsNullOrEmpty(playerName))
    {
      // we want to list all tags of all players
      Dictionary<string, List<string>> allTags = TagService.GetAllTagInfo();
      if (allTags.Count == 0)
      {
        ctx.Reply("No tags found in the system.");
        return;
      }

      foreach (var kvp in allTags)
      {
        string currentPlayerName = kvp.Key;
        var tags = kvp.Value;

        tags = tags.ToList();

        ctx.Reply($"Tags for character '{currentPlayerName}': {string.Join(", ", tags)}");
      }
      return;
    }

    if (TryFindPlayer(playerName, out playerEntity, out userEntity))
    {
      var playerData = PlayerDataService.GetPlayerData(playerEntity);
      if (playerData == null || playerData.Tags.Count == 0)
      {
        ctx.Reply($"Character '{playerName}' doesn't have any tags.");
        return;
      }

      var sb = new StringBuilder($"Tags for character '{playerName}': ");
      for (int i = 0; i < playerData.Tags.Count; i++)
      {
        if (i > 0) sb.Append(", ");
        sb.Append(playerData.Tags[i]);
      }

      ctx.Reply(sb.ToString());
    }
    else
    {
      ctx.Reply($"Player '{playerName}' not found.");
    }
  }

  // command that lists all valid tags
  [Command("tag listvalid", "List all valid tags in the system", adminOnly: true)]
  public static void ListAllTagsCommand(ChatCommandContext ctx)
  {
    string[] allTags = TagService.GetAllTags().ToArray();
    if (allTags.Length == 0)
    {
      ctx.Reply("No tags found in the system.");
      return;
    }

    var sb = new StringBuilder("Valid tags: ");
    for (int i = 0; i < allTags.Length; i++)
    {
      if (i > 0) sb.Append(", ");
      sb.Append(allTags[i]);
    }

    ctx.Reply(sb.ToString());
  }

  // command to refresh buffs for a player
  [Command("buff refresh", "Refresh buffs for the target player", adminOnly: true)]
  public static void RefreshBuffsCommand(ChatCommandContext ctx, string playerName = null)
  {
    Entity playerEntity;
    Entity userEntity;

    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = ctx.Event.SenderCharacterEntity;
    }
    else
    {
      if (!TryFindPlayer(playerName, out playerEntity, out userEntity))
      {
        ctx.Reply($"Player '{playerName}' not found.");
        return;
      }
    }

    BuffService.RefreshPlayerBuffs(playerEntity).Start();
    ctx.Reply($"Refreshed buffs for character '{playerName}'.");
  }
}