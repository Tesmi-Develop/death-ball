using System.Text.Json;

namespace Shared.ResourcesData.TiledTilesetParts;

public class Property
{
    public string Name { get; set; }
    public string Type { get; set; }
    public object Value { get; set; }

    public T? GetValue<T>()
    {
        return (Value is JsonElement element ? element : default).Deserialize<T>();
    }
}