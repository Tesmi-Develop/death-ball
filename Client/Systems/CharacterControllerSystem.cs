using Client.Data;
using Client.LifeCycles;
using Client.Systems.PredictSystems;
using Hypercube.Core.Ecs;
using Hypercube.Core.Input.Handler;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Shared.Components;
using Shared.Components.Commands;

namespace Client.Systems;

public class CharacterControllerSystem : EntitySystem, IServerUpdate
{
    [Dependency] private readonly IInputHandler _inputHandler = null!;
    [Dependency] private readonly InputStorage _inputStorage = null!;
    [Dependency] private readonly NetworkHelper _networkHelper = null!;
    [Dependency] private readonly GameClient _gameClient = null!;
    
    private Query _query = null!;
    
    public override void Initialize()
    {
        _query = GetQuery().WithAll<NetworkTransform, PlayerCharacter, Speed>().Build();
    }

    public void ServerUpdate(long serverTick, long predictTick)
    {
        _query.With<NetworkTransform, PlayerCharacter, Speed>((entity, ref transform, ref playerCharacter, ref speed) =>
        {
            if (_gameClient.Id != playerCharacter.ClientId)
                return;
            
            var direction = new Vector2();
        
            if (_inputStorage.HasInput(Input.MoveUp))
                direction += Vector2.UnitY;
        
            if (_inputStorage.HasInput(Input.MoveDown))
                direction -= Vector2.UnitY;
        
            if (_inputStorage.HasInput(Input.MoveLeft))
                direction -= Vector2.UnitX;
        
            if (_inputStorage.HasInput(Input.MoveRight))
                direction += Vector2.UnitX;

            if (direction != Vector2.Zero)
            {
                _networkHelper.SendInputIfPredicting(new MoveRequest { Direction = direction }, DeliveryMethod.Unreliable);
                transform.Position += direction * speed.Value;
            }
        });
    }
}