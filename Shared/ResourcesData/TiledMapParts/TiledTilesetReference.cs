using System.Text.Json.Serialization;

namespace Shared.ResourcesData.TiledMapParts;

public class TiledTilesetReference : IDisposable
{
    public int FirstGid { get; set; }
    
    [JsonPropertyName("source")]
    public string Path { get; set; }
    
    [JsonIgnore]
    public TiledTileset? Source { get; set; }

    public void Dispose()
    {
        Source?.Dispose();
    }
}