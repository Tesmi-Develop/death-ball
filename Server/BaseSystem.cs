using Arch.Core;
using Hypercube.Utilities.Dependencies;

namespace Server;

public class BaseSystem
{
    [Dependency] public readonly World world = null!;
    
    public virtual void PreInitialize() { }
    public virtual void Initialize() { }
    public virtual void PostInitialize() { }
    public virtual void BeforeUpdate(long tick) { }
    public virtual void Update(long tick) { }
    public virtual void AfterUpdate(long tick) { }
}