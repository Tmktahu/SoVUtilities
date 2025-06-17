using Unity.Entities;
using ProjectM.Network;
using Unity.Collections;

namespace SoVUtilities.Services;

public static class EntityService
{
  static EntityManager EntityManager => Core.EntityManager;

  public static bool TryFindPlayer(string playerName, out Entity playerEntity, out Entity userEntity)
  {
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
}
