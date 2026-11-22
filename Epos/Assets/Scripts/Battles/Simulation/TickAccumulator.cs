using System;

namespace Battles.Simulation
{
    /// <summary>
    /// 프레임마다 흐른 시간(초)을 모아, 이번 프레임에 돌릴 고정 간격 틱 수로 바꾼다. 유니티 Time 을 모르는 순수 클래스(부르는 쪽이 프레임 시간을 넣는다).
    /// - 한 틱에 10% 모자라도 미리 돌리고 모자란 시간은 다음 프레임에 갚는다 → 60Hz 화면에서 프레임 시간이 조금 흔들려도
    ///   0틱 · 2틱이 번갈아 나오지 않는다(긴 시간의 합은 같다).
    /// - 한 번에 maxTicksPerAdvance 를 넘게 밀리면 넘는 시간은 버린다(멈칫 · 디버거 정지 뒤 몰아서 돌지 않고 잠깐 느려진다).
    /// - 보간은 하지 않는다.
    /// </summary>
    public sealed class TickAccumulator
    {
        private const double SnapToleranceRatio = 0.1d;

        private readonly double _tickSeconds = 0d;
        private readonly double _snapToleranceSeconds = 0d;
        private readonly int _maxTicksPerAdvance = 1;
        private double _pendingSeconds = 0d;

        public TickAccumulator(int ticksPerSecond, int maxTicksPerAdvance)
        {
            _tickSeconds = 1d / Math.Max(1, ticksPerSecond);
            _snapToleranceSeconds = _tickSeconds * SnapToleranceRatio;
            _maxTicksPerAdvance = Math.Max(1, maxTicksPerAdvance);
        }

        /// <returns>이번에 돌릴 틱 수(0 ~ maxTicksPerAdvance). 0 · 음수 · NaN · 무한대 시간은 0.</returns>
        public int Advance(float deltaSeconds)
        {
            if (!(deltaSeconds > 0f) || float.IsInfinity(deltaSeconds))
                return 0;

            _pendingSeconds += deltaSeconds;

            int tickCount = 0;
            while (tickCount < _maxTicksPerAdvance && _pendingSeconds + _snapToleranceSeconds >= _tickSeconds)
            {
                _pendingSeconds -= _tickSeconds;
                tickCount++;
            }

            // 따라잡기 한도에 걸려 아직 한 틱 넘게 남았으면 버린다.
            if (_pendingSeconds + _snapToleranceSeconds >= _tickSeconds)
                _pendingSeconds = 0d;

            return tickCount;
        }

        /// <summary>쌓인 시간을 비운다(전투를 다시 켤 때).</summary>
        public void Reset()
        {
            _pendingSeconds = 0d;
        }
    }
}
