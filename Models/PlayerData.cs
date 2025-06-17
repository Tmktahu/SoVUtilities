namespace SoVUtilities.Models;

public class PlayerData
{
  public ulong SteamId { get; set; }
  public string PlayerName { get; set; }
  public List<string> Tags { get; set; } = new List<string>();
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
  public bool HasTag(string tag) => Tags.Contains(tag);
  public bool AddTag(string tag)
  {
    if (Tags.Contains(tag))
      return false;

    Tags.Add(tag);
    LastUpdated = DateTime.UtcNow;
    return true;
  }
  public bool RemoveTag(string tag)
  {
    bool removed = Tags.Remove(tag);
    if (removed)
      LastUpdated = DateTime.UtcNow;
    return removed;
  }
}
