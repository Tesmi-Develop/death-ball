namespace Shared.Data;

public enum PacketType : byte
{
    Ping,
    TimeHydrate,
    Hydrate,
    Dirty,
    EntitiesDeletion,
    ComponentsDeletion,
    ComponentsAddition,
    Request,
}