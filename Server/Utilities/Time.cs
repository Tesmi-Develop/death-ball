using System.Diagnostics;

namespace Server.Utilities;

public class Time
{
    public long Tick = 0;
    public readonly Stopwatch Stopwatch = new ();
}