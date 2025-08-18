using SoVUtilities.Services;
using System.Text;
using VampireCommandFramework;
using static SoVUtilities.Services.EntityService;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Stunlock.Core;

namespace SoVUtilities.Commands;

[CommandGroup("sov")]
internal static class SovCommands
{

  [Command("tag add", "Add a tag to the target player", adminOnly: true)]
  public static void AddTagCommand(ChatCommandContext ctx, string tag, string playerName)
  {
    Entity playerEntity;
    Entity userEntity;
    string lowercaseTag = tag.ToLowerInvariant();

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

    if (TagService.IsValidTag(lowercaseTag) == false)
    {
      ctx.Reply($"Invalid tag '{lowercaseTag}'.");
      return;
    }

    if (TagService.AddPlayerTag(playerEntity, lowercaseTag))
    {
      BuffService.RefreshPlayerBuffs(playerEntity).Start();
      ctx.Reply($"Added tag '{lowercaseTag}' to character '{playerName}'.");
    }
    else
    {
      ctx.Reply($"Character '{playerName}' already has the tag '{lowercaseTag}'.");
    }
  }

  [Command("tag remove", "Remove a tag from the target player", adminOnly: true)]
  public static void RemoveTagCommand(ChatCommandContext ctx, string tag, string playerName = null)
  {
    Entity playerEntity;
    Entity userEntity;
    string lowercaseTag = tag.ToLowerInvariant();

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

    if (TagService.IsValidTag(lowercaseTag) == false)
    {
      ctx.Reply($"Invalid tag '{lowercaseTag}'.");
      return;
    }

    if (TagService.RemovePlayerTag(playerEntity, lowercaseTag))
    {
      BuffService.RefreshPlayerBuffs(playerEntity).Start();
      ctx.Reply($"Removed tag '{lowercaseTag}' from character '{playerName}'.");
    }
    else
    {
      ctx.Reply($"Character '{playerName}' does not have the tag '{lowercaseTag}'.");
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
  public static void RefreshBuffsCommand(ChatCommandContext ctx, string playerName)
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

  // version of the buff refresh command for the public that only refreshes the users's buffs
  [Command("buff refresh", "Refresh your own buffs", adminOnly: false)]
  public static void RefreshSelfBuffsCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    BuffService.RefreshPlayerBuffs(playerEntity).Start();
    ctx.Reply("Your buffs have been refreshed.");
  }

  // we want a public command for players to use that reveals their nameplate
  [Command("reveal", "Reveal your nameplate", adminOnly: false)]
  public static void RevealNameplateCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    BuffService.RemoveHideNameplateBuff(playerEntity, true);
    ctx.Reply("Your nameplate has been revealed.");
  }

  // command to list nearby players with hide nameplate buff
  [Command("who", "List nearby players with hide nameplate buff", adminOnly: true)]
  public static void ListNearbyPlayersWithHideNameplateCommand(ChatCommandContext ctx, float radius = 10f)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    var nearbyPlayers = BuffService.NearbyPlayersHaveHideNameplateBuff(playerEntity, radius);
    if (nearbyPlayers.Count == 0)
    {
      ctx.Reply("No nearby players have the hide nameplate buff.");
      return;
    }

