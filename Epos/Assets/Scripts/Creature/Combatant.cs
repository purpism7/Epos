using UnityEngine;

using Common;
using Creature.Action;

namespace Creature
{
    public class Combatant : ICombatant
    {
        

        // public Transform Transform => _iActor?.Transform;

        public IActor IActor { get; private set; } = null;
        // public IActController IActCtr => _iActor?.IActCtr;
        public ISkillController ISkillCtr { get; private set; } = null;
        public ETeam ETeam { get; private set; } = ETeam.None;

        public Combatant(IActor iActor)
        {
            IActor = iActor;
        }

        void ICombatant.SetETeam(ETeam eTeam)
        {

        }

        void ICombatant.SetPosition(Vector3 position)
        {

        }

        private void InitializeSkillController()
        {
            ISkillCtr = new SkillController();
            // ISkillCtr?.Initialize(iCaster);
        }
    }
}
