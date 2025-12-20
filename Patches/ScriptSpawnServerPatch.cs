using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Shared.Systems;
using Stunlock.Core;
using Unity.Entities;
using static SoVUtilities.Services.BuffService;
using SoVUtilities.Resources;
using SoVUtilities.Services.Buffs;

namespace SoVUtilities.Patches;

[HarmonyPatch]
internal static class ScriptSpawnServerPatch
{
  static EntityManager EntityManager => Core.EntityManager;
  static readonly EntityQuery _query = QueryService.ScriptSpawnServerQuery;

  static readonly PrefabGUID ShapeshiftWolfSkinBuff = PrefabGUIDs.AB_Shapeshift_Wolf_Buff;
  static readonly PrefabGUID ShapeshiftWolfSkin01Buff = PrefabGUIDs.AB_Shapeshift_Wolf_Skin01_Buff;
  static readonly PrefabGUID ShapeshiftWolfSkin02Buff = PrefabGUIDs.AB_Shapeshift_Wolf_PMK_Skin02_Buff;
  static readonly PrefabGUID ShapeshiftWolfSkin03Buff = PrefabGUIDs.AB_Shapeshift_Wolf_Blackfang_Skin03_Buff;
  static readonly PrefabGUID ShapeshiftBearSkinBuff = PrefabGUIDs.AB_Shapeshift_Bear_Buff;
  static readonly PrefabGUID ShapeshiftBearSkin01Buff = PrefabGUIDs.AB_Shapeshift_Bear_Skin01_Buff;
  static readonly PrefabGUID ShapeshiftToadBuff = PrefabGUIDs.AB_Shapeshift_Toad_Buff;
  static readonly PrefabGUID ShapeshiftToadSkin01Buff = PrefabGUIDs.AB_Shapeshift_Toad_PMK_Skin01_Buff;
  static readonly PrefabGUID ShapeshiftSpiderBuff = PrefabGUIDs.AB_Shapeshift_Spider_Buff;
  static readonly PrefabGUID ShapeshiftHumanBuff = PrefabGUIDs.AB_Shapeshift_Human_Buff;
  static readonly PrefabGUID ShapeshiftHumanSkin01Buff = PrefabGUIDs.AB_Shapeshift_Human_Grandma_Skin01_Buff;
  static readonly PrefabGUID ShapeshiftHumanSkin02Buff = PrefabGUIDs.AB_Shapeshift_Human_PMK_Skin02_Buff;
  static readonly PrefabGUID HorseMountBuff = PrefabGUIDs.AB_Interact_Mount_Owner_Buff_Horse;
  static readonly PrefabGUID VampireHorseMountBuff = PrefabGUIDs.AB_Interact_Mount_Owner_Buff_Horse_Vampire;
  static readonly PrefabGUID VampireBlackfangHorseMountBuff = PrefabGUIDs.AB_Interact_Mount_Owner_Buff_Horse_Vampire_Blackfang;
  static readonly PrefabGUID VampireGloomrotHorseMountBuff = PrefabGUIDs.AB_Interact_Mount_Owner_Buff_Horse_Vampire_Gloomrot;
  static readonly PrefabGUID VampirePMKSkeletonHorseMountBuff = PrefabGUIDs.AB_Interact_Mount_Owner_Buff_Horse_Vampire_PMKSkeleton;



  static readonly PrefabGUID[] nameplateDisabledForms = new PrefabGUID[]
  {
    ShapeshiftWolfSkinBuff,
    ShapeshiftWolfSkin01Buff,
    ShapeshiftWolfSkin02Buff,
    ShapeshiftWolfSkin03Buff,
    ShapeshiftBearSkinBuff,
    ShapeshiftBearSkin01Buff,
    ShapeshiftToadBuff,
    ShapeshiftToadSkin01Buff,
    ShapeshiftSpiderBuff,
    ShapeshiftHumanBuff,
    ShapeshiftHumanSkin01Buff,
    ShapeshiftHumanSkin02Buff
  };

