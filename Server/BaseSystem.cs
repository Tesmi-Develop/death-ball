using System.Reflection;
using Hypercube.Core.Ecs;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Dependencies;
using Hypercube.Utilities.Helpers;

namespace Server;

public class BaseSystem : Hypercube.Ecs.System.EntitySystem<long>, IPostInject
{
    [Dependency] public readonly World world = null!;
    
    public void OnPostInject()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
        const string property = nameof(EntitySystem.World);
        
        // If the property setter is private, it does not exist in the inherited class.
        // Working to go one level below, DeclaringType of the PropertyInfo
        var propertyInfo = GetType()
            .GetProperty(property, flags)?
            .DeclaringType?
            .GetProperty(property, flags);
            
        propertyInfo?.SetValue(this, world);
    }
    
    public virtual void PreInitialize() { }
    public virtual void PostInitialize() { }
}