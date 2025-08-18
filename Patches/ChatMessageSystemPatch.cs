using ProjectM.Network;
using ProjectM;
using Unity.Entities;
using HarmonyLib;
using Unity.Collections;
using SoVUtilities.Services;

namespace SoVUtilities.Patches;

[HarmonyPriority(1000)]
[HarmonyBefore("gg.deca.Bloodstone", "gg.deca.VampireCommandFramework")]
[HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
public static class ChatMessageSystemPatch
{
  public static void Prefix(ChatMessageSystem __instance)
  {
    if (__instance.__query_661171423_0 != null)
    {
      NativeArray<Entity> entities = __instance.__query_661171423_0.ToEntityArray(Allocator.Temp);
      foreach (var entity in entities)
      {
        var fromData = __instance.EntityManager.GetComponentData<FromCharacter>(entity);
        var chatEventData = __instance.EntityManager.GetComponentData<ChatMessageEvent>(entity);
        var messageText = chatEventData.MessageText.ToString();

        if (!messageText.StartsWith(".anon")) continue;

        // Remove the ".anon" prefix and the space after it
        if (messageText.Length > 5)
        {
          messageText = messageText.Substring(5).TrimStart();
        }
        else
        {
          messageText = string.Empty;
        }

        EntityManager entityManager = Core.EntityManager;
        Entity senderEntity = fromData.Character;
        var nearbyUserEntities = EntityService.GetNearbyUserEntities(senderEntity, 45f); // local range
        if (nearbyUserEntities.Count == 0)
        {
          Core.Log.LogInfo($"ChatMessageSystem: No nearby players found for entity {entity}");
          return;
        }

        foreach (var nearbyUserEntity in nearbyUserEntities)
        {
          if (!entityManager.HasComponent<User>(nearbyUserEntity)) continue;

          var targetUserData = entityManager.GetComponentData<User>(nearbyUserEntity);

          ServerBootstrapSystem serverBootstrapSystem = Core.ServerBootstrapSystem;
          if (!serverBootstrapSystem._UserIndexToApprovedUserIndex.ContainsKey(targetUserData.Index)) continue;

          int userApprovedIndex = serverBootstrapSystem._UserIndexToApprovedUserIndex[targetUserData.Index];
          var message = new FixedString512Bytes(messageText);
          var toConnectedUserIndex = userApprovedIndex;
          NetworkId fromNetworkId = senderEntity.GetNetworkId();
          long timestamp = (long)Core.ServerGameManager.ServerTime;

          ServerChatUtils.SendChatMessage(entityManager, ref toConnectedUserIndex, ref message, ref fromNetworkId, ref fromNetworkId, ServerChatMessageType.Local, timestamp);
        }

        // Remove the entity after processing
        if (entityManager.Exists(entity))
        {
          entityManager.DestroyEntity(entity);
        }
      }
    }
  }
}