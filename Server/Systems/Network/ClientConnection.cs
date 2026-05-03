using System.Collections.Concurrent;
using LiteNetLib;
using LiteNetLib.Utils;
using Shared.Data;

namespace Server.Systems.Network;

public class ClientConnection
{
    public readonly NetPeer Peer;
    public readonly ConcurrentQueue<Packet> IncomingPackets = new();

    public ClientConnection(NetPeer peer)
    {
        Peer = peer;
    }

    public void Send(byte[] data, DeliveryMethod deliveryMethod)
    {
        if (Peer.ConnectionState != ConnectionState.Connected)
            return;

        Peer.Send(data, deliveryMethod);
    }
    
    public void OnPacketReceive(NetDataReader reader, PacketType packetType)
    {
        var data = reader.GetRemainingBytes();
        if (data is null)
            return;
        
        IncomingPackets.Enqueue(new Packet
        {
            PacketType = packetType, 
            Data= new Memory<byte>(data, 0, data.Length),
        });
    }
}