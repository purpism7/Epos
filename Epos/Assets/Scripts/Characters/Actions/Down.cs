namespace Characters.Actions
{
    public class DownParam : ActionParam
    {

    }

    /// <summary>
    /// 전투 불능(HP 0 · 사망 아님)의 그림. 시뮬에서 전투 불능은 되돌릴 수 없으므로 한 번 튼 뒤 다른 그림으로 바꾸지 않는다.
    /// - 쓰러짐(Death) 그림이 있으면 한 번만 틀고, 끝나면 스파인이 마지막 모습을 그대로 둔다(반복 없음).
    /// - 그림이 없는 캐릭터(옛 몬스터)는 대기 그림의 첫 모습에서 멈추고 어둡게 한다(⚑ 임시 표현 · 휘두르던 도중에 굳지 않게).
    /// </summary>
    public class Down : Action<DownParam>
    {
        public override void Execute()
        {
            if (_actor == null)
                return;

            if (PlayAnimation(AnimationKey.Death, false))
                return;

            PlayAnimation(AnimationKey.Idle, false);
            _actor.CharacterAnimator?.HoldDownWithoutAnimation();
        }

        public override void ChainUpdate()
        {
            return;
        }
    }
}
