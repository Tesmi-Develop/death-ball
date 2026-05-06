using Hypercube.Mathematics.Vectors;
using Hypercube.Physics.Shapes.Structs;
using MessagePack;
using MessagePack.Formatters;

namespace Shared.MessagePackFormatters.Shapes;

public sealed class ShapeCircleFormatter : IMessagePackFormatter<ShapeCircle>
{
    public void Serialize(ref MessagePackWriter writer, ShapeCircle value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        options.Resolver.GetFormatterWithVerify<Vector2>().Serialize(ref writer, value.Center, options);
        writer.Write(value.Radius);
    }

    public ShapeCircle Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil) return default;
        reader.ReadArrayHeader();
        
        return new ShapeCircle
        {
            Center = options.Resolver.GetFormatterWithVerify<Vector2>().Deserialize(ref reader, options),
            Radius = reader.ReadSingle()
        };
    }
}