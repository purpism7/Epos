namespace Characters.Actions
{
    public class IdleParam : ActionParam
    {
        
    }

    public class Idle : Action<IdleParam>
    {
        public override void Execute()
        {
            if (_actor == null)
                return;

            PlayAnimation("Idle", true);
        }

        public override void ChainUpdate()
        {
            return;
        }
    }
}