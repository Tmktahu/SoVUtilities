using SoVUtilities.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Shared.Systems;
using Stunlock.Core;
using Unity.Entities;
using static SoVUtilities.Services.BuffService;
using SoVUtilities.Resources;

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