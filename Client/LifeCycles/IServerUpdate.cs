namespace Client.LifeCycles;

public interface IServerUpdate
{
    void ServerUpdate(long serverTick, long predictTick);
}