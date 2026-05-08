using Arch.Core;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Components;
using Server.Components.Events;
using Server.Events;
using Server.Extensions;
using Shared.Components;

namespace Server.Systems;

[EcsSystem]
public class SpawnerPlayerCharacterSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly IEventBus _eventBus = null!;
    private QueryDescription _queryDescription = new QueryDescription().WithAll<PlayerSpawner, NetworkTransform>();
    private QueryDescription _queryPlayers = new QueryDescription().WithAll<ClientData>().WithNone<ControlledEntity>();
    
    public override void Initialize()
    { 
        _eventBus.Subscribe((Entity playerEntity, ref ClientData playerData, ref NewEntityClient _) =>
        {
            InitiateSpawnPlayerCharacters();
        });
    }

    public void InitiateSpawnPlayerCharacters()
    {
        var entity = world.GetFirstEntity(_queryDescription);
        if (entity == Entity.Null)
        {
            _logger.Warning("Not found player spawner entity");
            return;
        }
        
        world.Query(in _queryPlayers, (Entity clientEntity, ref ClientData clientData) =>
        {
            var networkTransform = world.Get<NetworkTransform>(entity);
            SpawnPlayerCharacter(clientEntity, networkTransform.Position, ref clientData);
        });
        
        _eventBus.Subscribe((Entity clientEntity, ref ClientData clientData, ref ClientEntityRemoved _) =>
        {
            DespawnPlayerCharacter(clientEntity);
        });
    }

    public void SpawnPlayerCharacter(Entity playerEntity, Vector2 position, ref ClientData playerData)
    {
        var characterEntity = world.Create();
        world.Add(playerEntity, new ControlledEntity { Reference = characterEntity });
        
        world.Add(characterEntity, new NetworkTransform { Position = position });
        world.Add(characterEntity, new SpriteReference { Path = string.Empty }); //TODO player sprite
        world.Add(characterEntity, new Speed { Value = 4f });
        world.Add(characterEntity, new PlayerCharacter { ClientId = playerData.Id });
        world.AddCollision(characterEntity, new Vector2(32, 32), isTrigger: true);
    }

    public void DespawnPlayerCharacter(Entity clientEntity)
    {
        if (!world.Has<ControlledEntity>(clientEntity))
            return;
        
        var controlled = world.Get<ControlledEntity>(clientEntity);
        var clientData = world.Get<ClientData>(clientEntity);
        if (!world.IsAlive(controlled.Reference))
            return;
        
        world.Destroy(controlled.Reference);
        _logger.Debug($"Player character with id {clientData.Id}");
    }
}