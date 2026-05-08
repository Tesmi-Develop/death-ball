using Arch.Core;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Shared.Components;

namespace Server.Systems;

[EcsSystem(EcsPriority.UpdateCollisionWorld)]
public class CollisionWorldSystem : BaseSystem
{
    private const int CellSize = 128;
    
    // ChunkId -> List<Entity>
    private readonly Dictionary<Vector2i, List<Entity>> _grid = new();
    private readonly QueryDescription _query = new QueryDescription().WithAll<NetworkTransform, HitboxComponent>();

    public override void Initialize()
    {
        /*world.SubscribeComponentRemoved<HitboxComponent>((in entity, ref hitbox) =>
        {
            Console.WriteLine("removed hitbox");
            UnregisterEntity(entity, ref hitbox);
        });*/
    }

    public override void Update(long tick)
    {
        world.Query(in _query, (Entity entity, ref NetworkTransform trans, ref HitboxComponent hitbox, ref HitboxComponent presence) =>
        {
            var currentGridIndex = WorldToGrid(trans.Position);
            
            if (currentGridIndex == presence.GridIndex)
                return;
        
            UpdateRegistration(entity, ref presence, currentGridIndex);
        });
    }

    private void UpdateRegistration(Entity entity, ref HitboxComponent presence, Vector2i currentGridIndex)
    {
        if (presence.GridIndex is { } prev)
        {
            if (!_grid.TryGetValue(prev, out var list)) 
                return;
            
            list.Remove(entity);
        }
        
        presence.GridIndex = currentGridIndex;

        {
            if (_grid.TryGetValue(currentGridIndex, out var list))
            {
                list.Add(entity);
                return;
            }
        }
        
        _grid[currentGridIndex] = [entity];
    }

    public void GetNearby(Vector2i gridId, List<Entity> result)
    {
        var rect2 = new Rect2i(gridId.X - 1, gridId.Y + 1, gridId.X + 1, gridId.Y - 1);

        for (var x = rect2.Left; x < rect2.Right + 1; x++)
        {
            for (var y = rect2.Bottom; y < rect2.Top + 1; y++)
            {
                if (!_grid.TryGetValue((x, y), out var list)) 
                    continue;

                result.AddRange(list);
            }
        }
    }
    
    public void UnregisterEntity(Entity entity, ref HitboxComponent presence)
    {
        if (presence.GridIndex is not { } prev) 
            return;
        
        if (_grid.TryGetValue(prev, out var list))
            list.Remove(entity);
    }

    public Vector2i WorldToGrid(Vector2 pos) => 
        new((int)(pos.X / CellSize), (int)(pos.Y / CellSize));
}