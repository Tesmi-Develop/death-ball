namespace Client.Extensions;

public static class QueueExtensions
{
    public static bool TryPeekNext<T>(this Queue<T> queue, out T next)
    {
        if (queue.Count < 2)
        {
            next = default!;
            return false;
        }
        
        using var enumerator = queue.GetEnumerator();
        enumerator.MoveNext();
        if (enumerator.MoveNext())
        {
            next = enumerator.Current;
            return true;
        }

        next = default!;
        return false;
    }
}