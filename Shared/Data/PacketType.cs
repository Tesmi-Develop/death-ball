namespace Shared.Data;

public enum PacketType : byte
{
    Ping,
    Handshake,
    Hydrate,
    Dirty,
    EntitiesDeletion,
    ComponentsDeletion,
    ComponentsAddition,
    Request,
}