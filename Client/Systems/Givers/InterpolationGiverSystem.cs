using Client.Components;
using Client.Extensions;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Shared.Components;
using Shared.Extensions;

namespace Client.Systems.Givers;

public class InterpolationGiverSystem : EntitySystem
{
    private Query _query = null!;
    private readonly List<Entity> _entities = [];

    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform>().WithNone<Interpolation>().Build();
    }

    public override void Update(FrameEventArgs args)
    {
        foreach (var e in World.CollectEntities(_query, _entities))
        {
            AddComponent(e, new Interpolation());
        }
    }
}