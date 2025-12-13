using ProjectM.Network;
using ProjectM;
using Unity.Entities;
using HarmonyLib;
using Unity.Collections;
using SoVUtilities.Services;
using Stunlock.Core;
using System.Collections;
using SoVUtilities.Resources;
using UnityEngine;


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
      foreach (var chatEventEntity in entities)
      {
        var fromData = __instance.EntityManager.GetComponentData<FromCharacter>(chatEventEntity);
        var chatEventData = __instance.EntityManager.GetComponentData<ChatMessageEvent>(chatEventEntity);
        var messageText = chatEventData.MessageText.ToString();

        // filter out backtick messages that are normally sent by mistake
        if (messageText == "`")
        {
          // send the sending player a funny message
          var fromUserEntity = fromData.User;
          User user = __instance.EntityManager.GetComponentData<User>(fromUserEntity);
          FixedString512Bytes message = new FixedString512Bytes("Lol doofus what a nerd. Don't worry, no one else saw it.");
          ServerChatUtils.SendSystemMessageToClient(__instance.EntityManager, user, ref message);

          // we just nuke these. filter them out
          if (__instance.EntityManager.Exists(chatEventEntity))
          {
            __instance.EntityManager.DestroyEntity(chatEventEntity);
          }

          continue;
        }

        if (messageText.StartsWith(".anon"))
        {
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
            Core.Log.LogInfo($"ChatMessageSystem: No nearby players found for entity {chatEventEntity}");
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
          if (entityManager.Exists(chatEventEntity))
          {
            entityManager.DestroyEntity(chatEventEntity);
          }

          continue;
        }

        if (chatEventData.MessageType == ChatMessageType.Local)
        {
          EntityManager entityManager = Core.EntityManager;
          Entity playerEntity = fromData.Character;
          Entity senderUserEntity = fromData.User;
          User user = entityManager.GetComponentData<User>(senderUserEntity);

          if (user.IsAdmin)
          {
            var playerData = PlayerDataService.GetPlayerData(playerEntity);
            if (playerData.HideAdminStatus)
            {
              Core.StartCoroutine(HideAdminStatus(user, senderUserEntity, fromData, messageText, chatEventEntity));
            }
          }

          continue;
        }
      }
    }
  }

  public static IEnumerator HideAdminStatus(User user, Entity senderUserEntity, FromCharacter fromData, string messageText, Entity chatEventEntity)
  {
    EntityManager entityManager = Core.EntityManager;

    user.IsAdmin = false; // temporarily set to false to avoid admin tag in local chat
    entityManager.SetComponentData(senderUserEntity, user);
    AdminAuthUtility.SendUserToConnectedUsers(entityManager, ref user, senderUserEntity);

    yield return new WaitForSeconds(0.3f);

    user.IsAdmin = true; // set it back to true
    entityManager.SetComponentData(senderUserEntity, user);
    AdminAuthUtility.SendUserToConnectedUsers(entityManager, ref user, senderUserEntity);

    yield return null;
  }
}