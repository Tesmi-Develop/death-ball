using Hypercube.Ecs.Events;
using Server.Systems.Network;

namespace Server.Components.Events;

public struct ClientConnected : IEvent
{
    public ClientConnection ClientConnection;
}