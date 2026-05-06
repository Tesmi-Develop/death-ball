using System.Text.Json.Serialization;
using Hypercube.Core.Resources.Loaders;
using Shared.Resources.TiledMapParts;
using Shared.ResourcesData.TiledMapParts;

namespace Shared.Resources;

public class TiledMap : Resource
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    [JsonPropertyName("tilewidth")]
    public int TileWidth { get; set; }
    
    [JsonPropertyName("tileheight")]
    public int TileHeight { get; set; }

    public List<TiledLayer> Layers { get; set; }
    public List<TiledTilesetReference> Tilesets { get; set; }
    
    public override void Dispose()
    {
        foreach (var tileset in Tilesets)
        {
            tileset.Dispose();
        }
    }
}