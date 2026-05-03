using Arch.Core;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Server.Events;
using Server.Utilities;
using Shared.Helpers;

namespace Server;

public static class Program
{
    public static readonly int TickRate = 30;
    
    public static void Main()
    {
        MessagePackHelper.SetupMessagePack();
        var logger = new ConsoleLogger();
        var world = World.Create();
        var dependenciesContainer = new DependenciesContainer();
        var eventBus = new EventBus();
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