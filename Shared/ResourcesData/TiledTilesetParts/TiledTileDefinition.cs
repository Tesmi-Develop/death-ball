namespace Shared.ResourcesData.TiledTilesetParts;

public class TiledTileDefinition
{
    public int Id { get; set; }
    public List<TiledProperty> Properties { get; set; } = [];
    
    public bool TryGetPropertyData(string name, out TiledProperty? result)
    {
        result = Properties.FirstOrDefault(p => p.Name == name);
        return result is not null;
    }
}