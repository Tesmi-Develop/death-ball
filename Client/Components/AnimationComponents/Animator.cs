using Hypercube.Ecs.Components;
using Shared.ResourcesData;

namespace Client.Components.AnimationComponents;

public struct Animator : IComponent
{
    public AnimationClip? CurrentClip;
    public bool IsPlaying;
    public bool IsPaused;
    public bool IsLooping;
    public int StartTick;
    public int PausedTicks;
}