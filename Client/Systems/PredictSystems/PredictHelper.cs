using System.Reflection;
using Hypercube.Core.Ecs;
using Hypercube.Ecs;
using Hypercube.Ecs.Components;
using Shared.Components;
using Shared.Helpers;

namespace Client.Systems.PredictSystems;

public class PredictHelper : EntitySystem
{
    public const int Capacity = 60;
    
    public void PredictField<T>(Entity entity, string fieldName) where T : unmanaged, IComponent
    {
        if (!World.Has<EntityHistory>(entity))
            World.Add(entity, new EntityHistory());
    
        ref var data = ref World.Get<EntityHistory>(entity);
        var componentId = NumeratorGenerator.GetId(typeof(T));
    
        if (!data.Buffers.TryGetValue(componentId, out var fields))
        {
            fields = [];
            data.Buffers.Add(componentId, fields);
        }
        
        var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        if (fieldInfo is null)
            throw new ArgumentException($"Field '{fieldName}' not found in component {typeof(T).Name}");
        
        var buffer = FieldHistoryHelper.CreateFieldBuffer<T>(fieldInfo, Capacity); 
        fields.Add(buffer);
    }
}