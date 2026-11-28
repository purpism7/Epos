using UnityEngine;

using Spine;
using Spine.Unity;

namespace Characters
{
    /// <summary>
    /// 캐릭터 한 명의 보이는 방향과 애니메이션 재생을 맡는 얇은 유니티/스파인 어댑터(표현 전용).
    /// - 좌우는 Skeleton.ScaleX 로, 앞뒤는 AnimationNameTable 이 고른 이름으로 나타낸다.
    /// - ScaleX 를 쓰는 곳은 여기 한 곳뿐이다(IActionController.Flip 도 여기로 온다).
    /// - 앞뒤가 바뀌면 반복 재생 중인 애니메이션을 새 방향 이름으로 다시 튼다
    ///   (ActionController 는 같은 액션이면 Execute 를 다시 부르지 않기 때문).
    /// - 시뮬레이션은 방향을 넣기만 하고 여기서 읽어 가지 않는다(바라보는 방향을 내주는 getter 를 두지 않는다).
    /// </summary>
    public sealed class CharacterAnimator
    {
        // 쓰러짐 그림이 없는 캐릭터가 전투 불능일 때 곱하는 색(⚑ 임시 표현 — 어둡고 조금 비친다).
        private const float DownTintBrightness = 0.4f;
        private const float DownTintAlpha = 0.75f;

        private readonly SkeletonAnimation _skeletonAnimation = null;
        private readonly HorizontalFacing _artFacing = HorizontalFacing.Right;
        private readonly bool[] _warnedKeys = new bool[AnimationNameTable.KeyCount];

        // 첫 재생 때 스켈레톤 데이터로 한 번 만든다(Character.Initialize 에서 스파인을 미리 깨우지 않으려고).
        private AnimationNameTable _nameTable = null;
        private Facing _facing = default;

        // 마지막 재생 요청. 앞뒤가 바뀌면 이것을 새 방향 이름으로 다시 튼다.
        private bool _hasRequest = false;
        private AnimationKey _requestedKey = AnimationKey.Idle;
        private bool _requestedLoop = false;
        private System.Action<TrackEntry> _requestedCompleted = null;
        private string _playingName = null;

        // 쓰러짐 그림이 없을 때의 멈춤 · 어둡게(HoldDownWithoutAnimation). 다시 하기 때 원래 값으로 돌린다.
        private bool _isHeldDown = false;
        private float _timeScaleBeforeHold = 1f;
        private Color _colorBeforeHold = Color.white;

        /// <param name="skeletonAnimation">null 이어도 된다(재생은 false, 좌우 반전은 건너뛴다).</param>
        /// <param name="artFacing">스파인 원본 그림이 뒤집지 않은 상태(ScaleX = +1)에서 바라보는 좌우 방향.</param>
        public CharacterAnimator(SkeletonAnimation skeletonAnimation, HorizontalFacing artFacing)
        {
            _skeletonAnimation = skeletonAnimation;
            _artFacing = artFacing;

            // 처음엔 그림이 그려진 쪽을 본다. ScaleX = +1(스파인 기본값)과 같으므로 여기서 스켈레톤을 건드리지 않는다.
            _facing = new Facing(VerticalFacing.Front, artFacing);
        }

        /// <summary>
        /// 방향 벡터(이동 축, 또는 대상 위치 - 내 위치)를 바라본다. 크기는 상관없다. 규칙은 Facing.TurnToward.
        /// 앞뒤가 바뀌면 반복 재생 중인 애니메이션을 새 방향 이름으로 다시 튼다(이름이 같으면 그대로 둔다).
        /// </summary>
        public void Face(Vector2 direction)
        {
            if (TurnToward(direction))
                ReplayInCurrentFacing();
        }

        /// <summary>
        /// 키로 애니메이션을 처음부터 재생한다. 지금 앞뒤에 맞는 이름을 고른다.
        /// faceDirection 을 주면 먼저 그쪽을 본 뒤 한 번만 재생한다(이전 애니메이션을 새 방향으로 헛되이 다시 틀지 않는다).
        /// </summary>
        public bool Play(AnimationKey key, bool loop, System.Action<TrackEntry> completed, Vector2? faceDirection = null)
        {
            if (faceDirection.HasValue)
                TurnToward(faceDirection.Value);

            _hasRequest = true;
            _requestedKey = key;
            _requestedLoop = loop;
            _requestedCompleted = completed;

            return PlayRequested();
        }

        /// <summary>
        /// 쓰러짐(Death) 그림이 없는 캐릭터의 전투 불능 표현(⚑ 임시): 지금 모습에서 애니메이션을 멈추고 어둡게 한다.
        /// 한 판 안에서는 되돌리지 않는다(전투 불능은 되돌릴 수 없다) — 그 뒤의 Play · Face 도 멈춘 채로 둔다. 다시 하기 때만 ClearDownHold 로 걷는다.
        /// </summary>
        public void HoldDownWithoutAnimation()
        {
            if (_isHeldDown || _skeletonAnimation == null || !_skeletonAnimation.IsValid)
                return;

            var animationState = _skeletonAnimation.AnimationState;
            var skeleton = _skeletonAnimation.skeleton;

            _isHeldDown = true;
            _timeScaleBeforeHold = animationState != null ? animationState.TimeScale : 1f;
            _colorBeforeHold = skeleton != null ? skeleton.GetColor() : Color.white;

            if (animationState != null)
                animationState.TimeScale = 0f;

            skeleton?.SetColor(DownTintBrightness, DownTintBrightness, DownTintBrightness, DownTintAlpha);
        }

