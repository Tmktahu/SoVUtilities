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

namespace SOVUtilities;

public static class VExtensions
{
  static EntityManager EntityManager => Core.EntityManager;
  static ServerGameManager ServerGameManager => Core.ServerGameManager;

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
}
