using System.Buffers;
using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using Server.Components;
using Server.Components.Events;
using Server.Events;
using Shared.Data;
using Shared.Helpers;

namespace Server.Systems.Network;

[EcsSystem(EcsPriority.High)]
public class NetworkClientPacketSystem : BaseSystem
{
    [Dependency] private readonly IEventBus _eventBus = null!;
    [Dependency] private readonly Logger _logger = null!;
    [Dependency] private readonly NetworkServer _networkServer = null!;
    
    private readonly QueryDescription _query = new QueryDescription().WithAll<ClientData>();
    private readonly ArrayBufferWriter<byte> _bufferWriter = new(1024);

    public override void Initialize()
    {
        _eventBus.Subscribe((ref ClientConnected args) =>
        {
            RegisterClientEntity(args.ClientConnection);
        });
    }

    private void RegisterClientEntity(ClientConnection clientConnection)
    {
        var entity = world.Create();
        world.Add(entity, new ClientData { ClientConnection = clientConnection, Id = clientConnection.Id });
        
        _eventBus.Raise<ClientData, NewEntityClient>(entity, new NewEntityClient { ClientEntity = entity });
    }

    private void HandleIncomingPackets(Entity entity, ref ClientData clientData)
    {
        while (clientData.ClientConnection.IncomingPackets.TryDequeue(out var packet))
        {
            clientData.IncomingPackets.Enqueue(packet);
        }
    }

    private void HandleOutgoingPackets(ref ClientData clientData)
    {
        while (clientData.PendingPackets.TryDequeue(out var packet))
        {
            var finalData = new byte[1 + 8 + packet.Data.Length];
            finalData[0] = (byte)packet.PacketType;
            MessagePackHelper.WriteInt64(new Span<byte>(finalData, 1, 8), packet.Tick);
            packet.Data.CopyTo(finalData.AsMemory(1 + 8));
            
            if (packet.DeliveryType != DeliveryMethod.Unreliable)
                _logger.Debug($"Sending packet {packet.PacketType}");
            
            clientData.ClientConnection.Send(finalData, packet.DeliveryType);
        }
    }
    
    public override void BeforeUpdate(long tick)
    {
        world.Query(in _query, (Entity entity, ref ClientData clientData) =>
        {
           HandleIncomingPackets(entity, ref clientData);
           HandleOutgoingPackets(ref clientData);
        });
    }
}