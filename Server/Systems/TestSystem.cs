using Arch.Core;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Random;
using Hypercube.Mathematics.Shapes;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Server.Components;
using Server.Events;
using Server.Helpers;
using Server.Utilities;
using Shared.Components;

namespace Server.Systems;

[EcsSystem]
public class TestSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    [Dependency] private readonly Time _time = null!;
    private Rect2 _arenaSize = new Rect2(-200, 200, 200, -200);
    private Xoshiro256 _random = new(new Random().Next());
    private QueryDescription _query;
    private QueryDescription _query2;
    private float _speed = 5;
    private int counter = 0;
    
    public override void PostInitialize()
    {
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            Console.WriteLine("Start test");
            for (var i = 0; i < 1; i++)
            {
                var entity2 = world.Create();
                world.Add(entity2, new NetworkTransform() { Position = Vector2.One });
                world.Add(entity2, new TargetLocation() { Location = Vector2.One });
            }
        });
 
        _query = new QueryDescription().WithAll<NetworkTransform, TargetLocation>();
    }

    private Vector2 GetRandomPosition()
    {
        var newPoint = new Vector2(_random.NextFloat(_arenaSize.Left, _arenaSize.Right), _random.NextFloat(_arenaSize.Bottom, _arenaSize.Top));
        return newPoint;
    }

    public override void Update(long tick)
    {
        world.Query(in _query, (Entity entity, ref NetworkTransform networkTransform, ref TargetLocation location) =>
        {
            if ((location.Location - networkTransform.Position).Length <= 10)
            {
                location.Location = GetRandomPosition();
            }

            networkTransform.Position = new Vector2(
                HyperMath.MoveTowards(networkTransform.Position.X, location.Location.X, _speed), 
                HyperMath.MoveTowards(networkTransform.Position.Y, location.Location.Y, _speed));
            
            NetworkHelper.MakeDirty<NetworkTransform>(world, entity);
        });
    }
}