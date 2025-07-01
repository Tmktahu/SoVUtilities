using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SoVUtilities;

public static class VExtensions
{
  static EntityManager EntityManager => Core.EntityManager;
  static ServerGameManager ServerGameManager => Core.ServerGameManager;
  const string PREFIX = "Entity(";
  const int LENGTH = 7;

  public static bool TryGetComponent<T>(this Entity entity, out T componentData) where T : struct
  {
    componentData = default;

    if (entity.Has<T>())
    {
      componentData = entity.Read<T>();
      return true;
    }

    return false;
  }

  public static unsafe bool TryGetBuffer<T>(this Entity entity, out DynamicBuffer<T> dynamicBuffer) where T : struct
  {
    if (ServerGameManager.TryGetBuffer(entity, out dynamicBuffer))
    {
      return true;
    }

    dynamicBuffer = default;
    return false;
  }

  public static bool Has<T>(this Entity entity) where T : struct
  {
    return EntityManager.HasComponent(entity, new(Il2CppType.Of<T>()));
  }
  public static T Read<T>(this Entity entity) where T : struct
  {
    return EntityManager.GetComponentData<T>(entity);
  }
  public static ulong GetSteamId(this Entity entity)
  {
    if (entity.TryGetComponent(out PlayerCharacter playerCharacter))
    {
      return playerCharacter.UserEntity.GetUser().PlatformId;
    }
    else if (entity.TryGetComponent(out User user))
    {
      return user.PlatformId;
    }

    return 0;
  }
  public static User GetUser(this Entity entity)
  {
    if (entity.TryGetComponent(out User user)) return user;
    else if (entity.TryGetComponent(out PlayerCharacter playerCharacter) && playerCharacter.UserEntity.TryGetComponent(out user)) return user;

    return User.Empty;
  }
  public static NetworkId GetNetworkId(this Entity entity)
  {
    if (entity.TryGetComponent(out NetworkId networkId))
    {
      return networkId;
    }

    return NetworkId.Empty;
  }
  public static bool IsPlayer(this Entity entity)
  {
    if (entity.Has<PlayerCharacter>())
    {
      return true;
    }

    return false;
  }
  public static Entity GetUserEntity(this Entity entity)
  {
    if (entity.TryGetComponent(out PlayerCharacter playerCharacter)) return playerCharacter.UserEntity;
    else if (entity.Has<User>()) return entity;

    return Entity.Null;
  }
  public static bool Exists(this Entity entity)
  {
    return entity != Entity.Null && entity.IndexWithinCapacity() && EntityManager.Exists(entity);
  }
  public static bool IndexWithinCapacity(this Entity entity)
  {
    string entityStr = entity.ToString();
    ReadOnlySpan<char> span = entityStr.AsSpan();

    if (!span.StartsWith(PREFIX)) return false;
    span = span[LENGTH..];

    int colon = span.IndexOf(':');
    if (colon <= 0) return false;

    ReadOnlySpan<char> tail = span[(colon + 1)..];

    int closeRel = tail.IndexOf(')');
    if (closeRel <= 0) return false;

    // Parse numbers
    if (!int.TryParse(span[..colon], out int index)) return false;
    if (!int.TryParse(tail[..closeRel], out _)) return false;

    // Single unsigned capacity check
    int capacity = EntityManager.EntityCapacity;
    bool isValid = (uint)index < (uint)capacity;

    if (!isValid)
    {
      // Core.Log.LogWarning($"Entity index out of range! ({index}>{capacity})");
    }

    return isValid;
  }
  public static PrefabGUID GetPrefabGuid(this Entity entity)
  {
    if (entity.TryGetComponent(out PrefabGUID prefabGuid)) return prefabGuid;

    return PrefabGUID.Empty;
  }
  public static NativeAccessor<Entity> ToEntityArrayAccessor(this EntityQuery entityQuery, Allocator allocator = Allocator.Temp)
  {
    NativeArray<Entity> entities = entityQuery.ToEntityArray(allocator);
    return new(entities);
  }
  public static NativeAccessor<T> ToComponentDataArrayAccessor<T>(this EntityQuery entityQuery, Allocator allocator = Allocator.Temp) where T : unmanaged
  {
    NativeArray<T> components = entityQuery.ToComponentDataArray<T>(allocator);
    return new(components);
  }
}
