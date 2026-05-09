using Hypercube.Ecs;
using Hypercube.Ecs.Events;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Components;
using Server.Components.Events;
using Server.Extensions;
using Shared.Components;
using Shared.Extensions;

namespace Server.Systems;

[EcsSystem]
public class SpawnerPlayerCharacterSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly IEventBus _eventBus = null!;
    private Query _queryDescription = null!;
    private Query _queryPlayers = null!;

    public override void PreInitialize()
    {
        _queryDescription = GetQuery().WithAll<PlayerSpawner, NetworkTransform>().Build();
        _queryPlayers = GetQuery().WithAll<ClientData>().WithNone<ControlledEntity>().Build();
    }

    public override void Initialize()
    {
        _eventBus.Subscribe((Entity playerEntity, ref ClientData playerData, ref NewEntityClient _) =>
        {
            InitiateSpawnPlayerCharacters();
        });
        
        _eventBus.Subscribe((Entity clientEntity, ref ClientData clientData, ref ClientEntityRemoved _) =>
        {
            DespawnPlayerCharacter(clientEntity);
        });
    }

    public void InitiateSpawnPlayerCharacters()
    {
        var entity = world.GetFirstEntity(_queryDescription);
        if (entity == Entity.Invalid)
        {
            _logger.Warning("Not found player spawner entity");
            return;
        }
        
        _queryPlayers.With((Entity clientEntity, ref ClientData clientData) =>
        {
            var networkTransform = world.Get<NetworkTransform>(entity);
            SpawnPlayerCharacter(clientEntity, networkTransform.Position, ref clientData);
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
        if (!world.Validate(controlled.Reference))
            return;
        
        world.Delete(controlled.Reference);
        Console.WriteLine(Thread.CurrentThread.Name);
        _logger.Debug($"Player character with id {clientData.Id}");
    }
}