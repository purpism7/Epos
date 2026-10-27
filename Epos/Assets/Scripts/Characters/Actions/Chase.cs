using Actor;

namespace Characters.Actions
{
    public class ChaseParam : ActionParam
    {
        public IActor TargetActor = null;
        public float Distance = 0;
    }

    public class Chase : Action<AttackParam>
    {
        public override void Execute()
        {
            if (_actor == null)
                return;

            PlayAnimation("Run", true);
        }

        public override void ChainUpdate()
        {
            return;
        }
    }
}