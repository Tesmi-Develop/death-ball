using Client.Components;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs;
using Hypercube.Utilities.Dependencies;

namespace Client.Systems.CharacterSystems;

public class CharacterBypassInterpolationSystem : EntitySystem
{
    [Dependency] private readonly GameHelperSystem _gameHelperSystem = null!;
    
    public override void Update(FrameEventArgs args)
    {
        var entity = _gameHelperSystem.GetLocalCharacter();
        
        if (entity == Entity.Invalid || !HasComponent<Interpolation>(entity))
            return;

        GetComponent<Interpolation>(entity).IsBypass = true;
    }
}