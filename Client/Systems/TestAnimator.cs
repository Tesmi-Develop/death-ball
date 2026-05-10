using Client.Components.AnimationComponents;
using Client.Utilities;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components;
using Shared.SharedSystemRealisation;

namespace Client.Systems;

[EcsSystem]
public class TestAnimator : BaseSystem
{
    [Dependency] private readonly AnimatorSystem _animator = null!;
    public override void Initialize()
    {
        var entity = EntityCreate();
        AddComponent<NetworkTransform>(entity);
        _animator.Play(entity, "enemy/Idle");
    }
}