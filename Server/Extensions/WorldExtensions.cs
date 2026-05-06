using Arch.Core;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Shapes;
using Hypercube.Physics.Shapes.Structs;
using Shared.Components;
using Shared.Components.Commands;
using Shared.Data;
using ShapeType = Hypercube.Physics.Shapes.ShapeType;

namespace Server.Extensions;

public static class WorldExtensions 
{
    extension(World world)
    {
        public Entity GetFirstEntity(in QueryDescription queryDescription)
        {
            var query = world.Query(in queryDescription);
            foreach (ref var chunk in query)
            {
                if (chunk.Count > 0)
                {
                    return chunk.Entity(0);
                }
            }
        
            return Entity.Null;
        }

        public void AddCollision(Entity entity, Vector2 size, Vector2? offset = null, bool isTrigger = false, bool isStatic = false)
        {
            offset ??= Vector2.Zero;
            
            world.Add(entity, new HitboxComponent
            {
                Shape = new ShapeUnionTyped()
                {
                    Shape = new ShapeUnion()
                    {
                        Polygon = ShapePolygon.CreateRectangle(size / 2f)
                    },
                    Type = ShapeType.Polygon,
                },
                IsTrigger = isTrigger,
                IsStatic = isStatic,
                Offset = offset.Value,
            });
        }
        
        public void AddCollision(Entity entity, float radius, Vector2? offset = null, bool isTrigger = false, bool isStatic = false)
        {
            offset ??= Vector2.Zero;
            
            world.Add(entity, new HitboxComponent
            {
                Shape = new ShapeUnionTyped()
                {
                    Shape = new ShapeUnion()
                    {
                        Circle = new ShapeCircle
                        {
                            Radius = radius
                        }
                    },
                    Type = ShapeType.Circle,
                },
                IsTrigger = isTrigger,
                IsStatic = isStatic,
                Offset = offset.Value,
            });
        }
    }
}