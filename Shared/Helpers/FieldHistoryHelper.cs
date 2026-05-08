using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hypercube.Mathematics.Extensions;
using Shared.Data;

namespace Shared.Helpers;

public static class FieldHistoryHelper
{
    public static FieldHistoryBuffer CreateFieldBuffer<TComponent, TField>(string fieldName, int capacity) 
        where TComponent : struct
        where TField : unmanaged
    {
        var offset = (int)Marshal.OffsetOf<TComponent>(fieldName);
        var size = Unsafe.SizeOf<TField>();
        
        return new FieldHistoryBuffer
        {
            Buffer = new byte[capacity * size],
            Capacity = capacity,
            FieldSize = size,
            FieldOffset = offset,
            Strategy = ComparisonStrategyHelper.GetStrategy(typeof(TField)),
            FieldName = fieldName
        };
    }

    public static FieldHistoryBuffer CreateFieldBuffer<TComponent>(FieldInfo fieldInfo, int capacity)
        where TComponent : struct
    {
        var offset = (int)Marshal.OffsetOf<TComponent>(fieldInfo.Name);
        var size = Marshal.SizeOf(fieldInfo.FieldType);
        
        return new FieldHistoryBuffer
        {
            Buffer = new byte[capacity * size],
            Capacity = capacity,
            FieldSize = size,
            FieldOffset = offset,
            Strategy = ComparisonStrategyHelper.GetStrategy(fieldInfo.FieldType),
            FieldName = fieldInfo.Name
        };
    }
    
    public static void WriteField<TComponent>(
        ref FieldHistoryBuffer history, 
        long tick, 
        ref TComponent component) 
        where TComponent : struct
    {
        var compSpan = MemoryMarshal.CreateReadOnlySpan(ref component, 1);
        var compBytes = MemoryMarshal.AsBytes(compSpan);
        var fieldData = compBytes.Slice(history.FieldOffset, history.FieldSize);
        var bufferIndex = (int)(tick % history.Capacity);
        var bufferOffset = bufferIndex * history.FieldSize;
        var destination = history.Buffer.AsSpan(bufferOffset, history.FieldSize);
        
        fieldData.CopyTo(destination);
    }
    
    public static TField ReadField<TField>(ref FieldHistoryBuffer history, long tick) 
        where TField : unmanaged
    {
        var bufferIndex = (int)(tick % history.Capacity);
        var bufferOffset = bufferIndex * history.FieldSize;
        var source = history.Buffer.AsSpan(bufferOffset, history.FieldSize);
        
        return MemoryMarshal.Read<TField>(source);
    }
    
    public static bool IsFieldValid<TComponent>(
        ref FieldHistoryBuffer history, 
        long tick, 
        ref TComponent serverComponent,
        float eps = 1e-6f) 
        where TComponent : struct
    {
        var bufferIndex = (int)(tick % history.Capacity);
        var bufferOffset = bufferIndex * history.FieldSize;
    
        ReadOnlySpan<byte> predictedBytes = history.Buffer.AsSpan(bufferOffset, history.FieldSize);
        var compBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref serverComponent, 1));
        var serverFieldBytes = compBytes.Slice(history.FieldOffset, history.FieldSize);

        var a = history.Strategy switch
        {
            ComparisonStrategy.RelaxedFloat => 
                Math.Abs(MemoryMarshal.Read<float>(predictedBytes) - MemoryMarshal.Read<float>(serverFieldBytes)) <= eps,

            ComparisonStrategy.RelaxedVector3 => 
                CompareVector3(predictedBytes, serverFieldBytes, eps),
            
            ComparisonStrategy.RelaxedVector2 => 
                CompareVector2(predictedBytes, serverFieldBytes, eps),

            ComparisonStrategy.RelaxedDouble => 
                Math.Abs(MemoryMarshal.Read<double>(predictedBytes) - MemoryMarshal.Read<double>(serverFieldBytes)) <= eps,
            
            _ => predictedBytes.SequenceEqual(serverFieldBytes)
        };

        if (!a)
        {
            return false;
        }

        return a;
    }

    private static bool CompareVector3(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, float eps)
    {
        var vA = MemoryMarshal.Cast<byte, float>(a);
        var vB = MemoryMarshal.Cast<byte, float>(b);

        return vA[0].AboutEquals(vB[0], eps) &&
               vA[1].AboutEquals(vB[1], eps) &&
               vA[2].AboutEquals(vB[2], eps);
    }
    
    private static bool CompareVector2(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b, float eps)
    {
        var vA = MemoryMarshal.Cast<byte, float>(a);
        var vB = MemoryMarshal.Cast<byte, float>(b);

        return vA[0].AboutEquals(vB[0], eps) &&
               vA[1].AboutEquals(vB[1], eps);
    }
}