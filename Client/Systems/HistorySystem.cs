using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Input;
using Hypercube.Core.Input.Handler;
using Hypercube.Utilities.Dependencies;

namespace Client.Systems;

public class HistorySystem : EntitySystem
{
    [Dependency] private readonly IInputHandler _inputHandler = null!;
    
    public override void Update(FrameEventArgs args)
    {
        
    }
}