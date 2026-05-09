using Hypercube.Ecs;
using Hypercube.Ecs.Components;

namespace Server.Components;

public struct ControlledEntity : IComponent
{
    public Entity Reference;
}