using System.Buffers;
using Client.Systems.PredictSystems;
using Hypercube.Core.Ecs;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using MessagePack;
using Shared.Data;

namespace Client.Systems;

public class NetworkHelper : EntitySystem
{
    [Dependency] private readonly GameClient _client = null!;
    [Dependency] private readonly ServerUpdateHandlerSystem  _serverUpdateHandlerSystem = null!;
    private readonly ArrayBufferWriter<byte> _buffer = new(1025);
    
    public void SendRequest<T>(T request, DeliveryMethod deliveryMethod, long tick = -1)
    {
        _buffer.Clear();
        
        var id = NumeratorGenerator.GetId(typeof(T));
        var writer = new MessagePackWriter(_buffer);
        
        writer.WriteInt32(id);
        writer.WriteInt64(tick);
        NetworkFactory.SerializeRequestComponent(id, ref writer, request);
        writer.Flush();
        
        _client.Send(PacketType.Request, _buffer.WrittenMemory.ToArray(), deliveryMethod);
    }

    public void SendInputIfPredicting<T>(T request, DeliveryMethod deliveryMethod)
    {
        if (_serverUpdateHandlerSystem.IsRollback)
            return;
        
        SendRequest(request, deliveryMethod, _serverUpdateHandlerSystem.PredictTick);
    }
}