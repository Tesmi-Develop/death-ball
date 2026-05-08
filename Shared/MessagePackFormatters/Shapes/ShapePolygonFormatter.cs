using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Shapes.Structs;
using Hypercube.Utilities.Collections;
using MessagePack;
using MessagePack.Formatters;

namespace Shared.MessagePackFormatters.Shapes;

public sealed class ShapePolygonFormatter : IMessagePackFormatter<ShapePolygon>
{
    public void Serialize(ref MessagePackWriter writer, ShapePolygon value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(5);
        options.Resolver.GetFormatterWithVerify<FixedArray8<Vector2>>().Serialize(ref writer, value.Vertices, options);
        options.Resolver.GetFormatterWithVerify<FixedArray8<Vector2>>().Serialize(ref writer, value.Normals, options);
        options.Resolver.GetFormatterWithVerify<Vector2>().Serialize(ref writer, value.Centroid, options);
        writer.Write(value.Count);
        writer.Write(value.Radius);
    }

    public ShapePolygon Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil) return default;
        reader.ReadArrayHeader();

        return new ShapePolygon
        {
            Vertices = options.Resolver.GetFormatterWithVerify<FixedArray8<Vector2>>().Deserialize(ref reader, options),
            Normals = options.Resolver.GetFormatterWithVerify<FixedArray8<Vector2>>().Deserialize(ref reader, options),
            Centroid = options.Resolver.GetFormatterWithVerify<Vector2>().Deserialize(ref reader, options),
            Count = reader.ReadInt32(),
            Radius = reader.ReadSingle()
        };
    }
}