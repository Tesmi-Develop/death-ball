using Client.Extensions;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Shared.Components;
using Shared.Extensions;

namespace Client.Systems.Givers;

public class TransformGiverSystem : EntitySystem
{
    private Query _query = null!;
    private List<Entity> _entities = [];

    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform>().WithNone<TransformComponent>().Build();
    }

    public override void Update(FrameEventArgs args)
    {
        foreach (var e in World.CollectEntities(_query, _entities))
        {
            var transform = GetComponent<NetworkTransform>(e);
            AddComponent(e, new TransformComponent { LocalPosition = (Vector3)transform.Position });
        }
    }
}