using Hypercube.Core.Resources;
using Hypercube.Utilities.Dependencies;
using Shared;

namespace Server.Systems;

[EcsSystem()]
public class LoadArenaTest : BaseSystem
{
    [Dependency] private readonly IResourceManager _resourceManager = null!;
    private MapHandler _mapHandler = null!;
    private PrototypeStorage _prototypeStorage = null!;
    
    public override void Initialize()
    {
        _mapHandler = new MapHandler("/TiledMaps/Arena1/arena1.tmj");
        _mapHandler.Compile(_resourceManager);
        
        _prototypeStorage = _resourceManager.Load<PrototypeStorage>("/prototypes.json");
        _mapHandler.Load(world, _prototypeStorage);
    }

    public override void Update(long tick)
    {
        
    }
}