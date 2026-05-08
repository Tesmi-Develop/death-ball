using System.Text.Json.Serialization;

namespace Shared.ResourcesData.TiledTilesetParts;

public class TiledTileDefinition
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("properties")]
    public List<Property> Properties { get; set; } = [];
    
    public bool TryGetPropertyData(string name, out Property? result)
    {
        result = Properties.FirstOrDefault(p => p.Name == name);
        return result is not null;
    }
}