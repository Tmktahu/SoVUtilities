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


namespace SoVUtilities;

internal static class Core
{
  public static ManualLogSource Log => Plugin.LogInstance;

  public static bool _initialized = false;
  public static World World { get; } = GetServerWorld() ?? throw new Exception("There is no Server world!");
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