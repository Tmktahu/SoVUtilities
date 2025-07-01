using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Stunlock.Network;
using System.Collections;
using Unity.Entities;
using UnityEngine;
using User = ProjectM.Network.User;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class ServerBootstrapSystemPatches
{
    static EntityManager EntityManager => Core.EntityManager;
    static readonly WaitForSeconds _delay = new(1f);

    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    [HarmonyPostfix]
    static void OnUserConnectedPostfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
    {
        if (!__instance._NetEndPointToApprovedUserIndex.TryGetValue(netConnectionId, out int userIndex)) return;
        ServerBootstrapSystem.ServerClient serverClient = __instance._ApprovedUsersLookup[userIndex];

        Entity userEntity = serverClient.UserEntity;
        User user = __instance.EntityManager.GetComponentData<User>(userEntity);

        Entity playerCharacter = user.LocalCharacter.GetEntityOnServer();
        bool exists = playerCharacter.Exists();

        // Core.Log.LogInfo($"[ServerBootstrapSystem.OnUserConnectedPostfix] - PlayerCharacter: {playerCharacter} - Exists: {exists}");
        UpdatePlayerData(playerCharacter, exists).Start();
    }
    static IEnumerator UpdatePlayerData(Entity playerCharacter, bool exists)
    {
        yield return _delay;

        if (exists)
        {
            // Core.Log.LogInfo($"[ServerBootstrapSystem.OnUserConnectedPostfix] - Trying to refresh player buffs for {playerCharacter}");
            BuffService.RefreshPlayerBuffs(playerCharacter).Start();
        }
    }
}
