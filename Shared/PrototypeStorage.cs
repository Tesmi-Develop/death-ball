using System.Text.Json;
using Hypercube.Core.Resources.Loaders;

namespace Shared;

using PrototypeContainer = Dictionary<string, Dictionary<string, Dictionary<string, object>>>;

public sealed class PrototypeStorage : Resource
{
    // [PrototypeName] -> [ComponentName] -> [PropertyName] -> [PropertyValue]
    private readonly PrototypeContainer _data;

    public static PrototypeStorage FromJson(string json)
    {
        var data = JsonSerializer.Deserialize<PrototypeContainer>(json);
        if (data is null)
            throw new NullReferenceException();
        
        return new PrototypeStorage(data);
    }

    private PrototypeStorage(PrototypeContainer container)
    {
        _data = container;
    }
    
    public bool TryGetPrototype(string protoName, out Dictionary<string, Dictionary<string, object>>? prototype)
    {
        prototype = null;
        return _data.TryGetValue(protoName, out prototype);
    }

    public override void Dispose()
    {
        
    }
}