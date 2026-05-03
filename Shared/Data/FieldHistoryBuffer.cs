namespace Shared.Data;

public struct FieldHistoryBuffer
{
    public byte[] Buffer;
    public int Capacity;
    public int FieldSize;
    public int FieldOffset;
    public ComparisonStrategy Strategy;
    
    public string FieldName;
}
