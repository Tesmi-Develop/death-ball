using Hypercube.Core;
using Hypercube.Core.Resources;
using Hypercube.Ecs;
using Hypercube.Ecs.Events;
using Hypercube.Physics;
using Hypercube.Physics.Collision;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Utilities;
using Shared.Helpers;

namespace Server;

public static class Program
{
    private static readonly int TickRate = 60;

    private static void InitResourceManager(DependenciesContainer dependenciesContainer)
    {
        var instance = new ResourceManager();
        dependenciesContainer.RegisterSingleton<IResourceManager>(instance);
        instance.AddAllLoaders();
        instance.Mount(Config.MountFolders);
    }
    
    public static void Main()
    {
        Thread.CurrentThread.Name = "Main";
        var dependenciesContainer = new DependenciesContainer();
        
        Contacts.Initialize();
        MessagePackHelper.SetupMessagePack();
        InitResourceManager(dependenciesContainer);
        
        var logger = new ConsoleLogger();
        var world = new World();
        var eventBus = world.Events;
        var time = new Time();
        
        dependenciesContainer.RegisterSingleton<IEventBus>(eventBus);
        dependenciesContainer.RegisterSingleton<Time>(time);
        
        var systemHandler = new EcsSystemHandler(world, logger, dependenciesContainer);
        
        systemHandler.InvokePreInitialize();
        systemHandler.InvokeInitialize();
        systemHandler.InvokePostInitialize();
        
        var tickDelta = 1000d / TickRate;

        var stopwatch = time.Stopwatch;
        stopwatch.Start();

        double accumulator = 0;
        var previous = stopwatch.Elapsed.TotalMilliseconds;

        while (true)
        {
            var current = stopwatch.Elapsed.TotalMilliseconds;
            var frameTime = current - previous;
            previous = current;

            accumulator += frameTime;

            var maxTicksPerFrame = 5;
            var processed = 0;

            while (accumulator >= tickDelta && processed < maxTicksPerFrame)
            {
                systemHandler.InvokeUpdateCycle(time.Tick);

                accumulator -= tickDelta;
                time.Tick++;

                processed++;
            }

            Thread.Yield();
        }
    }
}