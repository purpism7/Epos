using UnityEngine;

using Spine;

namespace Characters.Actions
{
    public class AttackParam : ActionParam
    {
        /// <summary>휘두르는 쪽(대상 위치 - 내 위치). 크기는 상관없다.</summary>
        public Vector2 FaceDirection = Vector2.zero;
    }

    public class Attack : Action<AttackParam>
    {
        /// <summary>공격 그림(Skill01)이 아직 끝나지 않았으면 true. 시뮬이 대기로 넘어가도 휘두르는 그림을 끝까지 보여 주려고 쓴다(ActionController.TryShowIdle).</summary>
        public bool IsSwingPlaying { get; private set; } = false;

        public override void Execute()
        {
            if (_actor == null)
                return;

            // 한 번만 휘두른다(반복 없음). 새 휘두르기마다 ActionController.ShowAttack 이 처음부터 다시 Execute 한다.
            // 피해가 들어가는 때는 그림(스파인 이벤트)이 아니라 시뮬 데이터(Attack Hit Sec)가 정한다.
            IsSwingPlaying = PlayAnimation(AnimationKey.Skill01, false, _actionParam?.FaceDirection);
        }

        public override void ChainUpdate()
        {
            return;
        }

        // 스파인이 이 휘두르기를 끝까지 재생했다. 다시 틀거나 다른 그림으로 바꾸면 이전 트랙의 Complete 는 오지 않는다(ClearTracks).
        protected override void OnCompleted(TrackEntry trackEntry)
        {
            IsSwingPlaying = false;
        }
    }
}