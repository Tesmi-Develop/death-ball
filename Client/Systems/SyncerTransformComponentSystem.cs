using Client.Components;
using Client.Extensions;
using Hypercube.Core.Ecs;
using Hypercube.Core.Execution.LifeCycle;
using Hypercube.Core.Systems.Rendering;
using Hypercube.Core.Systems.Transform;
using Hypercube.Ecs.Queries;
using Hypercube.Mathematics;
using Hypercube.Mathematics.Vectors;
using Hypercube.Utilities.Dependencies;
using Shared.Components;

namespace Client.Systems;

public class SyncerTransformComponentSystem : EntitySystem
{
    [Dependency] private readonly GameClient _client = null!;
    private double _visualTickCursor = -1;

    private Query _query = null!;

    public override void Initialize()
    {
        _query = GetQuery()
            .WithAll<NetworkTransform, TransformComponent, InterpolationComponent, SpriteComponent>()
            .Build();
    }

    public override void Update(FrameEventArgs deltaTime)
    {
        const double bufferTicks = 3.0; // Твой буфер
    
        // 1. Целевой тик, к которому мы хотим прийти (с учетом твоего "прыгающего" офсета)
        double targetTick = _client.GetServerTickDouble() - bufferTicks;

        // 2. Инициализация при первом запуске
        if (_visualTickCursor < 0)
            _visualTickCursor = targetTick;

        // 3. Плавное дотягивание (Visual Smoothing)
        // Мы не трогаем системный офсет, мы просто заставляем визуальный курсор 
        // медленно дрейфовать в сторону целевого тика.
        // Коэффициент 5.0 означает, что за 1 секунду мы покроем большую часть разрыва.
        var lerpFactor = deltaTime.Delta.Milliseconds * 5.0f; 
        _visualTickCursor = double.Lerp(_visualTickCursor, targetTick, lerpFactor);

        // 4. Анти-джиттер: Время для глаз никогда не должно идти назад
        // Даже если офсет сильно прыгнул назад, курсор просто замедлится или замрет.
        if (_visualTickCursor > targetTick + 10) // Если ушли вперед слишком сильно (дикий лаг)
            _visualTickCursor = targetTick;
         
        var renderTick = _visualTickCursor;

        _query.With<NetworkTransform, TransformComponent, InterpolationComponent>(
            (entity, ref net, ref transform, ref interp) =>
            {
                var snapshots = interp.Snapshots;

                // 1. Очистка: выкидываем старые тики, оставляя один "прошлый" для интерполяции
                // Нам нужно, чтобы snapshots.Peek().Tick был меньше или равен renderTick,
                // а следующий за ним тик был больше renderTick.
                while (snapshots.TryPeekNext(out var next) && next.Tick <= renderTick)
                {
                    snapshots.Dequeue();
                }

                // 2. Проверка данных
                if (snapshots.Count < 2)
                {
                    // Если остался только один снимок (новые еще не дошли), 
                    // просто ставим объект в последнюю известную позицию.
                    if (snapshots.Count == 1)
                    {
                        var last = snapshots.Peek();
                        transform.LocalPosition = new Vector3(last.Position.X, last.Position.Y, transform.LocalPosition.Z);
                    }
                    return;
                }

                // 3. Берем две точки для интерполяции
                var from = snapshots.Peek();
                snapshots.TryPeekNext(out var to);

                // 4. Вычисляем фактор интерполяции (t)
                // Все расчеты теперь на тиках, что дает идеальную точность
                double tickDelta = to.Tick - from.Tick;
                if (tickDelta <= 0) return;

                float t = (float)((renderTick - from.Tick) / tickDelta);
                t = Math.Clamp(t, 0f, 1f);

                // 5. Применяем позицию
                var targetPos = Vector2.Lerp(from.Position, to.Position, t);
                
                Console.WriteLine(t);
                transform.LocalPosition = new Vector3(
                    targetPos.X, 
                    targetPos.Y, 
                    transform.LocalPosition.Z
                );
            });
    }
}