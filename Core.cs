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

    RegionService = new RegionService();
    PlayerService = new PlayerService();
    // DiscordWebhookService.SendTestMessageAsync();

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

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.Buff_Hunger_For_Power_Effect, out Entity hungerForPowerEffectBuffEntity))
    {
      // it has a ModifyMovementSpeedBuff component we want to delete
      if (EntityManager.TryGetComponentData<ModifyMovementSpeedBuff>(hungerForPowerEffectBuffEntity, out var modifyMovementSpeedBuff))
      {
        modifyMovementSpeedBuff.MoveSpeed = 1.05f; // change to 5% increase instead of removing
        EntityManager.SetComponentData(hungerForPowerEffectBuffEntity, modifyMovementSpeedBuff);
      }

      // it has an EmpowerBuff component we want to edit
      if (EntityManager.TryGetComponentData<EmpowerBuff>(hungerForPowerEffectBuffEntity, out var empowerBuff))
      {
        empowerBuff.EmpowerModifier = 0.1f; // original is 0.2f for 20% global damage increase
        EntityManager.SetComponentData(hungerForPowerEffectBuffEntity, empowerBuff);
      }
    }

    // public static readonly PrefabGUID AB_Werewolf_Bite_AbilityGroup = new PrefabGUID(-1789525825);
    // public static readonly PrefabGUID AB_Werewolf_Bite_BleedBuff = new PrefabGUID(1313262093);
    // public static readonly PrefabGUID AB_Werewolf_Bite_Cast = new PrefabGUID(1523382598);
    // public static readonly PrefabGUID AB_Werewolf_Bite_Hit = new PrefabGUID(-315731882);

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Werewolf_Bite_Cast, out Entity biteCastEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(biteCastEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original Werewolf Bite cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 30f; // change to 30 seconds
        EntityManager.SetComponentData(biteCastEntity, abilityCooldownData);
      }
    }

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Werewolf_Bite_Hit, out Entity biteHitEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<DealDamageOnGameplayEvent>(biteHitEntity, out var dealDamageOnGameplayEvent))
      {
        DealDamageParameters damageParameters = dealDamageOnGameplayEvent.Parameters;
        float mainFactor = damageParameters.MainFactor;
        float resourceModifier = damageParameters.ResourceModifier;
        float staggerFactor = damageParameters.StaggerFactor;
        float rawDamageValue = damageParameters.RawDamageValue;
        float rawDamagePercent = damageParameters.RawDamagePercent;
        int dealDamageFlags = damageParameters.DealDamageFlags;
        MainDamageType mainDamageType = damageParameters.MainType;

        Core.Log.LogInfo($"Original Werewolf Bite damage - MainFactor: {mainFactor}, ResourceModifier: {resourceModifier}, StaggerFactor: {staggerFactor}, RawDamageValue: {rawDamageValue}, RawDamagePercent: {rawDamagePercent}, DealDamageFlags: {dealDamageFlags}, MainDamageType: {mainDamageType}");

        float damageModifierPerHit = dealDamageOnGameplayEvent.DamageModifierPerHit;
        bool multiplyMainFactorWithStacks = dealDamageOnGameplayEvent.MultiplyMainFactorWithStacks;
        Core.Log.LogInfo($"Original Werewolf Bite DamageModifierPerHit: {damageModifierPerHit}, MultiplyMainFactorWithStacks: {multiplyMainFactorWithStacks}");

        dealDamageOnGameplayEvent.Parameters.RawDamagePercent = 50f;
        EntityManager.SetComponentData(biteHitEntity, dealDamageOnGameplayEvent);
      }
    }

    // public static readonly PrefabGUID AB_HighLord_SwordPrimary_MeleeAttack_AbilityGroup = new PrefabGUID(-328302080);
    // public static readonly PrefabGUID AB_HighLord_SwordPrimary_MeleeAttack_Cast01 = new PrefabGUID(761892370);
    // public static readonly PrefabGUID AB_HighLord_SwordPrimary_MeleeAttack_Cast02 = new PrefabGUID(-560217827);
    // public static readonly PrefabGUID AB_HighLord_SwordPrimary_MeleeAttack_Hit01 = new PrefabGUID(49816890);
    // public static readonly PrefabGUID AB_HighLord_SwordPrimary_MeleeAttack_Hit02 = new PrefabGUID(1433379105);

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_HighLord_SwordPrimary_MeleeAttack_Cast01, out Entity highLordMeleeAttackEntity))
    {
      // it has a AbilityCooldownData component we want to edit
      if (EntityManager.TryGetComponentData<AbilityCooldownData>(highLordMeleeAttackEntity, out var abilityCooldownData))
      {
        float originalCooldown = abilityCooldownData.Cooldown._Value;
        Core.Log.LogInfo($"Original High Lord Melee Attack cooldown: {originalCooldown} seconds.");
        abilityCooldownData.Cooldown._Value = 30f; // change to 30 seconds
        EntityManager.SetComponentData(highLordMeleeAttackEntity, abilityCooldownData);
      }
    }

    // public static readonly PrefabGUID AB_Shapeshift_Wolf_Buff = new PrefabGUID(-351718282);

    if (SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(PrefabGUIDs.AB_Shapeshift_Wolf_Buff, out Entity wolfShapeshiftBuffEntity))
    {
      if (EntityManager.TryGetComponentData<Script_Buff_Shapeshift_DataShared>(wolfShapeshiftBuffEntity, out var shapeshiftData))
      {
        Core.Log.LogInfo($"Original Wolf Shapeshift RemoveOnDamageTaken: {shapeshiftData.RemoveOnDamageTaken}");
        shapeshiftData.RemoveOnDamageTaken = false; // change to not remove on damage taken
        EntityManager.SetComponentData(wolfShapeshiftBuffEntity, shapeshiftData);
      }




      // it has a AbilityCooldownData component we want to edit
      // if (EntityManager.TryGetComponentData<DestroyData>(wolfShapeshiftBuffEntity, out var destroyOnGameplayEventComponent))
      // {
      //   // DestroyOnGameplayEventWho who = destroyOnGameplayEventComponent.Who;
      //   // DestroyOnGameplayEventType type = destroyOnGameplayEventComponent.Type;
      //   // DestroyReason destroyReason = destroyOnGameplayEventComponent.DestroyReason;
      //   // bool setTranslationToEventTranslation = destroyOnGameplayEventComponent.SetTranslationToEventTranslation;

      //   DestroyReason destroyReason = destroyOnGameplayEventComponent.DestroyReason;
      //   Core.Log.LogInfo($"Original Wolf Buff DestroyReason: {destroyReason}");
      //   // EntityManager.RemoveComponent<DestroyOnGameplayEvent>(wolfShapeshiftBuffEntity);
      // }

      // if (EntityManager.TryGetBuffer<GameplayEventListeners>(wolfShapeshiftBuffEntity, out var gameplayEventListeners))
      // {
      //   for (int i = gameplayEventListeners.Length - 1; i >= 0; i--)
      //   {
      //     GameplayEventListeners listener = gameplayEventListeners[i];
      //     int eventIdIndex = listener.EventIdIndex;
      //     int eventIndexOfType = listener.EventIndexOfType;
      //     GameplayEventTypeEnum gameplayEventType = listener.GameplayEventType;
      //     GameplayEventId gameplayEventId = listener.GameplayEventId;
      //     Core.Log.LogInfo($"Wolf Buff GameplayEventListener - EventIdIndex: {eventIdIndex}, EventIndexOfType: {eventIndexOfType}, GameplayEventType: {gameplayEventType}, GameplayEventId: {gameplayEventId}");
      //   }

      //   gameplayEventListeners.Clear();
      // }


      // EntityManager.RemoveComponent<DestroyData>(wolfShapeshiftBuffEntity);
      // EntityManager.RemoveComponent<DestroyOnGameplayEvent>(wolfShapeshiftBuffEntity);
      // EntityManager.RemoveComponent<RemoveBuffOnGameplayEvent>(wolfShapeshiftBuffEntity);
      // EntityManager.RemoveComponent<RemoveBuffOnGameplayEventEntry>(wolfShapeshiftBuffEntity);
      // EntityManager.RemoveComponent<Buff_Destroy_On_Owner_Death>(wolfShapeshiftBuffEntity);
      // EntityManager.RemoveComponent<Buff_Destroy_On_Owner_Death>(wolfShapeshiftBuffEntity);
      // EntityManager.RemoveComponent<BuffModificationFlagData>(wolfShapeshiftBuffEntity);
      // EntityManager.RemoveComponent<DestroyState>(wolfShapeshiftBuffEntity);


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