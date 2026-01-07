using SoVUtilities.Services;
using System.Text;
using VampireCommandFramework;
using static SoVUtilities.Services.EntityService;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Stunlock.Core;
using ProjectM;
using SoVUtilities.Services.Buffs;
using SoVUtilities.Resources;
using ProjectM.Terrain;

namespace SoVUtilities.Commands;

[CommandGroup("sov")]
internal static class RegionCommands
{
  // command to add a buff to a region
  [Command("region buffs add", "rba", "Add a buff to a region", adminOnly: true)]
  public static void SetSpellsCommand(ChatCommandContext ctx, string regionTag, int buffPrefabGUID)
  {
    string lowercaseTag = regionTag.ToLowerInvariant();
    var matchedTags = TagService.MatchRegionTag(lowercaseTag);
    if (matchedTags.Length == 0)
    {
      ctx.Reply($"Invalid region tag: {regionTag}. Valid tags are: {string.Join(", ", RegionService.RegionTypeToTag.Values)}");
      return;
    }

    if (matchedTags.Length > 1)
    {
      ctx.Reply($"Ambiguous region tag '{regionTag}'. Did you mean: {string.Join(", ", matchedTags)}?");
      return;
    }

    WorldRegionType region = RegionService.TagToRegionType[matchedTags[0]];
    PrefabGUID buffGuid = new PrefabGUID(buffPrefabGUID);
    Core.RegionService.AddBuffToRegion(region, buffGuid);
    ctx.Reply($"Added buff {buffPrefabGUID} to region {region}.");
  }

  // command to remove a buff from a region
  [Command("region buffs remove", "rbr", "Remove a buff from a region", adminOnly: true)]
  public static void RemoveBuffFromRegionCommand(ChatCommandContext ctx, string regionTag, int buffPrefabGUID)
  {
    string lowercaseTag = regionTag.ToLowerInvariant();
    var matchedTags = TagService.MatchRegionTag(lowercaseTag);
    if (matchedTags.Length == 0)
    {
      ctx.Reply($"Invalid region tag: {regionTag}. Valid tags are: {string.Join(", ", RegionService.RegionTypeToTag.Values)}");
      return;
    }

    if (matchedTags.Length > 1)
    {
      ctx.Reply($"Ambiguous region tag '{regionTag}'. Did you mean: {string.Join(", ", matchedTags)}?");
      return;
    }

    WorldRegionType region = RegionService.TagToRegionType[matchedTags[0]];
    PrefabGUID buffGuid = new PrefabGUID(buffPrefabGUID);
    Core.RegionService.RemoveBuffFromRegion(region, buffGuid);
    ctx.Reply($"Removed buff {buffPrefabGUID} from region {region}.");
  }

  // command to enable region buffs
  [Command("region buffs enable", "rbe", "Enable region buffs", adminOnly: true)]
  public static void EnableRegionBuffsCommand(ChatCommandContext ctx, string regionTag)
  {
    string lowercaseTag = regionTag.ToLowerInvariant();
    var matchedTags = TagService.MatchRegionTag(lowercaseTag);
    if (matchedTags.Length == 0)
    {
      ctx.Reply($"Invalid region tag: {regionTag}. Valid tags are: {string.Join(", ", RegionService.RegionTypeToTag.Values)}");
      return;
    }

    if (matchedTags.Length > 1)
    {
      ctx.Reply($"Ambiguous region tag '{regionTag}'. Did you mean: {string.Join(", ", matchedTags)}?");
      return;
    }

    WorldRegionType region = RegionService.TagToRegionType[matchedTags[0]];
    Core.RegionService.EnableRegionBuffs(region);
    ctx.Reply($"Enabled buffs for region {region}.");
  }

  // command to disable region buffs
  [Command("region buffs disable", "rbd", "Disable region buffs", adminOnly: true)]
  public static void DisableRegionBuffsCommand(ChatCommandContext ctx, string regionTag)
  {
    string lowercaseTag = regionTag.ToLowerInvariant();
    var matchedTags = TagService.MatchRegionTag(lowercaseTag);
    if (matchedTags.Length == 0)
    {
      ctx.Reply($"Invalid region tag: {regionTag}. Valid tags are: {string.Join(", ", RegionService.RegionTypeToTag.Values)}");
      return;
    }

    if (matchedTags.Length > 1)
    {
      ctx.Reply($"Ambiguous region tag '{regionTag}'. Did you mean: {string.Join(", ", matchedTags)}?");
      return;
    }

    WorldRegionType region = RegionService.TagToRegionType[matchedTags[0]];
    Core.RegionService.DisableRegionBuffs(region);
    ctx.Reply($"Disabled buffs for region {region}.");
  }

  // command to list buffs in a region
  [Command("region buffs list", "rbl", "List buffs in a region", adminOnly: true)]
  public static void ListRegionBuffsCommand(ChatCommandContext ctx, string regionTag)
  {
    string lowercaseTag = regionTag.ToLowerInvariant();
    var matchedTags = TagService.MatchRegionTag(lowercaseTag);
    if (matchedTags.Length == 0)
    {
      ctx.Reply($"Invalid region tag: {regionTag}. Valid tags are: {string.Join(", ", RegionService.RegionTypeToTag.Values)}");
      return;
    }

    if (matchedTags.Length > 1)
    {
      ctx.Reply($"Ambiguous region tag '{regionTag}'. Did you mean: {string.Join(", ", matchedTags)}?");
      return;
    }

    WorldRegionType region = RegionService.TagToRegionType[matchedTags[0]];
    var regionBuffs = ModDataService.GetRegionBuffMapping();
    if (regionBuffs.TryGetValue((int)region, out RegionBuffConfig buffConfig))
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine($"Buffs in region {region}:");
      foreach (var buffId in buffConfig.BuffIds)
      {
        sb.AppendLine($"- {PrefabGUIDsExtensions.GetPrefabGUIDName(new PrefabGUID(buffId))} ({buffId})");
      }
      ctx.Reply(sb.ToString());
    }
    else
    {
      ctx.Reply($"No buffs configured for region {region}.");
    }
  }

  // command to clear all buffs from a region
  [Command("region buffs clear", "rbc", "Clear all buffs from a region", adminOnly: true)]
  public static void ClearRegionBuffsCommand(ChatCommandContext ctx, string regionTag)
  {
    string lowercaseTag = regionTag.ToLowerInvariant();
    var matchedTags = TagService.MatchRegionTag(lowercaseTag);
    if (matchedTags.Length == 0)
    {
      ctx.Reply($"Invalid region tag: {regionTag}. Valid tags are: {string.Join(", ", RegionService.RegionTypeToTag.Values)}");
      return;
    }

    if (matchedTags.Length > 1)
    {
      ctx.Reply($"Ambiguous region tag '{regionTag}'. Did you mean: {string.Join(", ", matchedTags)}?");
      return;
    }

    WorldRegionType region = RegionService.TagToRegionType[matchedTags[0]];
    Core.RegionService.ClearBuffsFromRegion(region);
    ctx.Reply($"Cleared all buffs from region {region}.");
  }
}