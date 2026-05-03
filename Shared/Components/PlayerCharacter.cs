using Hypercube.Ecs.Components;
using Shared.Attributes;

namespace Shared.Components;

[SyncComponent]
public partial struct PlayerCharacter : IComponent
{
    public int ClientId;
}