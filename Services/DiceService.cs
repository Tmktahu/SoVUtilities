using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace SoVUtilities.Services;

public class DiceResult
{
  public bool Valid { get; set; } = false;
  public int Result { get; set; }
  public string ResultText { get; set; }
  public int NumberOfDice { get; set; }
}

public static class DiceService
{
  private const int MaxDiceCount = 100;
  private const int MaxSides = 100;
  private const int DetailDisplayThreshold = 5;
  public static DiceResult RollDice(string diceExpression)
  {
    // Core.Log.LogInfo($"RollDice input: '{diceExpression}'");
    var regex = new System.Text.RegularExpressions.Regex(@"^\s*(\d+)d(\d+)([+-]\d+)?\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    var match = regex.Match(diceExpression);
    if (!match.Success || int.Parse(match.Groups[1].Value) <= 0 || int.Parse(match.Groups[2].Value) <= 0)
    {
      // Core.Log.LogError($"RollDice: Invalid dice expression '{diceExpression}'");
      return new DiceResult
      {
        Valid = false,
        Result = 0,
        ResultText = "Invalid dice expression"
      };
    }

    int numberOfDice = int.Parse(match.Groups[1].Value);
    int sides = int.Parse(match.Groups[2].Value);
    
    if (numberOfDice > MaxDiceCount)
    {
      return new DiceResult
      {
        Valid = false,
        Result = 0,
        ResultText = "Maximum dice count is 100"
      };
    }
    
    if (sides > MaxSides)
    {
      return new DiceResult
      {
        Valid = false,
        Result = 0,
        ResultText = "Maximum dice sides is 100"
      };
    }
    
    int modifier = 0;
    if (match.Groups[3].Success)
    {
      modifier = int.Parse(match.Groups[3].Value);
    }
    // Core.Log.LogInfo($"RollDice parsed: numberOfDice={numberOfDice}, sides={sides}, modifier={modifier}");

    var random = new Random();
    int total = 0;
    StringBuilder resultText = new StringBuilder();

    for (int i = 0; i < numberOfDice; i++)
    {
      int roll = random.Next(1, sides + 1);
      total += roll;
      resultText.Append(roll);
      if (i < numberOfDice - 1) resultText.Append(" + ");
    }

    total += modifier;
    if (modifier != 0)
    {
      resultText.Append(modifier > 0 ? $" + {modifier}" : $" - {Math.Abs(modifier)}");
    }

    resultText.Append($" = {total}");

    return new DiceResult
    {
      Valid = true,
      Result = total,
      ResultText = resultText.ToString(),
      NumberOfDice = numberOfDice
    };
  }

  public static bool CheckSyntax(string diceExpression)
  {
    try
    {
      // Core.Log.LogInfo($"CheckSyntax input: '{diceExpression}'");
      var regex = new System.Text.RegularExpressions.Regex(@"^\s*(\d+)d(\d+)([+-]\d+)?\s*$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      var match = regex.Match(diceExpression);
      // Core.Log.LogInfo($"Regex match success: {match.Success}");
      return match.Success && int.Parse(match.Groups[1].Value) > 0 && int.Parse(match.Groups[2].Value) > 0;
    }
    catch (Exception ex)
    {
      Core.Log.LogError($"CheckSyntax error: {ex.Message}");
      return false;
    }
  }

  public static void SendToNearbyPlayers(Entity playerEntity, DiceResult diceResult)
  {
    EntityManager entityManager = Core.EntityManager;
    var nearbyUserEntities = EntityService.GetNearbyUserEntities(playerEntity, 45f); // local range
    if (nearbyUserEntities.Count == 0)
    {
      // Core.Log.LogInfo($"ChatMessageSystem: No nearby players found for entity {playerEntity}");
      return;
    }

    foreach (var nearbyUserEntity in nearbyUserEntities)
    {
      if (!entityManager.HasComponent<User>(nearbyUserEntity)) continue;

      var targetUserData = entityManager.GetComponentData<User>(nearbyUserEntity);

      ServerBootstrapSystem serverBootstrapSystem = Core.ServerBootstrapSystem;
      if (!serverBootstrapSystem._UserIndexToApprovedUserIndex.ContainsKey(targetUserData.Index)) continue;

      int userApprovedIndex = serverBootstrapSystem._UserIndexToApprovedUserIndex[targetUserData.Index];

      User user = playerEntity.GetUser();
      string playerName = user.CharacterName.ToString();
      // target message format is
      // NAME rolled DICE_NOTATION and got: RESULT (RESULT_TEXT)

      var message = diceResult.NumberOfDice > DetailDisplayThreshold 
        ? new FixedString512Bytes($"*{playerName} rolled and got: {diceResult.Result}*")
        : new FixedString512Bytes($"*{playerName} rolled and got: {diceResult.Result} ({diceResult.ResultText})*");
      var toConnectedUserIndex = userApprovedIndex;
      NetworkId fromNetworkId = playerEntity.GetNetworkId();
      long timestamp = (long)Core.ServerGameManager.ServerTime;

      ServerChatUtils.SendChatMessage(entityManager, ref toConnectedUserIndex, ref message, ref fromNetworkId, ref fromNetworkId, ServerChatMessageType.Local, timestamp);
    }
  }

  public static void SendToDiscord(Entity playerEntity, string diceNotation, DiceResult diceResult)
  {
    // we can get the player name off the user component
    User user = playerEntity.GetUser();
    string playerName = user.CharacterName.ToString();

    // we pretty much prepare the message to be sent to discord and then call the discord service for sending
    // we need to include who rolled, what they rolled, and what the result was
    string message = diceResult.NumberOfDice > DetailDisplayThreshold
      ? $"**{playerName}** rolled **{diceNotation}** and got: **{diceResult.Result}**"
      : $"**{playerName}** rolled **{diceNotation}** and got: **{diceResult.Result}** (`{diceResult.ResultText}`)";
    DiscordWebhookService.SendMessageToWebhook(message);
  }
}