    var playerNames = nearbyPlayers.Select(p => p.GetUser().CharacterName).ToList();
    ctx.Reply($"Nearby players with hide nameplate buff: {string.Join(", ", playerNames)}");
  }

  // command to AOE unlock a vblood for all players
  [Command("vblood unlock aoe", "Unlock a vblood for all players", adminOnly: true)]
  public static void UnlockVBloodAOECommand(ChatCommandContext ctx, string vBloodName, int radius = 10)
  {
    // Implementation for unlocking the VBlood for all players
    string lowercaseVBloodName = vBloodName.ToLowerInvariant();
    string[] availableVBloods = ProgressionService.GetAvailableVBloodUnlocks();

    if (!availableVBloods.Contains(lowercaseVBloodName))
    {
      ctx.Reply($"Invalid VBlood name '{lowercaseVBloodName}'. Available options: {string.Join(", ", availableVBloods)}");
      return;
    }

    foreach (var userEntity in GetNearbyUserEntities(ctx.Event.SenderCharacterEntity, radius))
    {
      Entity character = userEntity.GetUser().LocalCharacter._Entity;
      ProgressionService.UnlockVBlood(character, lowercaseVBloodName);
    }

    ctx.Reply($"Unlocked VBlood '{lowercaseVBloodName}' for all players.");
  }

  // command to unlock a vblood for the player
  [Command("vblood unlock", "Unlock a vblood for the player", adminOnly: true)]
  public static void UnlockVBloodCommand(ChatCommandContext ctx, string vBloodName, string playerName = null)
  {
    Entity playerEntity;
    Entity userEntity;
    string lowercaseVBloodName = vBloodName.ToLowerInvariant();

    if (string.IsNullOrEmpty(playerName))
    {
      userEntity = ctx.Event.SenderUserEntity;
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

    // we want to make sure the vBloodName is valid
    string[] availableVBloods = ProgressionService.GetAvailableVBloodUnlocks();

    if (!availableVBloods.Contains(lowercaseVBloodName))
    {
      ctx.Reply($"Invalid VBlood name '{lowercaseVBloodName}'. Available options: {string.Join(", ", availableVBloods)}");
      return;
    }

    ProgressionService.UnlockVBlood(playerEntity, lowercaseVBloodName);
    ctx.Reply($"Unlocked VBlood '{lowercaseVBloodName}' for character '{playerName ?? userEntity.GetUser().CharacterName}'.");
  }

  // command to remove a vblood unlocked by the player
  [Command("vblood remove", "Remove a vblood from the player's progression", adminOnly: true)]
  public static void RemoveVBloodCommand(ChatCommandContext ctx, string vBloodName, string playerName = null)
  {
    Entity playerEntity;
    Entity userEntity;
    string lowercaseVBloodName = vBloodName.ToLowerInvariant();

    if (string.IsNullOrEmpty(playerName))
    {
      userEntity = ctx.Event.SenderUserEntity;
    }
    else
    {
      if (!TryFindPlayer(playerName, out playerEntity, out userEntity))
      {
        ctx.Reply($"Player '{playerName}' not found.");
        return;
      }
    }

    // we want to make sure the vBloodName is valid
    string[] availableVBloods = ProgressionService.GetAvailableVBloodRemovals();

    if (!availableVBloods.Contains(lowercaseVBloodName))
    {
      ctx.Reply($"Invalid VBlood name '{lowercaseVBloodName}'. Available options: {string.Join(", ", availableVBloods)}");
      return;
    }

    if (ProgressionService.RemoveVBloodUnlock(userEntity, lowercaseVBloodName))
    {
      ctx.Reply($"Removed VBlood '{lowercaseVBloodName}' from character '{playerName}'.");
    }
    else
    {
      ctx.Reply($"Character '{playerName}' does not have VBlood '{lowercaseVBloodName}'.");
    }
  }

  // command to teleport to the sender's map marker
  [Command("teleport to marker", "Teleport to your map marker", adminOnly: false)]
  public static void TeleportToMarkerCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity userEntity = ctx.Event.SenderUserEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    TeleportService.TeleportToMapMarker(playerEntity, userEntity);
    ctx.Reply($"Teleported to your map marker.");
  }

  // command to teleport to target coordinates
  [Command("teleport to", "Teleport to specified coordinates", adminOnly: true)]
  public static void TeleportCommand(ChatCommandContext ctx, float x, float y, float z)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    TeleportService.TeleportToCoordinate(playerEntity, ctx.Event.SenderUserEntity, new float3(x, y, z));
    ctx.Reply($"Teleported to coordinates: {x}, {y}, {z}");
  }

  // command to teleport to another player
  [Command("teleport to", "Teleport to another player", adminOnly: true)]
  public static void TeleportToPlayerCommand(ChatCommandContext ctx, string playerName)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity userEntity = ctx.Event.SenderUserEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    if (!TryFindPlayer(playerName, out Entity targetPlayerEntity, out Entity targetUserEntity))
    {
      ctx.Reply($"Player '{playerName}' not found.");
      return;
    }

    TeleportService.TeleportToEntity(playerEntity, userEntity, targetPlayerEntity);
    ctx.Reply($"Teleported to player: {playerName}");
  }

  // command to teleport another player to the sender
  [Command("teleport here", "Teleport another player to you", adminOnly: true)]
  public static void TeleportHereCommand(ChatCommandContext ctx, string playerName)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    if (!TryFindPlayer(playerName, out Entity targetPlayerEntity, out Entity targetUserEntity))
    {
      ctx.Reply($"Player '{playerName}' not found.");
      return;
    }

    TeleportService.TeleportToEntity(targetPlayerEntity, targetUserEntity, playerEntity);
    ctx.Reply($"Teleported player: {playerName} to you.");
  }

  // command to print the current coordinates of the sender
  [Command("coords", "Print your current coordinates", adminOnly: false)]
  public static void CoordsCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    float3 position = Core.EntityManager.GetComponentData<Translation>(playerEntity).Value;
    ctx.Reply($"Your current coordinates are: {position.x}, {position.y}, {position.z}");
  }

  // command to repair all buildings around the target
  [Command("repair", "Repair all buildings around the target", adminOnly: true)]
  public static void RepairCommand(ChatCommandContext ctx, int radius = 10)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    if (playerEntity == null)
    {
      ctx.Reply("You are not a valid player. TELL VLUUR IN DISCORD IMMEDIATELY!");
      return;
    }

    BuildingService.RepairAroundTarget(playerEntity, radius);
    ctx.Reply($"Repaired all buildings around you.");
  }

  // Softlock commands
  [Command("softlock add", "Add a boss to a tag's softlock mapping", adminOnly: true)]
  public static void AddBossToTagCommand(ChatCommandContext ctx, string tagName, string bossKey)
  {
    if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(bossKey))
    {
      ctx.Reply("Usage: .sov softlock add <tagName> <bossKey>");
      return;
    }

    if (SoftlockService.AddBossToTag(tagName, bossKey))
    {
      ctx.Reply($"Successfully associated boss '{bossKey}' with tag '{tagName}'.");
    }
    else
    {
      ctx.Reply($"Failed to associate boss '{bossKey}' with tag '{tagName}'. Check that the boss key is valid.");
    }
  }

  [Command("softlock remove", "Remove a boss from a tag's softlock mapping", adminOnly: true)]
  public static void RemoveBossFromTagCommand(ChatCommandContext ctx, string tagName, string bossKey)
  {
    if (string.IsNullOrEmpty(tagName) || string.IsNullOrEmpty(bossKey))
    {
      ctx.Reply("Usage: .sov softlock remove <tagName> <bossKey>");
      return;
    }

    if (SoftlockService.RemoveBossFromTag(tagName, bossKey))
    {
      ctx.Reply($"Successfully removed boss '{bossKey}' from tag '{tagName}'.");
    }
    else
    {
      ctx.Reply($"Failed to remove boss '{bossKey}' from tag '{tagName}'. Check that the association exists.");
    }
  }

  [Command("softlock list", "List all tags and their associated bosses", adminOnly: true)]
  public static void ListSoftlockMappingsCommand(ChatCommandContext ctx)
  {
    // Check if the mapping exists
    if (!ModDataService.ModData.ContainsKey(ModDataService.SOFTLOCK_MAPPING_KEY))
    {
      ctx.Reply("No softlock mappings found.");
      return;
    }

    var mapping = (Dictionary<string, PrefabGUID[]>)ModDataService.ModData[ModDataService.SOFTLOCK_MAPPING_KEY];

    // Filter out empty mappings
    var nonEmptyMappings = mapping.Where(kvp => kvp.Value != null && kvp.Value.Length > 0).ToList();

    if (nonEmptyMappings.Count == 0)
    {
      ctx.Reply("No tags with associated bosses found.");
      return;
    }

    var sb = new StringBuilder();
    sb.AppendLine("Softlock Mappings:");

    foreach (var kvp in nonEmptyMappings)
    {
      string tagName = kvp.Key;
      var bosses = kvp.Value;

      // Find boss names from AvailableBosses dictionary
      var bossNames = new List<string>();
      foreach (var bossGuid in bosses)
      {
        string bossName = SoftlockService.AvailableBosses.FirstOrDefault(x => x.Value.Equals(bossGuid)).Key;
        bossNames.Add(bossName ?? $"Unknown({bossGuid})");
      }

      sb.AppendLine($"  {tagName}: {string.Join(", ", bossNames)}");
    }

    ctx.Reply(sb.ToString());
  }

  [Command("softlock bosses", "List all available bosses for softlocking", adminOnly: true)]
  public static void ListAvailableBossesCommand(ChatCommandContext ctx)
  {
    var sb = new StringBuilder();
    sb.AppendLine("Available Bosses:");

    foreach (var boss in SoftlockService.AvailableBosses)
    {
      sb.AppendLine($"  {boss.Key}");
    }

    ctx.Reply(sb.ToString());
  }
}