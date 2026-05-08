using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Collections;
using MessagePack;
using MessagePack.Formatters;

namespace Shared.MessagePackFormatters;

public sealed class FixedArray8Vector2Formatter : IMessagePackFormatter<FixedArray8<Vector2>>
{
    private const int Counts = 8;
    public void Serialize(ref MessagePackWriter writer, FixedArray8<Vector2> value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(Counts);
        for (var i = 0; i < Counts; i++)
            options.Resolver.GetFormatterWithVerify<Vector2>().Serialize(ref writer, value[i], options);
    }

    public FixedArray8<Vector2> Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.IsNil) return default;
        reader.ReadArrayHeader();

        var array = new FixedArray8<Vector2>();
        
        for (var i = 0; i < Counts; i++)
            array[i] = options.Resolver.GetFormatterWithVerify<Vector2>().Deserialize(ref reader, options);

        return array;
    }
}