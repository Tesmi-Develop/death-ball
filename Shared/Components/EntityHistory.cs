using Hypercube.Ecs.Components;
using Shared.Data;

namespace Shared.Components;

public struct EntityHistory : IComponent
{
    // ID -> fields
    public Dictionary<int, List<FieldHistoryBuffer>> Buffers = [];
    public bool NeedsRollback = false;
    public long RollbackTick = 0;

    public EntityHistory()
    {
    }
}