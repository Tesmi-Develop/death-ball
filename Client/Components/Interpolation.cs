using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;

namespace Client.Components;

public struct Interpolation : IComponent
{
    public bool IsBypass = false;
    public Queue<(long Tick, Vector2 Position)> Snapshots = new();
    public float ClientInterpolationTime = 0;
    public Vector2 LastPosition = Vector2.Zero;

    public Interpolation()
    {
    }
}