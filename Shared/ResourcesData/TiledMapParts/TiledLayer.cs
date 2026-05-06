using Hypercube.Mathematics.Vectors;

namespace Shared.Resources.TiledMapParts;

public class TiledLayer
{
    public int[] Data { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public bool Visible { get; set; }

    public int GetTileAt(Vector2i point)
    {
        if (point.X < 0 || point.X >= Width || point.Y < 0 || point.Y >= Height)
            return 0;

        return Data[point.Y * Width + point.X];
    }
}