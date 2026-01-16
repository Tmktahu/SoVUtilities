namespace SoVUtilities.Models;

public class PlayerData
{
  public ulong SteamId { get; set; }
  public string CharacterName { get; set; }
  public int GuidHash { get; set; }
  public bool HideAdminStatus { get; set; } = false;
  public bool DisableHideNameplate { get; set; } = false;
  public bool ShapeshiftDamageImmunity { get; set; } = false;
  public List<string> Tags { get; set; } = new List<string>();
  public DateTime? LastBloodPotionTime { get; set; }
  public int[] AbilitySlotPrefabGUIDs { get; set; } = new int[8];
  public Dictionary<int, int[]> AbilitySlotDefinitions { get; set; } = new Dictionary<int, int[]>();
  public bool HasTag(string tag) => Tags.Contains(tag);
  public bool AddTag(string tag)
  {
    if (Tags.Contains(tag))
      return false;

    Tags.Add(tag);
    return true;
  }
  public bool RemoveTag(string tag)
  {
    if (!Tags.Contains(tag))
      return false;

    bool removed = Tags.Remove(tag);
    return removed;
  }
}
