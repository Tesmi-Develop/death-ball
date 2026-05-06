using Client.Components;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Shared.Components;

namespace Client.Systems;

public class SpriteGiverSystem : EntitySystem
{
    [Dependency] private GameClient _gameClient = null!;
    private Query _query = null!;
    private List<Entity> _entities = new();

    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform>().WithNone<SpriteComponent>().Build();
    }

    public override void Update(FrameEventArgs deltaTime)
    {
        _entities.Clear();
        
        var i = 0;
        _query.With<NetworkTransform>((e, ref transform) =>
        {
            i++;
            
            if (HasComponent<SpriteComponent>(e))
                Console.WriteLine(e);
            
            _entities.Add(e);
        });

        foreach (var e in _entities)
        {
            AddComponent(e, new TransformComponent());
            AddComponent(e, new SpriteComponent() { Path = "/textures/default.png" });

            var intep = new InterpolationComponent();
            
            if (HasComponent<PlayerCharacter>(e) && GetComponent<PlayerCharacter>(e).ClientId == _gameClient.Id)
                intep.IsBypass = true;
            
            AddComponent(e, intep);
        } 
        
        if (i > 0)
            Console.WriteLine(i);
    }
}