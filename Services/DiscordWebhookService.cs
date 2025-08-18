// using System.Text;
// using System.Text.Json;

// namespace SoVUtilities.Services;

// public static class DiscordWebhookService
// {
//   private static readonly HttpClient _httpClient = new HttpClient();

//   public static async Task SendMessageAsync(string webhookUrl, string message)
//   {
//     try
//     {
//       var payload = new { content = message };
//       string json = JsonSerializer.Serialize(payload);
//       var content = new StringContent(json, Encoding.UTF8, "application/json");

//       var response = await _httpClient.PostAsync(webhookUrl, content);

//       if (!response.IsSuccessStatusCode)
//       {
//         string errorContent = await response.Content.ReadAsStringAsync();
//         Core.Log.LogError($"Discord webhook failed with status {response.StatusCode}: {errorContent}");
//       }
//     }
//     catch (Exception ex)
//     {
//       Core.Log.LogError($"Failed to send Discord webhook message: {ex.Message}");
//     }
//   }

//   // Fire-and-forget method for synchronous contexts
//   public static void SendMessage(string webhookUrl, string message)
//   {
//     _ = Task.Run(async () => await SendMessageAsync(webhookUrl, message));
//   }
// }
