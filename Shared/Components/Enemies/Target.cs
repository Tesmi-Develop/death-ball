using Hypercube.Ecs;
using Shared.Attributes;

namespace Shared.Components.Enemies;

[SyncComponent]
public partial struct Target
{
    public long EntityMask;
    
    [NonSynced]
    public Entity TargetEntity;
}