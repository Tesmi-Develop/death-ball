using Hypercube.Ecs.Components;

namespace Server.Components;

public struct Dirty : IComponent
{
    public HashSet<int> ComponentIds = [];

    public Dirty()
    {
    }
}