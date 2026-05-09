using Hypercube.Ecs;

namespace Server.Extensions;

public static class EntityExtensions
{
    public static long GetFullMask(this Entity entity)
    {
        return (long)entity.Version << 4 | entity.Id;
    }
}