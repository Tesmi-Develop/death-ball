using Hypercube.Ecs.Events;

namespace Client.Events;

public struct ComponentDirtyEvent : IEvent
{
    public long Tick;
    public int ComponentId;
}