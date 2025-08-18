using ProjectM;
using Unity.Entities;
using ProjectM.CastleBuilding;

namespace SoVUtilities.Services;

public static class BuildingService
{
  static EntityManager EntityManager = Core.EntityManager;

  public static void RepairAroundTarget(Entity target, int radius = 10)
  {
    List<Entity> damagedBuildings = EntityService.GetNearbyEntities(target, radius);
    foreach (var building in damagedBuildings)
    {
      if (!EntityManager.HasComponent<CastleHeartConnection>(building))
        continue;

      if (EntityManager.TryGetComponentData<Health>(building, out var health))
      {
        if (health.Value < health.MaxHealth.Value)
        {
          health.Value = health.MaxHealth.Value; // Set health to max
          EntityManager.SetComponentData(building, health);
        }
      }
    }
  }
}