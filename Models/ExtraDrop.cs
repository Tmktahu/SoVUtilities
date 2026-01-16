using Stunlock.Core;
using SoVUtilities.Resources;

namespace SoVUtilities.Models;

public class DropRule
{
  public PrefabGUID ItemPrefab { get; set; }
  public int MinAmount { get; set; }
  public int MaxAmount { get; set; }
  public float DropChance { get; set; }
  public string[] DroppedByResources { get; set; }
  public string[] DroppedByMobs { get; set; }
}

public static class ExtraDropConfig
{
  public static readonly List<DropRule> DropRules = new()
  {
    new DropRule
    {
      ItemPrefab = PrefabGUIDs.Item_Ingredient_Plant_RadiantFiber,
      MinAmount = 1,
      MaxAmount = 2,
      DropChance = 1f, // 100% chance
      DroppedByResources = new[] { "plant", "plantfiber" },
      DroppedByMobs = null
    },
    new DropRule
    {
      ItemPrefab = PrefabGUIDs.Item_Ingredient_Mineral_GoldOre,
      MinAmount = 1,
      MaxAmount = 1,
      DropChance = 0.5f, // 50% chance
      DroppedByResources = null,
      DroppedByMobs = new[] { "vampire" }
    }
  };
}
