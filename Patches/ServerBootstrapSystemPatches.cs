using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using Stunlock.Network;
using System.Collections;
using Unity.Entities;
using UnityEngine;
using User = ProjectM.Network.User;
using SoVUtilities.Resources;

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

        UpdatePlayerData(playerCharacter, exists).Start();
    }
    static IEnumerator UpdatePlayerData(Entity playerCharacter, bool exists)
    {
        yield return _delay;

        if (exists)
        {
            BuffService.RefreshPlayerBuffs(playerCharacter).Start();
            TeamService.ResetTeam(playerCharacter);

            // if they are wearing the razor hood, we want to apply the hide nameplate buff
            if (EntityManager.HasComponent<Equipment>(playerCharacter))
            {
                Equipment equipment = EntityManager.GetComponentData<Equipment>(playerCharacter);
                if (equipment.IsEquipped(PrefabGUIDs.Item_Headgear_RazerHood, out _))
                {
                    BuffService.AddHideNameplateBuff(playerCharacter);
                }
            }
        }
    }
}
