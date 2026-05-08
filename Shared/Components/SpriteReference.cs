using Hypercube.Ecs.Components;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct SpriteReference : IComponent
{
    public string Path = string.Empty;
    
    public SpriteReference()
    {
    }
}