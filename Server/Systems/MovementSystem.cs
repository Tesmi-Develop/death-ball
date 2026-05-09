using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Server.Components;
using Server.Extensions;
using Server.Helpers;
using Shared.Components;
using Shared.Components.Commands;

namespace Server.Systems;

[EcsSystem]
public class MovementSystem : BaseSystem
{
    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<ClientData>().Build();
    }

    public override void Update(long tick)
    {
        _query.With((Entity clientEntity, ref ClientData clientData) =>
        {
            if (!world.Has<ControlledEntity>(clientEntity))
                return;
            
            var characterEntity = world.Get<ControlledEntity>(clientEntity).Reference;
            if (!world.Validate(characterEntity))
                return;

            if (!NetworkHelper.TryGetInputFromTick<MoveRequest>(world, clientEntity, tick, out var inputData))
                return;
            
            if (!world.Has<NetworkTransform>(characterEntity) || !world.Has<Speed>(characterEntity))
                return;
            
            ref var transform = ref world.Get<NetworkTransform>(characterEntity);
            ref var speed = ref world.Get<Speed>(characterEntity);
            
            transform.Position += inputData.Direction * speed.Value;
            NetworkHelper.MakeDirty<NetworkTransform>(world, characterEntity);
        });
    }
}