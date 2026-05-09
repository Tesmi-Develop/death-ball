using System.Buffers;
using System.Collections.Concurrent;
using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Hypercube.Ecs.Events;
using Hypercube.Ecs.Lifetime;
using Hypercube.Ecs.Queries;
using Server.Extensions;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using LiteNetLib;
using MessagePack;
using Server.Components;
using Server.Components.Events;
using Server.Helpers;
using Shared.Attributes;
using Shared.Data;
using Shared.Extensions;
using Shared.NetworkUtilities;

namespace Server.Systems;

[EcsSystem(EcsPriority.Low)]
public class SyncSystem : BaseSystem
{
    [Dependency] private readonly ILogger _logger = null!;
    [Dependency] private readonly IEventBus _eventBus = null!;
    
    private Query _query = null!;
    private Query _clientQuery = null!;
    private Query _syncQuery = null!;
    private readonly ArrayBufferWriter<byte> _bufferWriter = new(1024); 
    private readonly ConcurrentQueue<(Entity entity, int NetId)> _removalQueue = [];
    private readonly ConcurrentQueue<(Entity entity, int NetId)> _additionalQueue = [];
    private readonly ConcurrentQueue<long> _destroyedEntities = [];
    private readonly List<Entity> _tempEntityList = [];
    private readonly ArrayBufferWriter<byte> _tempBuffer = new (1024);
    
    private void RegisterRemovalHook<T>(int netId) where T : struct, IComponent
    {
        _eventBus.Subscribe<T, RemovedEvent>(Handler);
        return;

        void Handler(Entity entity, ref T comp, ref RemovedEvent args) 
        {
            _removalQueue.Enqueue((entity, netId));
        }
    }
    
    private void RegisterAddedHook<T>(int netId) where T : struct, IComponent
    {
        _eventBus.Subscribe<T, AddedEvent>(Handler);
        return;

        void Handler(Entity entity, ref T comp, ref AddedEvent args) 
        {
            world.Add<NetworkEntityTag>(entity);
            _additionalQueue.Enqueue((entity, netId));
        }
    }

