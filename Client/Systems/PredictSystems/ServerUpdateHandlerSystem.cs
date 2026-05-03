using Client.LifeCycles;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Ecs;
using Hypercube.Ecs.Queries;
using Hypercube.Utilities.Debugging.Logger;
using Hypercube.Utilities.Dependencies;
using Shared.Components;

namespace Client.Systems.PredictSystems;

public class ServerUpdateHandlerSystem : EntitySystem
{
    [Dependency] private readonly GameClient _gameClient = null!;
    [Dependency] private readonly InputStorage _inputStorage = null!;
    [Dependency] private readonly IRuntimeLoop _runtimeLoop = null!;
    [Dependency] private readonly FieldHistorySystem _fieldHistorySystem = null!;
    [Dependency] private readonly IDependenciesContainer _dependenciesContainer = null!;
    [Dependency] private readonly ILogger _logger = null!;

    private Query _query = null!;
    private readonly List<IServerUpdate> _serverUpdates = [];
    private bool _needRollback;
    private long _lastTick;
    private long _lastPredictTick;
    private long _missPredictTick = long.MaxValue;
    public long PredictTick { get; private set; } = 0;
    public bool IsRollback { get; private set; } = false;

    public override void Initialize()
    {
        _query = GetQuery().WithAll<EntityHistory>().Build();
        
        var interfaceType = typeof(IServerUpdate);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => interfaceType.IsAssignableFrom(p) && p is { IsClass: true, IsAbstract: false });

        foreach (var type in types)
        {
            var instance = _dependenciesContainer.Resolve(type);
            if (instance is IServerUpdate serverUpdate)
                _serverUpdates.Add(serverUpdate);
        }
        
        _runtimeLoop.Actions.Add(OnUpdate, EngineUpdatePriority.RendererUpdate);
    }

    private void OnUpdate(FrameEventArgs args)
    {
        if (!_gameClient.Connected)
            return;
        
        var serverTick = _gameClient.GetServerTick();
        
        if (_needRollback)
        {
            StartRollback();
            return;
        }
        
        while (_lastTick < serverTick)
        {
            _lastTick++;
            PredictTick = _gameClient.GetPredictServerTick(_lastTick);
            
            _inputStorage.CaptureActualInputs(PredictTick);
            InvokeServerUpdate(_lastTick, PredictTick);
            _fieldHistorySystem.WriteEntitiesHistory(PredictTick);
        }
    }

    private void StartRollback()
    {
        _logger.Trace($"Got miss predict {_missPredictTick}");
        IsRollback = true;
        ProcessRollback(PredictTick);
        IsRollback = false;
        _logger.Trace($"Got rollback {_lastTick}");
    }

    private void ProcessRollback(long currentTick)
    {
        if (currentTick < _missPredictTick)
        {
            _logger.Warning("An attempt to reproduce the future. How?");
            _needRollback = false;
            _missPredictTick = long.MaxValue;
            return;
        }
        
        for (var tick = _missPredictTick + 1; tick <= currentTick; tick++)
        {
            _inputStorage.SetMockInput(tick);
            InvokeServerUpdate(tick, -1);
            _fieldHistorySystem.WriteEntitiesHistory(tick);
        }
        
        _needRollback = false;
        _missPredictTick = long.MaxValue;
    }

    private void InvokeServerUpdate(long tick, long predictTick)
    {
        foreach (var instance in _serverUpdates)
        {
            instance.ServerUpdate(tick, predictTick);
        }
    }

    public void ReconcileState(Entity entity)
    {
        if (!World.Has<EntityHistory>(entity))
            return;
        
        ref var history = ref World.Get<EntityHistory>(entity);
        if (!history.NeedsRollback)
            return;
        
        _needRollback = true;
        _missPredictTick = Math.Min(history.RollbackTick, _missPredictTick);
        history.NeedsRollback = false;
        history.RollbackTick = 0;
        StartRollback();
    }

    public override void Update(FrameEventArgs args)
    {
        _query.With<EntityHistory>((_, ref history) =>
        {
            if (!history.NeedsRollback) 
                return;
            
            _needRollback = true;
            _missPredictTick = Math.Min(history.RollbackTick, _missPredictTick);
            history.NeedsRollback = false;
            history.RollbackTick = 0;
        });
    }
}