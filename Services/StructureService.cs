

using ProjectM;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace SoVUtilities.Services;

public static class StructureService
{
  static EntityManager EntityManager => Core.EntityManager;
  public static void ToggleContainerNameVisibility(Entity structureEntity)
  {
    if (EntityManager.TryGetComponentData<NameableInteractable>(structureEntity, out var nameableInteractable))
    {
      FixedString64Bytes name = nameableInteractable.Name;
      bool visible = nameableInteractable.OnlyAllySee;
      bool OnlyAllyRename = nameableInteractable.OnlyAllyRename;

      Core.Log.LogInfo($"Name of structure {structureEntity} is '{name}', OnlyAllyRename: {OnlyAllyRename}, OnlyAllySee: {visible}");
      Core.Log.LogInfo($"Setting container name visibility to {visible} for structure {structureEntity}");
      nameableInteractable.OnlyAllySee = !visible;
      EntityManager.SetComponentData(structureEntity, nameableInteractable);
    }
  }

  public static void GetContainerNameVisibility(Entity structureEntity, out bool visible)
  {
    visible = false;
    if (EntityManager.TryGetComponentData<NameableInteractable>(structureEntity, out var nameableInteractable))
    {
      visible = !nameableInteractable.OnlyAllySee;
    }
  }

  public static Entity GetClosestContainerEntity(Entity playerEntity, float maxDistance = 5f)
  {
    Entity closestContainer = Entity.Null;
    float closestDistanceSq = maxDistance * maxDistance;

    if (!EntityManager.HasComponent<Translation>(playerEntity))
      return closestContainer;

    var playerPos = EntityManager.GetComponentData<Translation>(playerEntity).Value;

    var structureQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<NameableInteractable>());
    var structureEntities = structureQuery.ToEntityArray(Allocator.Temp);

    foreach (var structureEntity in structureEntities)
    {
      if (EntityManager.HasComponent<NameableInteractable>(structureEntity) &&
          EntityManager.HasComponent<Translation>(structureEntity))
      {
        var structurePos = EntityManager.GetComponentData<Translation>(structureEntity).Value;
        float distance = (structurePos.x - playerPos.x) * (structurePos.x - playerPos.x) +
                         (structurePos.y - playerPos.y) * (structurePos.y - playerPos.y) +
                         (structurePos.z - playerPos.z) * (structurePos.z - playerPos.z);
        if (distance < closestDistanceSq)
        {
          closestDistanceSq = distance;
          closestContainer = structureEntity;
        }
      }
    }

    return closestContainer;
  }
}