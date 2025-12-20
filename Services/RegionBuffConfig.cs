using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SoVUtilities.Services
{
  public class RegionBuffConfig
  {
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("buffIds")]
    public List<int> BuffIds { get; set; } = new List<int>();

    // Placeholder for future extensibility
    // [JsonPropertyName("otherField")]
    // public string OtherField { get; set; }
  }
}