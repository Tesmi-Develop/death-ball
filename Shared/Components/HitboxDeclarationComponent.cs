using Hypercube.Ecs.Components;
using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Shapes;

namespace Shared.Components;

public struct HitboxDeclarationComponent : IComponent
{
    public ShapeType ShapeType;
    public Vector2? Size;
}