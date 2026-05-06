using System.Text.Json.Serialization;
using Hypercube.Core.Graphics.Resources;
using Hypercube.Core.Resources.Loaders;
using Shared.ResourcesData.TiledTilesetParts;

namespace Shared.ResourcesData;

public class TiledTileset : Resource
{
    public string Name { get; set; } = null!;
    public int Columns { get; set; }
    
    [JsonPropertyName("image")]
    public string ImagePath { get; set; } = null!;

    [JsonIgnore]
    public Texture? Texture { get; set; }
    
    [JsonPropertyName("tilecount")]
    public int TileCount { get; set; }
    
    [JsonPropertyName("tilewidth")]
    public int TileWidth { get; set; }
    
    [JsonPropertyName("tileheight")]
    public int TileHeight { get; set; }

    public List<TiledTileDefinition> Tiles { get; set; } = [];
    
    public override void Dispose()
    {
        Texture?.Dispose();
    }
}