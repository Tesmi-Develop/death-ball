using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Shared.Attributes;
using Shared.Components;

namespace Server.Systems;

//[EcsSystem]
public class UnitTestSystem : BaseSystem
{
    public struct MyCompo : IComponent
    {
        
    }

    [Priority(EcsPriority.High + 1)]
    public override void PreInitialize()
    {
        var query = GetQuery().WithAll<MyCompo>().Build();
        var seed = 50;
        var random = new Random(seed);
        for (int j = 0; j < 5; j++)
        {
            var lastEntity = Entity.Invalid;
            var preLast = Entity.Invalid;
            var entities = new List<Entity>();
        
            for (var i = 0; i < 250; i++)
            {
                var entity = world.Create();
                entities.Add(entity);
                world.Add(entity, new MyCompo());

                if (random.Next(0, 2) == 0)
                {
                    world.Add(entity, new NetworkTransform());
                }
                
                if (random.Next(0, 2) == 1)
                {
                    world.Add(entity, new HitboxComponent());
                }
                
                preLast = lastEntity;
                lastEntity = entity;
            }

            foreach (var e in query)
            {
            
            }
        
            world.Delete(preLast);
            world.Delete(lastEntity);

            var min = random.Next(0, entities.Count - 1);
            var max = random.Next(min, entities.Count);
            for (int i = min; i < max; i++)
            {
                world.Delete(entities[i]);
            }
            
            //world.Delete(lastEntity);
        
            foreach (var e in query)
            {
            
            }
        }
        
        
    }
}