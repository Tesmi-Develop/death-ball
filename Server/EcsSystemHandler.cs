using System.Reflection;
using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Hypercube.Utilities.Helpers;
using Server.Utilities;
using Shared.Attributes;

namespace Server;

public class EcsSystemHandler
{
    private readonly List<BaseSystem> _allSystems;
    private readonly IDependenciesContainer _dependenciesContainer;
    private readonly ILogger _logger;
    
    private readonly List<BaseSystem> _preInitializeSystems = [];
    private readonly List<BaseSystem> _initializeSystems = [];
    private readonly List<BaseSystem> _postInitializeSystems = [];
    
    private readonly List<BaseSystem> _preUpdateSystems = [];
    private readonly List<BaseSystem> _updateSystems = [];
    private readonly List<BaseSystem> _postUpdateSystems = [];
    
    public EcsSystemHandler(World world, ILogger logger, IDependenciesContainer dependenciesContainer)
    {
        _logger = logger;
        _dependenciesContainer = dependenciesContainer;
        _dependenciesContainer.RegisterSingleton<World>(world);
        
        PrepareLogger();
        
        _allSystems = InstantiateSystems();
        
        PreparePhaseSystems();
    }

    private List<BaseSystem> InstantiateSystems()
    {
        var priorities = new List<(Type Type, int Priority)>();
        var baseSystemType = typeof(BaseSystem);
        
        foreach (var (type, attributeData) in ReflectionHelper.GetAllTypesWithAttribute<EcsSystemAttribute>())
        {
            if (type.IsAbstract || !type.IsAssignableTo(baseSystemType))
            {
                _logger.Warning($"Class {type.Name} does not implement {baseSystemType.Name}");
                continue;
            }
        
            _logger.Trace($"Found system: {type.Name}");
            _dependenciesContainer.Register(type);
            priorities.Add((type, attributeData.Priority));
        }
    
        _dependenciesContainer.ResolveAll();
        
        return priorities
            .OrderByDescending(static p => p.Priority)
            .Select(p => (BaseSystem)_dependenciesContainer.Resolve(p.Type))
            .ToList();
    }

    private void PreparePhaseSystems()
    {
        _preInitializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(BaseSystem.PreInitialize)));
        _initializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(BaseSystem.Initialize)));
        _postInitializeSystems.AddRange(SortByMethodPriority(_allSystems, nameof(BaseSystem.PostInitialize)));

        _preUpdateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(BaseSystem.BeforeUpdate)));
        _updateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(BaseSystem.Update)));
        _postUpdateSystems.AddRange(SortByMethodPriority(_allSystems, nameof(BaseSystem.AfterUpdate)));
    }

    private IEnumerable<BaseSystem> SortByMethodPriority(List<BaseSystem> systems, string methodName)
    {
        return systems
            .Select(s => new
            {
                System = s,
                Priority = s.GetType()
                    .GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    ?.GetCustomAttribute<PriorityAttribute>()?.Priority ?? 0
            })
            .OrderByDescending(x => x.Priority)
            .Select(x => x.System);
    }
    
    public void InvokePreInitialize() => InvokePhase(_preInitializeSystems, static s => s.PreInitialize(), "PreInitialize");
    public void InvokeInitialize() => InvokePhase(_initializeSystems, static s => s.Initialize(), "Initialize");
    public void InvokePostInitialize() => InvokePhase(_postInitializeSystems, static s => s.PostInitialize(), "PostInitialize");

    public void InvokeBeforeUpdate(long tick)
    {
        var count = _preUpdateSystems.Count;
        for (var i = 0; i < count; i++)
        {
            _preUpdateSystems[i].BeforeUpdate(tick);
        }
    }

    public void InvokeUpdate(long tick)
    {
        var count = _updateSystems.Count;
        for (var i = 0; i < count; i++)
        {
            _updateSystems[i].Update(tick);
        }
    }

    public void InvokeAfterUpdate(long tick)
    {
        var count = _postUpdateSystems.Count;
        for (var i = 0; i < count; i++)
        {
            _postUpdateSystems[i].AfterUpdate(tick);
        }
    }

    private void InvokePhase(List<BaseSystem> systems, Action<BaseSystem> action, string phaseName)
    {
        foreach (var system in systems)
        {
            action(system);
            _logger.Trace($"{phaseName} system: {system.GetType().Name}");
        }
    }

    public void InvokeUpdateCycle(long tick)
    {
        InvokeBeforeUpdate(tick);
        InvokeUpdate(tick);
        InvokeAfterUpdate(tick);
    }
    
    private void PrepareLogger()
    {
        _dependenciesContainer.Register<ILogger>((_, target) =>
        {
            if (target is null)
                return new ConsoleLogger();
            
            return new MyLogger(target);
        }, DependencyLifetime.Transient);
        
        _dependenciesContainer.Register<Logger>((_, target) =>
        {
            if (target is null)
                return new ConsoleLogger();
            
            return new MyLogger(target);
        }, DependencyLifetime.Transient);
    }
}