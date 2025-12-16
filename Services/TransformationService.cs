// using ProjectM;
// using Unity.Entities;
// using ProjectM.Network;
// using Unity.Transforms;
// using Unity.Mathematics;
// using SoVUtilities.Services.Buffs;
// using UnityEngine;
// using System.Collections;

// namespace SoVUtilities.Services;

// public static class TransformationService
// {
//   public static EntityManager EntityManager => Core.EntityManager;

//   // werewolves have three states
//   // 1. Human form: sun immunity, 0 silver resistance, no blood drain
//   // 2. Wolf form: sun immunity, 0 silver resistance, wolf abilities, speed buff
//   // 3. Werewolf form: sun immunity, 0 silver resistance, werewolf abilities, speed buff, health buff, damage buff

//   // function to transform into werewolf form
//   public static IEnumerator TransformToWerewolf(Entity playerEntity)
//   {
//     if (playerEntity == Entity.Null) yield break;

//     // first we clear any existing werewolf buffs
//     // ClearWerewolfBuffs(playerEntity);

//     // if (WerewolfHumanBuff.HasBuff(playerEntity))
//     //   WerewolfHumanBuff.RemoveBuff(playerEntity);

//     // if (WerewolfWolfBuff.HasBuff(playerEntity))
//     //   WerewolfWolfBuff.RemoveBuff(playerEntity);

//     // Wait 1 second for all cleanups to complete
//     yield return new WaitForSeconds(1f);

//     // then we apply the werewolf buff
//     WerewolfBuff.ApplyCustomBuff(playerEntity).Start();
//     WerewolfStatsBuff.UpdateCustomBuff(playerEntity);
//   }

//   // function to apply stats and abilities to wolf form
//   public static IEnumerator UpdateWerewolfStats(Entity playerEntity)
//   {
//     if (playerEntity == Entity.Null) yield break;

//     WerewolfStatsBuff.UpdateCustomBuff(playerEntity);
//   }
// }