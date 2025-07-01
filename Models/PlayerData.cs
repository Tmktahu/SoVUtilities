namespace SoVUtilities.Models;

public class PlayerData
{
  public ulong SteamId { get; set; }
  public string CharacterName { get; set; }
  public List<string> Tags { get; set; } = new List<string>();
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
