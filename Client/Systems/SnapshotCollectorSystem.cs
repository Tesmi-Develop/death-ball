using Client.Components;
using Client.LifeCycles;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;

namespace Client.Systems;

public class SnapshotCollectorSystem : EntitySystem, IServerUpdate
{
    [Dependency] private readonly GameClient _client = null!;
    private Query _query = null!;
    
    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, InterpolationComponent>().Build();
    }
    
    public override void Update(FrameEventArgs args)
    {
        
    }

    public void ServerUpdate(long serverTick, long predictTick)
    {
        _query.With<NetworkTransform, InterpolationComponent>((entity, ref networkTransform, ref interpolation) =>
        {
            var pos = networkTransform.Position;
            
            if (interpolation.Snapshots.Count > 0 && serverTick <= interpolation.Snapshots.Last().Tick)
                return;
            
            if (pos == interpolation.LastPosition && interpolation.Snapshots.Count > 0)
            {
                return; 
            }

            interpolation.LastPosition = pos;
            interpolation.Snapshots.Enqueue((serverTick, pos));
        
            // Ограничиваем размер очереди, но с запасом под лаги
            while (interpolation.Snapshots.Count > 20)
                interpolation.Snapshots.Dequeue();
        });
    }
}