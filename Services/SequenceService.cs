








using ProjectM;
using ProjectM.Sequencer;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace SoVUtilities.Services;

internal class SequenceService
{
  static EntityManager EntityManager => Core.EntityManager;
  readonly SystemService SystemService = Core.SystemService;

  public void Spawn(Entity playerChar, SequenceGUID guid, int lifetime)
  {
    var pos = playerChar.Read<Translation>().Value;

    if (!SystemService.PrefabCollectionSystem._PrefabGuidToEntityMap.TryGetValue(new(651179295), out var prefab))
    {
      Core.Log.LogError("PrefabEntity not found!");
      return;
    }

    var entity = Core.EntityManager.Instantiate(prefab);

    entity.Add<PhysicsCustomTags>();
    entity.Write(new Translation { Value = pos });

    entity.HasWith((ref SpawnSequenceForEntity ss) =>
        {
          ss.SequenceGuid = guid;
          ss.Target = playerChar;
          ss.SecondaryTarget = Entity.Null;
        }
    );

    entity.HasWith((ref LifeTime lf) =>
        {
          lf.Duration = lifetime;
          lf.EndAction = LifeTimeEndAction.Destroy;
        }
    );
  }

  public void Despawn(Entity playerChar, SequenceGUID guid)
  {
    var networkedSequenceQuery = QueryService.NetworkedSequenceQuery;
    NativeArray<Entity> entities = networkedSequenceQuery.ToEntityArray(Allocator.Temp);

    foreach (var entity in entities)
    {
      var ss = entity.Read<SpawnSequenceForEntity>();
      if (ss.Target._Entity == playerChar && ss.SequenceGuid == guid)
      {
        Core.EntityManager.DestroyEntity(entity);
      }
    }
  }

  public void ListActiveSequences(Entity playerChar)
  {
    var networkedSequenceQuery = QueryService.NetworkedSequenceQuery;
    NativeArray<Entity> entities = networkedSequenceQuery.ToEntityArray(Allocator.Temp);

    Core.Log.LogInfo($"Active Sequences for Player {playerChar}:");

    foreach (var entity in entities)
    {
      if (EntityManager.TryGetComponentData<SpawnSequenceForEntity>(entity, out var ss))
      {
        if (ss.Target._Entity == playerChar)
        {
          Core.Log.LogInfo($"- Sequence GUID: {ss.SequenceGuid}");
        }
      }
    }
  }
}