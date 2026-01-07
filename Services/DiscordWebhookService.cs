using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoVUtilities.Services
{
  public static class DiscordWebhookService
  {
    private static readonly HttpClient _httpClient = new HttpClient();
    // dev webhook URL
    // private const string WebhookUrl = "https://discord.com/api/webhooks/1451815135793516646/O5UHUdYqJhMPtXUhFioc5WJaF-o3F2VJxVbaCBHHprbSPxO0fzU3j7DE4vvjWG0LZJr7";

    // prod webhook URL
    private const string WebhookUrl = "https://discord.com/api/webhooks/1458262009488343041/o72EIbTmUF9v1BkIpxmBeDQIW3lbuaNNfqyUoK7vMTvxBleMU8RkCoewM0L6A1jv5C4Q";

    // Minimal async method for POC
    public static async Task SendTestMessageAsync()
    {
      try
      {
        var payload = new { content = "Test message from VRising mod." };
        string json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(WebhookUrl, content);

        if (!response.IsSuccessStatusCode)
        {
          string errorContent = await response.Content.ReadAsStringAsync();
          Console.WriteLine($"Discord webhook failed: {response.StatusCode} - {errorContent}");
        }
        else
        {
          Console.WriteLine("Discord webhook sent successfully.");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Exception sending Discord webhook: {ex.Message}");
      }
    }


    public static async Task SendMessageToWebhook(string message)
    {
      try
      {
        var payload = new { content = message };
        string json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(WebhookUrl, content);

        if (!response.IsSuccessStatusCode)
        {
          string errorContent = await response.Content.ReadAsStringAsync();
          Console.WriteLine($"Discord webhook failed: {response.StatusCode} - {errorContent}");
        }
        else
        {
          Console.WriteLine("Discord webhook sent successfully.");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Exception sending Discord webhook: {ex.Message}");
      }
    }
  }
}
