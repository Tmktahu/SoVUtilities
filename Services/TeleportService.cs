using ProjectM;
using Unity.Entities;
using ProjectM.Network;
using Unity.Transforms;
using Unity.Mathematics;

namespace SoVUtilities.Services;

public static class TeleportService
{
  public static EntityManager EntityManager => Core.EntityManager;
  public static SetMapMarkerSystem SetMapMarkerSystem => Core.SetMapMarkerSystem;

  public static void TeleportToEntity(Entity toTeleportEntity, Entity toTeleportUserEntity, Entity targetEntity)
  {
    if (toTeleportEntity == Entity.Null || targetEntity == Entity.Null)
    {
      Core.Log.LogError("Invalid entity provided for teleportation.");
      return;
    }

    float3 pos = Core.EntityManager.GetComponentData<Translation>(targetEntity).Value;
    TeleportToCoordinate(toTeleportEntity, toTeleportUserEntity, pos);
  }

  public static void TeleportToCoordinate(Entity entityToTeleport, Entity userEntityToTeleport, float3 pos)
  {
    if (entityToTeleport == Entity.Null || userEntityToTeleport == Entity.Null)
    {
      Core.Log.LogError("Invalid entity provided for teleportation.");
      return;
    }

    var archetype = Core.EntityManager.CreateArchetype(new ComponentType[] {
        ComponentType.ReadWrite<FromCharacter>(),
        ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
      });

    var entity = Core.EntityManager.CreateEntity(archetype);
    Core.EntityManager.SetComponentData(entity, new FromCharacter()
    {
      User = userEntityToTeleport,
      Character = entityToTeleport
    });

    Core.EntityManager.SetComponentData(entity, new PlayerTeleportDebugEvent()
    {
      Position = new float3(pos.x, pos.y, pos.z),
      Target = PlayerTeleportDebugEvent.TeleportTarget.Self
    });
  }

  // command to teleport to the user's waypoint
  public static void TeleportToMapMarker(Entity entityToTeleport, Entity userEntityToTeleport)
  {
    if (entityToTeleport == Entity.Null || userEntityToTeleport == Entity.Null)
    {
      Core.Log.LogError("Invalid entity provided for teleportation.");
      return;
    }

    if (SetMapMarkerSystem.TryFindUserCustomMapIcon(userEntityToTeleport, out Entity iconEntity))
    {
      float3 markerPosition = Core.EntityManager.GetComponentData<Translation>(iconEntity).Value;
      // we need to set the second float number to like 30
      markerPosition.y = 30f; // Adjust the height as needed
      TeleportToCoordinate(entityToTeleport, userEntityToTeleport, markerPosition);
    }
    else
    {
      Core.Log.LogWarning("No custom map icon found for the user.");
      return;
    }
  }
}