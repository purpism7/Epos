using Common;
using Creature.Action;
using Datas.ScriptableObjects;
using System.Runtime.CompilerServices;
using UnityEngine;
using VContainer;

namespace Creature
{
    public class Combatant : ICombatant
    {
        public IActor IActor { get; private set; } = null;
        public ISkillController ISkillCtr { get; private set; } = null;
        public ETeam ETeam { get; private set; } = ETeam.None;

        public ICombatant Initialize(IActor iActor, Skill[] skills)
        {
            IActor = iActor;

            //WeakTypeMap<IActor>
            //Bind(iActor, this);

            InitializeSkillController(skills);

            return this;
        }

        void ICombatant.SetETeam(ETeam eTeam)
        {
            ETeam = eTeam;
        }

        void ICombatant.SetPosition(Vector3 position)
        {

        }

        private void InitializeSkillController(Skill[] skills)
        {
            ISkillCtr = new SkillController(skills);
            ISkillCtr?.Initialize(this);
        }
    }
}
