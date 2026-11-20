namespace Characters
{
    /// <summary>앞(화면 아래, 카메라 쪽) / 뒤(화면 위). 이름 표의 칸 번호로 쓰이므로 값을 바꾸지 않는다.</summary>
    public enum VerticalFacing
    {
        Front = 0,
        Back = 1,
    }

    /// <summary>화면 기준 좌우. Character 인스펙터에 직렬화되므로 값을 바꾸지 않는다.</summary>
    public enum HorizontalFacing
    {
        Right = 0,
        Left = 1,
    }

    /// <summary>
    /// 바라보는 방향(앞뒤 + 좌우)과 그 갱신 규칙. 표현 전용 값이며 유니티/스파인에 기대지 않는 순수 값이다.
    /// 키 입력(이동 축)과 AI(대상 위치 - 내 위치) 방향 벡터에 같은 규칙을 쓴다.
    /// </summary>
    public readonly struct Facing
    {
        // 이 길이 이하의 방향은 '입력 없음'으로 보고 아무것도 바꾸지 않는다(기존 Flip 문턱 0.01 과 같다).
        private const float MinDirectionLength = 0.01f;
        private const float MinDirectionSqrLength = MinDirectionLength * MinDirectionLength;

        // 한 축 성분이 방향 길이의 20%(축에서 약 11.5도)를 넘어야 그 축의 방향을 바꾼다.
        // - 키 입력은 성분이 0 / ±0.707 / ±1 뿐이라 영향이 없다. "아래 성분이 있으면 앞, 위 성분이 있으면 뒤" 규칙이 그대로 지켜진다.
        // - 정규화하지 않은 AI 방향(#2 자동 공격: 대상 - 나)에서 거의 수평인 대상을 볼 때
        //   앞뒤가 떨리며 애니메이션이 매번 처음부터 다시 도는 것을 막으려는 값이다. 키 입력 규칙과 어긋난다고 보고 지우지 말 것.
        private const float AxisDeadZoneRatio = 0.2f;
        private const float AxisDeadZoneRatioSqr = AxisDeadZoneRatio * AxisDeadZoneRatio;

        public VerticalFacing Vertical { get; }
        public HorizontalFacing Horizontal { get; }

        public Facing(VerticalFacing vertical, HorizontalFacing horizontal)
        {
            Vertical = vertical;
            Horizontal = horizontal;
        }

        /// <summary>
        /// (x, y) 방향(월드 XY, y 가 화면 위)을 바라보도록 돌린 새 값을 돌려준다. 크기는 상관없다.
        /// y &lt; 0 → Front, y &gt; 0 → Back / x &lt; 0 → Left, x &gt; 0 → Right.
        /// 문턱을 넘지 못한 축은 이전 값을 유지한다. 0·NaN·무한대·넘침은 '변화 없음'.
        /// </summary>
        public Facing TurnToward(float x, float y)
        {
            float sqrLength = x * x + y * y;

            // 0 이거나 NaN 이면 비교가 false 이므로 '!(>)' 로 함께 거른다.
            if (!(sqrLength > MinDirectionSqrLength))
                return this;

            float axisThresholdSqr = sqrLength * AxisDeadZoneRatioSqr;

            var vertical = Vertical;
            if (y * y > axisThresholdSqr)
                vertical = y < 0f ? VerticalFacing.Front : VerticalFacing.Back;

            var horizontal = Horizontal;
            if (x * x > axisThresholdSqr)
                horizontal = x < 0f ? HorizontalFacing.Left : HorizontalFacing.Right;

            return new Facing(vertical, horizontal);
        }
    }
}
