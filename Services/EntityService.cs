using Unity.Entities;
using ProjectM.Network;
using Unity.Collections;
using Unity.Transforms;
using ProjectM;

namespace SoVUtilities.Services;

public static class EntityService
{
  static EntityManager EntityManager => Core.EntityManager;

  public static bool TryFindPlayer(string playerName, out Entity playerEntity, out Entity userEntity)
  {
    // if the playerName is null or empty, we return false
    if (string.IsNullOrEmpty(playerName))
    {
      playerEntity = Entity.Null;
      userEntity = Entity.Null;
      return false;
    }

    playerEntity = Entity.Null;
    userEntity = Entity.Null;

    var userEntities = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<User>())
                       .ToEntityArray(Allocator.Temp);

    foreach (var entity in userEntities)
    {
      var userData = EntityManager.GetComponentData<User>(entity);
      if (userData.CharacterName.ToString().Equals(playerName, StringComparison.OrdinalIgnoreCase))
      {
        userEntity = entity;
        playerEntity = userData.LocalCharacter._Entity;
        return true;
      }
    }

    return false;
  }

  public static List<Entity> GetNearbyUserEntities(Entity playerEntity, float radius = 10f)
  {
    List<Entity> nearbyPlayers = new List<Entity>();
    if (!EntityManager.HasComponent<Translation>(playerEntity))
      return nearbyPlayers;

    var playerPos = EntityManager.GetComponentData<Translation>(playerEntity).Value;

    var playerCharactersQuery = QueryService.PlayerCharactersQuery;
    NativeArray<Entity> entities = playerCharactersQuery.ToEntityArray(Allocator.Temp);

    foreach (var entity in entities)
    {
      var playerCharacter = EntityManager.GetComponentData<PlayerCharacter>(entity);
      Entity userEntity = playerCharacter.UserEntity;
      if (EntityManager.HasComponent<Translation>(userEntity))
      {
        var userPos = EntityManager.GetComponentData<Translation>(userEntity).Value;
        float distance =
            (userPos.x - playerPos.x) * (userPos.x - playerPos.x) +
            (userPos.y - playerPos.y) * (userPos.y - playerPos.y) +
            (userPos.z - playerPos.z) * (userPos.z - playerPos.z);

        if (distance <= radius * radius)
        {
          nearbyPlayers.Add(userEntity);
        }
      }
    }

    return nearbyPlayers;
  }

  public static List<Entity> GetNearbyEntities(Entity playerEntity, float radius = 10f)
  {
    List<Entity> nearbyEntities = new List<Entity>();
    if (!EntityManager.HasComponent<Translation>(playerEntity))
      return nearbyEntities;

    var playerPos = EntityManager.GetComponentData<Translation>(playerEntity).Value;

    var query = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Translation>());
    var entities = query.ToEntityArray(Allocator.Temp);

    foreach (var entity in entities)
    {
      if (EntityManager.HasComponent<Translation>(entity))
      {
        var entityPos = EntityManager.GetComponentData<Translation>(entity).Value;
        float distance =
            (entityPos.x - playerPos.x) * (entityPos.x - playerPos.x) +
            (entityPos.y - playerPos.y) * (entityPos.y - playerPos.y) +
            (entityPos.z - playerPos.z) * (entityPos.z - playerPos.z);

        if (distance <= radius * radius)
        {
          nearbyEntities.Add(entity);
        }
      }
    }

    return nearbyEntities;
  }
}
