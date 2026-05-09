using Hypercube.Ecs;
using Hypercube.Ecs.Events;

namespace Server.Components.Events;

public struct NewEntityClient : IEvent
{
    public Entity ClientEntity;
}