        /// <summary>다시 하기: HoldDownWithoutAnimation 의 멈춤 · 어둡게를 걷고 원래대로 돌린다. 쓰러짐 그림 자체는 다음 Play 가 갈음한다.</summary>
        public void ClearDownHold()
        {
            if (!_isHeldDown)
                return;

            _isHeldDown = false;

            if (_skeletonAnimation == null || !_skeletonAnimation.IsValid)
                return;

            var animationState = _skeletonAnimation.AnimationState;
            if (animationState != null)
                animationState.TimeScale = _timeScaleBeforeHold;

            _skeletonAnimation.skeleton?.SetColor(_colorBeforeHold);
        }

        // 방향을 갱신하고 좌우 반전을 반영한다. 앞뒤가 바뀌었으면 true.
        private bool TurnToward(Vector2 direction)
        {
            var previousVertical = _facing.Vertical;
            _facing = _facing.TurnToward(direction.x, direction.y);

            ApplyHorizontalFacing();

            return _facing.Vertical != previousVertical;
        }

        private void ReplayInCurrentFacing()
        {
            // 한 번만 도는 애니메이션(공격·쓰러짐)은 도중에도, 끝난 뒤에도 다시 틀지 않는다. 새 방향은 다음 재생부터.
            if (!_hasRequest || !_requestedLoop)
                return;

            // 앞뒤 이름이 같으면(옛 Knight_01 의 "Run" 등) 다시 틀지 않는다 → 끊기지 않고 지금과 똑같다.
            if (_nameTable != null &&
                string.Equals(_nameTable.GetName(_requestedKey, _facing.Vertical), _playingName, System.StringComparison.Ordinal))
                return;

            PlayRequested();
        }

        private bool PlayRequested()
        {
            _playingName = null;

            if (_skeletonAnimation == null || !TryGetNameTable(out var nameTable))
                return false;

            var vertical = _facing.Vertical;
            var animationName = nameTable.GetName(_requestedKey, vertical);
            WarnOnceIfFallback(nameTable, _requestedKey, vertical, animationName);

            if (!_skeletonAnimation.PlayAnimation(animationName, _requestedLoop, _requestedCompleted, out _))
                return false;

            _playingName = animationName;

            // 재생이 스켈레톤을 막 초기화했을 수도 있으니 좌우를 한 번 더 맞춘다.
            ApplyHorizontalFacing();

            return true;
        }

        private bool TryGetNameTable(out AnimationNameTable nameTable)
        {
            if (_nameTable == null)
            {
                // 공용 PlayAnimation(ResolveAnimationName)이 이름을 찾는 것과 같은 데이터에서 찾는다.
                var skeletonData = _skeletonAnimation.AnimationState?.Data?.SkeletonData;
                if (skeletonData != null)
                    _nameTable = new AnimationNameTable(animationName => skeletonData.FindAnimation(animationName) != null);
            }

            nameTable = _nameTable;
            return nameTable != null;
        }

        private void ApplyHorizontalFacing()
        {
            // Skeleton 게터는 부를 때마다 Initialize(false) 를 부른다. 데이터가 깨진 에셋이면 매 프레임 다시 읽고 에러를 찍으므로,
            // 초기화가 끝난(IsValid) 경우에만 쓴다.
            if (_skeletonAnimation == null || !_skeletonAnimation.IsValid)
                return;

            var skeleton = _skeletonAnimation.skeleton;
            if (skeleton == null)
                return;

            // 원본 그림 방향과 같으면 그대로(+1), 다르면 뒤집는다(-1). 기존 Flip 처럼 크기는 ±1.
            skeleton.ScaleX = _facing.Horizontal == _artFacing ? 1f : -1f;
        }

        // 정확한 이름(그 방향 이름·방향 없는 이름)이 없어 대신한 이름을 쓸 때 키마다 한 번만 알린다.
        // 4.2 로 다시 내보내며 이름이 바뀌면 공용 부분 일치가 엉뚱한 애니메이션(예: "Idle" → "Back/B_Idle")을 고를 수 있어서다.
        private void WarnOnceIfFallback(AnimationNameTable nameTable, AnimationKey key, VerticalFacing vertical, string animationName)
        {
            if (!nameTable.IsFallback(key, vertical))
                return;

            int keyIndex = (int)key;
            if ((uint)keyIndex >= (uint)_warnedKeys.Length || _warnedKeys[keyIndex])
                return;

            _warnedKeys[keyIndex] = true;

            Debug.LogWarning($"[CharacterAnimator] {_skeletonAnimation.name}: {key}({vertical})에 정확히 맞는 애니메이션이 없어 '{animationName}'(으)로 대신합니다. 스파인 애니메이션 이름을 확인하세요.", _skeletonAnimation);
        }
    }
}
