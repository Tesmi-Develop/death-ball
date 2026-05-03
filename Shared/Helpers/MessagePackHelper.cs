using MessagePack;
using MessagePack.Resolvers;
using Shared.MessagePackFormatters;

namespace Shared.Helpers;

public static class MessagePackHelper
{
    public static void SetupMessagePack()
    {
        var formatterResolver = StaticCompositeResolver.Instance;
        formatterResolver.Register(new Vector2Formatter());
        
        var resolver = CompositeResolver.Create(
            formatterResolver,
            ContractlessStandardResolver.Instance,
            StandardResolver.Instance
        );
        
        var options = MessagePackSerializerOptions.Standard
            .WithResolver(resolver)
            .WithSecurity(MessagePackSecurity.UntrustedData); 

        MessagePackSerializer.DefaultOptions = options;
    }
    
    // A quick way to write a long without a header when the packet structure is strictly defined
    public static void WriteInt64(Span<byte> span, long value)
    {
        span[0] = (byte)value;
        span[1] = (byte)(value >> 8);
        span[2] = (byte)(value >> 16);
        span[3] = (byte)(value >> 24);
        span[4] = (byte)(value >> 32);
        span[5] = (byte)(value >> 40);
        span[6] = (byte)(value >> 48);
        span[7] = (byte)(value >> 56);
    }
}