  [HarmonyPatch(typeof(ScriptSpawnServer), nameof(ScriptSpawnServer.OnUpdate))]
  [HarmonyPrefix]
  static void OnUpdatePrefix(ScriptSpawnServer __instance)
  {
    if (!Core._initialized) return;

    using NativeAccessor<Entity> entities = _query.ToEntityArrayAccessor();
    using NativeAccessor<PrefabGUID> prefabGuids = _query.ToComponentDataArrayAccessor<PrefabGUID>();
    using NativeAccessor<Buff> buffs = _query.ToComponentDataArrayAccessor<Buff>();

    ComponentLookup<PlayerCharacter> playerCharacterLookup = __instance.GetComponentLookup<PlayerCharacter>(true);

    try
    {
      for (int i = 0; i < entities.Length; i++)
      {
        Entity buffTarget = buffs[i].Target;
        PrefabGUID prefabGuid = prefabGuids[i];

        bool targetIsPlayer = playerCharacterLookup.HasComponent(buffTarget);
        bool isDebuff = buffs[i].BuffEffectType.Equals(BuffEffectType.Debuff);

        if (targetIsPlayer)
        {
          Core.Log.LogInfo($"[ScriptSpawnServer.OnUpdatePrefix] - Player Buff Applied: {prefabGuid} to {buffTarget}");
          var playerData = PlayerDataService.GetPlayerData(buffTarget);

          if (prefabGuid.Equals(PrefabGUIDs.AB_Shapeshift_Bear_Buff))
          {
            // check if they have the werewolf tag
            if (playerData.HasTag(TagService.Tags.WEREWOLF))
            {
              entities[i].Destroy();
              WerewolfBuff.ApplyCustomBuff(buffTarget).Start();
              WerewolfStatsBuff.ReapplyCustomBuff(buffTarget).Start();
              continue;
            }
          }

          if (prefabGuid.Equals(HorseMountBuff) || prefabGuid.Equals(VampireHorseMountBuff) || prefabGuid.Equals(VampireBlackfangHorseMountBuff) || prefabGuid.Equals(VampirePMKSkeletonHorseMountBuff))
          {
            if (playerData.HasTag(TagService.Tags.WEREWOLF))
            {
              entities[i].Destroy();
              continue;
            }
          }

          if (prefabGuid.Equals(ShapeshiftWolfSkinBuff) || prefabGuid.Equals(ShapeshiftWolfSkin01Buff) || prefabGuid.Equals(ShapeshiftWolfSkin02Buff) || prefabGuid.Equals(ShapeshiftWolfSkin03Buff))
          {
            if (playerData.HasTag(TagService.Tags.WEREWOLF))
            {
              WerewolfBuff.RemoveBuff(buffTarget);
              WerewolfStatsBuff.ReapplyCustomBuff(buffTarget).Start();
            }
          }

          if (prefabGuid.Equals(PrefabGUIDs.SpellPassive_Storm_T03_HungerForPower))
          {
            if (EntityManager.TryGetBuffer<ModifyUnitStatBuff_DOTS>(entities[i], out var modifyStatBuffer))
            {
              for (int j = 0; j < modifyStatBuffer.Length; j++)
              {
                Core.Log.LogInfo($"Modifying entry {j} in ModifyUnitStatBuff_DOTS buffer.");
                // AttributeCapType attributeCapType = modifyStatBuffer[j].AttributeCapType;
                UnitStatType statType = modifyStatBuffer[j].StatType;
                // ModificationType modificationType = modifyStatBuffer[j].ModificationType;
                // float value = modifyStatBuffer[j].Value;
                // float softCapValue = modifyStatBuffer[j].SoftCapValue;
                // float modifier = modifyStatBuffer[j].Modifier;
                // bool increaseByStacks = modifyStatBuffer[j].IncreaseByStacks;
                // float valueByStacks = modifyStatBuffer[j].ValueByStacks;
                // int priority = modifyStatBuffer[i].Priority;
                // Core.Log.LogInfo($"Before: AttributeCapType={attributeCapType}, StatType={statType}, ModificationType={modificationType}, Value={value}, SoftCapValue={softCapValue}, Modifier={modifier}, IncreaseByStacks={increaseByStacks}, ValueByStacks={valueByStacks}, Priority={priority}");

                // if (statType == UnitStatType.SpellLifeLeech)
                // {
                //   modifyStatBuffer[i] = new ModifyUnitStatBuff_DOTS
                //   {
                //     AttributeCapType = modifyStatBuffer[i].AttributeCapType,
                //     StatType = modifyStatBuffer[i].StatType,
                //     ModificationType = modifyStatBuffer[i].ModificationType,
                //     Value = 2f, // change to flat 2.0 increase
                //     SoftCapValue = modifyStatBuffer[i].SoftCapValue,
                //     Modifier = modifyStatBuffer[i].Modifier,
                //     IncreaseByStacks = modifyStatBuffer[i].IncreaseByStacks,
                //     ValueByStacks = modifyStatBuffer[i].ValueByStacks,
                //     Priority = modifyStatBuffer[i].Priority,
                //     Id = modifyStatBuffer[i].Id
                //   };
                // }

                modifyStatBuffer.Clear();
              }
            }
          }

          bool shouldNameplateBeDisabled = false;
          foreach (PrefabGUID disabledForm in nameplateDisabledForms)
          {
            if (prefabGuid.Equals(disabledForm))
            {
              shouldNameplateBeDisabled = true;
              break;
            }
          }

          if (shouldNameplateBeDisabled)
          {
            AddHideNameplateBuff(buffTarget);
          }
        }
      }
    }
    catch (Exception e)
    {
      Core.Log.LogWarning($"[ScriptSpawnServer.OnUpdatePrefix] - {e}");
    }
    finally
    {
      entities.Dispose();
      prefabGuids.Dispose();
      buffs.Dispose();
    }
  }
}