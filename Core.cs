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
  public static RegionService RegionService { get; private set; }
  public static SequenceService SequenceService { get; private set; }
  public static EntityManager EntityManager => World.EntityManager;
  private static SystemService _systemService;
  public static SystemService SystemService => _systemService ??= new(World);
  public static ServerGameManager ServerGameManager => SystemService.ServerScriptMapper.GetServerGameManager();
  public static SystemMessageSystem SystemMessageSystem => SystemService.SystemMessageSystem;
  public static ServerBootstrapSystem ServerBootstrapSystem => SystemService.ServerBootstrapSystem;
  public static SetMapMarkerSystem SetMapMarkerSystem => SystemService.SetMapMarkerSystem;
  public static DebugEventsSystem DebugEventsSystem => SystemService.DebugEventsSystem;
  public static FactionLookupSystem FactionLookupSystem => SystemService.FactionLookupSystem;

  static MonoBehaviour _monoBehaviour;

  public static void Initialize()
  {
    // Initialize player data service
    PlayerDataService.Initialize();

    RegionService = new RegionService();
    PlayerService = new PlayerService();
    SequenceService = new SequenceService();
    // DiscordWebhookService.SendTestMessageAsync();

    // Initialize mod data service
    ModDataService.Initialize();
    // Initialize item data service
    ItemDataService.Initialize();

    PrefabService prefabService = new PrefabService();
    prefabService.ModifyPrefabs();

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