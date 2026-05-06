using Arch.Core;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Components;
using Server.Components.Events;
using Server.Events;
using Server.Extensions;
using Shared.Components;
using Shared.Data;

namespace Server.Systems;

[EcsSystem]
public class SpawnPlayerCharacterSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly IEventBus _eventBus = null!;
    
    public override void Initialize()
    {
        _eventBus.Subscribe((Entity playerEntity, ref ClientData playerData, ref NewEntityClient _) =>
        { 
            SpawnPlayerCharacter(playerEntity, ref playerData);
        });
    }

    private void SpawnPlayerCharacter(Entity playerEntity, ref ClientData playerData)
    {
        var characterEntity = world.Create();
        world.Add(playerEntity, new ControlledEntity { Reference = characterEntity});
        
        world.Add(characterEntity, new NetworkTransform { Position = Vector2.Zero });
        world.Add(characterEntity, new Speed { Value = 4f });
        world.Add(characterEntity, new PlayerCharacter { ClientId = playerData.Id });
        world.AddCollision(characterEntity, new Vector2(32, 32));
    }
}