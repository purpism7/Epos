using UnityEngine;

using Actor;

namespace Characters.Actions
{
    public class ChaseParam : ActionParam
    {
        public IActor TargetActor = null;
        public float Distance = 0;
        /// <summary>쫓는 쪽(대상 위치 - 내 위치). 크기는 상관없다. 다가가는 것은 전투 시뮬이 하고, 여기서는 그쪽을 보고 달리기만 튼다.</summary>
        public Vector2 FaceDirection = Vector2.zero;
    }

    public class Chase : Action<ChaseParam>
    {
        public override void Execute()
        {
            if (_actor == null)
                return;

            // 쫓는 쪽을 먼저 본 뒤 한 번만 재생한다.
            PlayAnimation(AnimationKey.Run, true, _actionParam?.FaceDirection);
        }

        public override void ChainUpdate()
        {
            return;
        }
    }
}