using Server.Systems.Network;
using Shared.Data;

namespace Server.Components;

public struct ClientData
{
    public Queue<Packet> PendingPackets = [];
    public Queue<Packet> IncomingPackets = [];
    public ClientConnection ClientConnection = null!;
    public InputData[] InputsWithTick = new InputData[60];
    public List<InputData> Inputs = [];
    public long Id = 0;

    public ClientData()
    {
    }
}