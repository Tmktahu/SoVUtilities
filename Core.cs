using BepInEx.Logging;
using ProjectM.Scripting;
using Unity.Entities;
using System.Collections;
using Unity.Collections;
using SoVUtilities.Services;
using UnityEngine;
using ProjectM.Physics;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ProjectM;
using SoVUtilities.Resources;
using SoVUtilities.Services;

// using Il2CppSystem;

using ProjectM.Gameplay.Scripting;
using Stunlock.Core;

namespace SoVUtilities;

internal static class Core
{
  public static ManualLogSource Log => Plugin.LogInstance;

  public static bool _initialized = false;
  public static World World { get; } = GetServerWorld() ?? throw new Exception("There is no Server world!");
  public static ModificationIDs ModificationIdGenerator = Core.ModificationIdGenerator;
  public static PlayerService PlayerService { get; private set; }
  public static EntityManager EntityManager => World.EntityManager;
  private static SystemService _systemService;
  public static SystemService SystemService => _systemService ??= new(World);
  public static ServerGameManager ServerGameManager => SystemService.ServerScriptMapper.GetServerGameManager();
  public static SystemMessageSystem SystemMessageSystem => SystemService.SystemMessageSystem;
  public static ServerBootstrapSystem ServerBootstrapSystem => SystemService.ServerBootstrapSystem;
  public static SetMapMarkerSystem SetMapMarkerSystem => SystemService.SetMapMarkerSystem;
  public static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;
  static MonoBehaviour _monoBehaviour;

  public static void Initialize()
  {
    // Initialize player data service
    PlayerDataService.Initialize();

    PlayerService = new PlayerService();

    // Initialize mod data service
    ModDataService.Initialize();

    ModifyPrefabs();

    _initialized = true;
  }

  static World GetServerWorld()
  {
    return World.s_AllWorlds.ToArray().FirstOrDefault(world => world.Name == "Server");
  }

  public static void StartCoroutine(IEnumerator routine)
  {
    if (_monoBehaviour == null)
    {
      _monoBehaviour = new GameObject(MyPluginInfo.PLUGIN_NAME).AddComponent<IgnorePhysicsDebugSystem>();
      UnityEngine.Object.DontDestroyOnLoad(_monoBehaviour.gameObject);
    }

    _monoBehaviour.StartCoroutine(routine.WrapToIl2Cpp());
  }

  static void ModifyPrefabs()
  {
    // Modify prefabs as needed
    if (ConfigService.DisableBatForm)
    {
      if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.Tech_Collection_VBlood_T08_BatVampire, out Entity prefabEntity))
      {
        // it has a ProgressionBookShapeshiftElement buffer
        if (EntityManager.TryGetBuffer<ProgressionBookShapeshiftElement>(prefabEntity, out var shapeshiftBuffer))
        {
          // it only has one, so we just clear the buffer
          shapeshiftBuffer.Clear();
        }
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.SpellPassive_Storm_T03_HungerForPower, out Entity hungerForPowerBuffEntity))
    {
      Core.Log.LogInfo("Modifying Hunger For Power Buff.");
      // it has a ModifyUnitStatBuff_DOTS buffer
      if (EntityManager.TryGetBuffer<ModifyUnitStatBuff_DOTS>(hungerForPowerBuffEntity, out var modifyStatBuffer))
      {
        for (int i = 0; i < modifyStatBuffer.Length; i++)
        {
          Core.Log.LogInfo($"Modifying entry {i} in ModifyUnitStatBuff_DOTS buffer.");
          UnitStatType statType = modifyStatBuffer[i].StatType;
          ModificationType modificationType = modifyStatBuffer[i].ModificationType;
          float value = modifyStatBuffer[i].Value;
          float modifier = modifyStatBuffer[i].Modifier;
          Core.Log.LogInfo($"Before: StatType={statType}, ModificationType={modificationType}, Value={value}, Modifier={modifier}");
        }
      }

      // it has a ProjectM.Gameplay.Scripting.Script_ApplyBuffOnAbilityUseData component
      // if (EntityManager.TryGetComponentData<Script_ApplyBuffOnAbilityUseData>(hungerForPowerBuffEntity, out var applyBuffData))
      // {
      //   Core.Log.LogInfo("Found Script_ApplyBuffOnAbilityUseData component.");
      //   PrefabGUID buff = applyBuffData.Buff;
      //   PrefabGUID ignoreIfBuffIsActive = applyBuffData.IgnoreIfBuffIsActive;
      //   Core.Log.LogInfo($"Before Modification: Buff={buff}, IgnoreIfBuffIsActive={ignoreIfBuffIsActive}");
      // }

    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.Buff_Hunger_For_Power_Effect, out Entity hungerForPowerEffectBuffEntity))
    {
      // it has a ModifyMovementSpeedBuff component we want to delete
      if (EntityManager.HasComponent<ModifyMovementSpeedBuff>(hungerForPowerEffectBuffEntity))
      {
        Core.Log.LogInfo("Removing ModifyMovementSpeedBuff component from Hunger For Power Effect Buff.");
        EntityManager.RemoveComponent<ModifyMovementSpeedBuff>(hungerForPowerEffectBuffEntity);
      }

      // it has an EmpowerBuff component we want to edit
      if (EntityManager.TryGetComponentData<EmpowerBuff>(hungerForPowerEffectBuffEntity, out var empowerBuff))
      {
        float empowerModifier = empowerBuff.EmpowerModifier;
        Core.Log.LogInfo($"Before Modification: EmpowerModifier={empowerModifier}");
        empowerBuff.EmpowerModifier = 0.05f; // original is 0.2f for 20% global damage increase
        EntityManager.SetComponentData(hungerForPowerEffectBuffEntity, empowerBuff);
      }
    }

  }
}

public readonly struct NativeAccessor<T> : IDisposable where T : unmanaged
{
  static NativeArray<T> _array;
  public NativeAccessor(NativeArray<T> array)
  {
    _array = array;
  }
  public T this[int index]
  {
    get => _array[index];
    set => _array[index] = value;
  }
  public int Length => _array.Length;
  public NativeArray<T>.Enumerator GetEnumerator() => _array.GetEnumerator();
  public void Dispose() => _array.Dispose();
}