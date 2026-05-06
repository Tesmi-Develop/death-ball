using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hypercube.Physics.Shapes;
using Hypercube.Physics.Shapes.Structs;
using MessagePack;
using MessagePack.Formatters;

namespace Shared.MessagePackFormatters.Shapes;

public class ShapeUnionTypedFormatter : IMessagePackFormatter<ShapeUnionTyped>
{
    public void Serialize(ref MessagePackWriter writer, ShapeUnionTyped value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write((byte)value.Type);
        
        var size = Unsafe.SizeOf<ShapeUnion>();
        writer.WriteBinHeader(size);
        
        var span = MemoryMarshal.CreateReadOnlySpan(ref value.Shape, 1);
        writer.Write(MemoryMarshal.AsBytes(span));
    }

    public ShapeUnionTyped Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        reader.ReadArrayHeader();
        
        var type = (ShapeType)reader.ReadByte();
        
        var sequence = reader.ReadBytes();
        if (sequence is null)
            return default;
        
        var length = (int)sequence.Value.Length;
        Span<byte> bytes = stackalloc byte[length];
        
        sequence.Value.CopyTo(bytes);
        var shapeUnion = MemoryMarshal.Read<ShapeUnion>(bytes);

        return new ShapeUnionTyped
        {
            Type = type,
            Shape = shapeUnion
        };
    }
}