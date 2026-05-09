using Hypercube.Ecs.Queries;
using Server.Components;

namespace Server.Systems;

[EcsSystem(EcsPriority.Low)]
public class DestroySystem : BaseSystem
{
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<DestroyTag>().Build();
    }

    public override void AfterUpdate(long tick)
    {
        _query.ForEach(entity =>
        {
            world.Delete(entity);
        });
    }
}