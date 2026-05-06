using Hypercube.Core.Ecs;
using Hypercube.Utilities.Dependencies;

namespace Client.Systems.Collisions;

public class TestCollisionsSystem : EntitySystem
{
    [Dependency] private GameClient _gameClient = null!;
    
    public override void Initialize()
    {
        
    }
}