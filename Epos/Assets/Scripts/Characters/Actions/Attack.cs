namespace Characters.Actions
{
    public class AttackParam : ActionParam
    {
        
    }

    public class Attack : Action<AttackParam>
    {
        public override void Execute()
        {
            if (_actor == null)
                return;

            PlayAnimation("Skill_01", true);
        }

        public override void ChainUpdate()
        {
            return;
        }
    }
}