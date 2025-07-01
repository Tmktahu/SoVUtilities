// using ProjectM.Network;
// using ProjectM;
// using Unity.Entities;
// using HarmonyLib;
// using Unity.Collections;
// using System.Text;

// namespace SoVUtilities.Patches;

// [HarmonyPriority(200)]
// [HarmonyBefore("gg.deca.Bloodstone")]
// [HarmonyAfter("gg.deca.VampireCommandFramework")]
// [HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
// public static class ChatMessageSystemPatch
// {
// 	public static void Prefix(ChatMessageSystem __instance)
// 	{
// 		if (__instance.__query_661171423_0 != null)
// 		{
// 			NativeArray<Entity> entities = __instance.__query_661171423_0.ToEntityArray(Allocator.Temp);
// 			foreach (var entity in entities)
// 			{
// 				var fromData = __instance.EntityManager.GetComponentData<FromCharacter>(entity);
// 				var userData = __instance.EntityManager.GetComponentData<User>(fromData.User);
// 				var chatEventData = __instance.EntityManager.GetComponentData<ChatMessageEvent>(entity);

// 				var messageText = chatEventData.MessageText.ToString();

// 				Core.Log.LogInfo($"[ChatMessageSystemPatch] User: {userData.CharacterName} | Message: {messageText}");

// 				// we want to try stopping the message completely for debugging
// 			}
// 		}
// 	}
// }