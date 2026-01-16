using SoVUtilities.Services;
using System.Text;
using VampireCommandFramework;
using static SoVUtilities.Services.EntityService;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Stunlock.Core;
using ProjectM;

namespace SoVUtilities.Commands;

[CommandGroup("sov")]
internal static class SovCommands
{
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
    BuffService.RefreshPlayerBuffs(playerEntity).Start();
    ctx.Reply("Your buffs have been refreshed.");
  }

  // we want a public command for players to use that reveals their nameplate
  [Command("reveal", "Reveal your nameplate", adminOnly: false)]
  public static void RevealNameplateCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    BuffService.RemoveHideNameplateBuff(playerEntity, true);
    ctx.Reply("Your nameplate has been revealed.");
  }

  // command for players to request a blood potion once per day
  [Command("drawblood", "Request your daily blood potion", adminOnly: false)]
  public static void BloodPotionCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;

    // Check if player has human tag first
    var playerData = PlayerDataService.GetPlayerData(playerEntity);
    if (!playerData.HasTag(PlayerDataService.HUMAN_TAG))
    {
      ctx.Reply("You are not a human. Doofus.");
      return;
    }

    bool success = ItemService.GiveHumanBloodPotion(playerEntity, out TimeSpan? timeRemaining);
    if (success)
    {
      // if it was a success, then they got their blood potion
      ctx.Reply("You have drawn your blood into a bottle.");
    }
    else if (timeRemaining.HasValue && timeRemaining.Value > TimeSpan.Zero)
    {
      // otherwise it was a failure, but we have a time remaining that is greater than zero
      // Format the time remaining nicely
      var hours = (int)timeRemaining.Value.TotalHours;
      var minutes = timeRemaining.Value.Minutes;
      var seconds = timeRemaining.Value.Seconds;
      ctx.Reply($"You must wait {hours}:{minutes}:{seconds} before drawing blood again.");
    }
    else
    {
      // otherwise they COULD get a potion but something went wrong, check inventory?
      ctx.Reply("Failed to draw blood. Is your inventory full?");
    }
  }

  // command to list nearby players with hide nameplate buff
  [Command("who", "List nearby players with hide nameplate buff", adminOnly: true)]
  public static void ListNearbyPlayersWithHideNameplateCommand(ChatCommandContext ctx, float radius = 10f)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
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
    TeleportService.TeleportToMapMarker(playerEntity, userEntity);
    ctx.Reply($"Teleported to your map marker.");
  }

  // command to teleport to target coordinates
  [Command("teleport to", "Teleport to specified coordinates", adminOnly: true)]
  public static void TeleportCommand(ChatCommandContext ctx, float x, float y, float z)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    TeleportService.TeleportToCoordinate(playerEntity, ctx.Event.SenderUserEntity, new float3(x, y, z));
    ctx.Reply($"Teleported to coordinates: {x}, {y}, {z}");
  }

  // command to teleport to another player
  [Command("teleport to", "Teleport to another player", adminOnly: true)]
  public static void TeleportToPlayerCommand(ChatCommandContext ctx, string playerName)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Entity userEntity = ctx.Event.SenderUserEntity;
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
    float3 position = Core.EntityManager.GetComponentData<Translation>(playerEntity).Value;
    ctx.Reply($"Your current coordinates are: {position.x}, {position.y}, {position.z}");
  }

  // command to repair all buildings around the target
  [Command("repair", "Repair all buildings around the target", adminOnly: true)]
  public static void RepairCommand(ChatCommandContext ctx, int radius = 10)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
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

  // a command to set the hide admin status flag for a player
  [Command("hideadmin", "Toggle hiding your admin status in chat", adminOnly: true)]
  public static void ToggleHideAdminStatusCommand(ChatCommandContext ctx, string hideStatus = null)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    var playerData = PlayerDataService.GetPlayerData(playerEntity);

    bool hideStatusBool;
    if (string.Equals(hideStatus, "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(hideStatus, "yes", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(hideStatus, "1", StringComparison.OrdinalIgnoreCase))
    {
      hideStatusBool = true;
    }
    else if (string.Equals(hideStatus, "false", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(hideStatus, "no", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(hideStatus, "0", StringComparison.OrdinalIgnoreCase))
    {
      hideStatusBool = false;
    }
    else if (string.IsNullOrEmpty(hideStatus))
    {
      // toggle the current status
      hideStatusBool = !playerData.HideAdminStatus;
    }
    else
    {
      ctx.Reply("Invalid argument. Use 'true', 'false', or leave empty to toggle.");
      return;
    }

    playerData.HideAdminStatus = hideStatusBool;
    PlayerDataService.MarkDirty();

    ctx.Reply($"Your admin status will now be {(hideStatusBool ? "hidden" : "visible")} in chat.");
  }

  // a command to set the always reveal nameplate flag
  [Command("alwaysreveal", "Toggle always revealing your nameplate (bypasses Razer Hood)", adminOnly: false)]
  public static void ToggleAlwaysRevealCommand(ChatCommandContext ctx, string revealStatus = null)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    var playerData = PlayerDataService.GetPlayerData(playerEntity);

    bool revealStatusBool;
    if (string.Equals(revealStatus, "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(revealStatus, "yes", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(revealStatus, "1", StringComparison.OrdinalIgnoreCase))
    {
      revealStatusBool = true;
    }
    else if (string.Equals(revealStatus, "false", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(revealStatus, "no", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(revealStatus, "0", StringComparison.OrdinalIgnoreCase))
    {
      revealStatusBool = false;
    }
    else if (string.IsNullOrEmpty(revealStatus))
    {
      revealStatusBool = !playerData.DisableHideNameplate;
    }
    else
    {
      ctx.Reply("Invalid argument. Use 'true', 'false', or leave empty to toggle.");
      return;
    }

    playerData.DisableHideNameplate = revealStatusBool;
    PlayerDataService.MarkDirty();

    ctx.Reply($"Your nameplate will now {(revealStatusBool ? "always be visible" : "follow normal hiding rules")}.");
  }

  // a command to toggle shapeshift damage immunity
  [Command("shapeshift immunity", "Toggle shapeshift damage immunity (maintain shapeshift form when taking damage)", adminOnly: true)]
  public static void ToggleShapeshiftImmunityCommand(ChatCommandContext ctx, string immunityStatus = null)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    var playerData = PlayerDataService.GetPlayerData(playerEntity);

    bool immunityStatusBool;
    if (string.Equals(immunityStatus, "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(immunityStatus, "yes", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(immunityStatus, "1", StringComparison.OrdinalIgnoreCase))
    {
      immunityStatusBool = true;
    }
    else if (string.Equals(immunityStatus, "false", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(immunityStatus, "no", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(immunityStatus, "0", StringComparison.OrdinalIgnoreCase))
    {
      immunityStatusBool = false;
    }
    else if (string.IsNullOrEmpty(immunityStatus))
    {
      immunityStatusBool = !playerData.ShapeshiftDamageImmunity;
    }
    else
    {
      ctx.Reply("Invalid argument. Use 'true', 'false', or leave empty to toggle.");
      return;
    }

    playerData.ShapeshiftDamageImmunity = immunityStatusBool;
    PlayerDataService.MarkDirty();

    ctx.Reply($"You are now {(immunityStatusBool ? "immune" : "vulnerable")} to being knocked out of shapeshift forms when taking damage.");
  }

  // command to spawn a sequence from sequence GUID onto the player
  [Command("spawn sequence", "Spawn a sequence onto the player", adminOnly: true)]
  public static void SpawnSequenceCommand(ChatCommandContext ctx, int sequenceGuid, int lifetime)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Core.SequenceService.Spawn(playerEntity, new SequenceGUID(sequenceGuid), lifetime);
    ctx.Reply($"Spawned sequence {sequenceGuid} onto you.");
  }

  // command to list all sequences on the player
  [Command("list sequences", "List all sequences on the player", adminOnly: true)]
  public static void ListSequencesCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Core.SequenceService.ListActiveSequences(playerEntity);
    ctx.Reply($"Listed active sequences for you in the server log.");
  }

  // command to despawn a sequence from sequence GUID from the player
  [Command("despawn sequence", "Despawn a sequence from the player", adminOnly: true)]
  public static void DespawnSequenceCommand(ChatCommandContext ctx, int sequenceGuid)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    Core.SequenceService.Despawn(playerEntity, new SequenceGUID(sequenceGuid));
    ctx.Reply($"Despawned sequence {sequenceGuid} from you.");
  }

  // command to test item stuff
  [Command("item test", "Test item command", adminOnly: true)]
  public static void ItemTestCommand(ChatCommandContext ctx)
  {
    Entity playerEntity = ctx.Event.SenderCharacterEntity;
    ItemService.TestItemFunctionality(playerEntity);
    ctx.Reply("Tested item functionality. Check server log for details.");
  }

  // command to make a dice roll
  [Command("roll", "Roll a dice in NdM format (e.g., 2d6)", adminOnly: false)]
  public static void RollDiceCommand(ChatCommandContext ctx, string diceNotation)
  {
    try
    {
      DiceResult diceResult = DiceService.RollDice(diceNotation);
      if (diceResult.Valid)
      {
        int result = diceResult.Result;
        string details = diceResult.ResultText;
        // ctx.Reply($"You rolled {diceNotation}: {details}"); we handle display in SendToNearbyPlayers

        DiceService.SendToNearbyPlayers(ctx.Event.SenderCharacterEntity, diceResult);
        DiceService.SendToDiscord(ctx.Event.SenderCharacterEntity, diceNotation, diceResult);
      }
      else
      {
        ctx.Reply(diceResult.ResultText);
      }
    }
    catch (Exception ex)
    {
      ctx.Reply($"Error rolling dice: {ex.Message}");
    }
  }
}