using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Physics.Shapes;
using Server.Extensions;
using Shared.Components;
using Shared.Extensions;

namespace Server.Systems;

[EcsSystem]
public class TilesetRefHandlerSystem : BaseSystem
{
    private Query _query = null!;
    private readonly List<Entity> _entities = [];

    public override void Initialize()
    {
        _query = GetQuery().WithAll<TilesetRefComponent, HitboxDeclarationComponent>().WithNone<HitboxComponent>()
            .Build();
    }

    public override void Update(long tick)
    {
        foreach (var entity in world.CollectEntities(_query, _entities))
        {
            ref var hitboxDeclaration = ref world.Get<HitboxDeclarationComponent>(entity);
            ref var tilesetRef = ref  world.Get<TilesetRefComponent>(entity);
            
            if (hitboxDeclaration.ShapeType == ShapeType.Polygon)
                world.AddCollision(entity, tilesetRef.Size, isStatic: true);
        }
    }
}