    [Priority(EcsPriority.High - 1)]
    public override void PreInitialize()
    {
        _query = GetQuery().WithAll<Dirty>().Build();
        _clientQuery = GetQuery().WithAll<ClientData>().Build();
        _syncQuery = GetQuery().WithAll<NetworkEntityTag>().Build();
    
        var networkComponents = NetworkHelper.GetNetworkComponentMetadata(world);

        foreach (var (netId, type) in networkComponents.ComponentsById)
        {
            _logger.Trace($"Registering {type.Name}");
            var method = GetType().GetMethod(nameof(RegisterRemovalHook), 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        
            var genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(this, [netId]);
            
            method = GetType().GetMethod(nameof(RegisterAddedHook), 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        
            genericMethod = method.MakeGenericMethod(type);
            genericMethod.Invoke(this, [netId]);
        }
        
        _eventBus.Subscribe((Entity entity, ref NetworkEntityTag _, ref RemovedEvent _) =>
        {
            _destroyedEntities.Enqueue(entity.GetFullMask());
        });
        
        _eventBus.Subscribe((Entity _, ref ClientData playerData, ref NewEntityClient _) =>
        {
            SendFullStateToClient(ref playerData);
        });
    }

    public void SendFullStateToClient(ref ClientData clientData)
    {
        var networkComponents = NetworkHelper.GetNetworkComponentMetadata(world);
        _bufferWriter.Clear();

        var counts = world.CountEntities(_syncQuery);
        MessagePackSerializer.Serialize(_bufferWriter, counts);
        
        _syncQuery.ForEach(entity =>
        {
            MessagePackSerializer.Serialize(_bufferWriter, entity.GetFullMask());
            
            var validComponents = 0;
            foreach (var (_, type) in networkComponents.ComponentsById)
                if (world.Has(entity, type)) validComponents++;

            MessagePackSerializer.Serialize(_bufferWriter, validComponents);
            
            foreach (var (netId, type) in networkComponents.ComponentsById)
            {
                if (!world.Has(entity, type)) 
                    continue;
                
                var componentData = world.Get(entity, type);
                MessagePackSerializer.Serialize(_bufferWriter, netId);
                ((ISynced)componentData!).Serialize(_bufferWriter, null);
            }
        });
        
        var packet = new Packet
        {
            PacketType = PacketType.Hydrate,
            Data = _bufferWriter.WrittenSpan.ToArray()
        };
        
        clientData.PendingPackets.Enqueue(packet);
    }

    public override void AfterUpdate(long tick)
    {
        _bufferWriter.Clear();
        
        SerializeDestroyedEntities();
        BroadcastSyncPacket(PacketType.EntitiesDeletion, DeliveryMethod.ReliableOrdered, tick);
        _bufferWriter.Clear();
        
        SerializeRemovedComponents();
        BroadcastSyncPacket(PacketType.ComponentsDeletion, DeliveryMethod.ReliableOrdered, tick);
        _bufferWriter.Clear();
        
        SerializeAddedComponents();
        BroadcastSyncPacket(PacketType.ComponentsAddition, DeliveryMethod.ReliableOrdered, tick);
        _bufferWriter.Clear();
        
        SerializeDirtyChanges();
        BroadcastSyncPacket(PacketType.Dirty, DeliveryMethod.Unreliable, tick);
        _bufferWriter.Clear();
    }

    private void SerializeDestroyedEntities()
    {
        var actualCount = 0;
        var writer = new MessagePackWriter(_tempBuffer);
        
        while (_destroyedEntities.TryDequeue(out var mask))
        {
            writer.WriteInt64(mask);
            actualCount++;
        }

        writer.Flush();

        if (actualCount <= 0) 
            return;
        
        _bufferWriter.Clear();
        MessagePackSerializer.Serialize(_bufferWriter, actualCount);
        _bufferWriter.Write(_tempBuffer.WrittenSpan);
    }

    private void SerializeRemovedComponents()
    {
        _tempBuffer.Clear();
        var writer = new MessagePackWriter(_tempBuffer);
    
        var actualCount = 0;
        while (_removalQueue.TryDequeue(out var result))
        {
            var (entity, netId) = result;
            if (!world.Validate(entity)) 
                continue;

            writer.WriteInt64(entity.GetFullMask());
            writer.WriteInt32(netId);
            actualCount++;
        }
        
        writer.Flush();

        if (actualCount <= 0) 
            return;
        
        _bufferWriter.Clear();
        MessagePackSerializer.Serialize(_bufferWriter, actualCount);
        _bufferWriter.Write(_tempBuffer.WrittenSpan);
    }
    
    private void SerializeAddedComponents()
    {
        if (_additionalQueue.IsEmpty)
            return;
        
        _logger.Trace("Start serialize added components");
        _tempBuffer.Clear();
        var writer = new MessagePackWriter(_tempBuffer);
        
        var actualCount = 0;
        
        while (_additionalQueue.TryDequeue(out var result))
        {
            var (entity, componentId) = result;
            if (!world.Validate(entity)) 
                continue;

            writer.WriteInt64(entity.GetFullMask());
            writer.WriteInt32(componentId);
            writer.Flush();
            
            var componentType = NetworkHelper.GetNetworkComponentById(world, componentId);
            var synced = (ISynced)world.Get(entity, componentType)!;
            synced.Serialize(_tempBuffer, null);
            writer = new MessagePackWriter(_tempBuffer);
            
            _logger.Trace($"Wrote {componentType.Name}, entityMask: {entity.GetFullMask()}");
            actualCount++;
        }
        
        writer.Flush();
        
        if (actualCount <= 0) 
            return;
        
        _bufferWriter.Clear();
        MessagePackSerializer.Serialize(_bufferWriter, actualCount);
        _bufferWriter.Write(_tempBuffer.WrittenSpan);
    }

    private void SerializeDirtyChanges()
    {
        var dirtyEntitiesCount = world.CountEntities(_query);
        MessagePackSerializer.Serialize(_bufferWriter, dirtyEntitiesCount);
        
        _tempEntityList.Clear(); 

        _query.With((Entity entity, ref Dirty dirty) =>
        {
            _tempEntityList.Add(entity);
            MessagePackSerializer.Serialize(_bufferWriter, entity.GetFullMask());

            if (dirty.ComponentIds.Count == 0)
            {
                MessagePackSerializer.Serialize(_bufferWriter, 0);
                _logger.Warning($"Entity {entity} is Dirty but has no component IDs.");
                return;
            }

            MessagePackSerializer.Serialize(_bufferWriter, dirty.ComponentIds.Count);
            foreach (var id in dirty.ComponentIds)
            {
                MessagePackSerializer.Serialize(_bufferWriter, id);
                var componentType = NetworkHelper.GetNetworkComponentById(world, id);
                var synced = (ISynced)world.Get(entity, componentType)!;
                synced.Serialize(_bufferWriter, null);
            }
        });

        foreach(var e in _tempEntityList) 
            world.Remove<Dirty>(e);
    }

    private void BroadcastSyncPacket(PacketType packetType, DeliveryMethod deliveryType, long tick)
    {
        if (_bufferWriter.WrittenCount <= 3) 
            return;
        
        var packet = new Packet
        {
            PacketType = packetType,
            DeliveryType = deliveryType,
            Data = _bufferWriter.WrittenSpan.ToArray(),
            Tick = tick
        };
        
        PushAllClientsSyncData(packet);
    }

    private void PushAllClientsSyncData(Packet packet)
    {
        _clientQuery.With((Entity _, ref ClientData payload) =>
        {
            payload.PendingPackets.Enqueue(packet);
        });
    }
}