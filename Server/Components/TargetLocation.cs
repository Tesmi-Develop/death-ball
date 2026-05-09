using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;

namespace Server.Components;

public struct TargetLocation : IComponent
{
    public Vector2 Location;
}