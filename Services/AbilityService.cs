
// using Unity.Entities;
// using Stunlock.Core;
// using ProjectM;

// namespace SoVUtilities.Services;

// public static class AbilityService
// {
//   static EntityManager EntityManager => Core.EntityManager;

//   public static void setAbility(Entity playerEntity, int targetSlot, PrefabGUID abilityPrefab)
//   {
//     // it has a AbilityGroupSlotBuffer
//     if (EntityManager.TryGetBuffer<AbilityGroupSlotBuffer>(playerEntity, out var abilityGroupSlotBuffer))
//     {
//       // Logic to set the ability in the buffer
//       foreach (var slot in abilityGroupSlotBuffer)
//       {
//         PrefabGUID prefabGuid = slot.BaseAbilityGroupOnSlot;
//         LogPrefabInfo(prefabGuid);
//       }
//     }
//   }
// }