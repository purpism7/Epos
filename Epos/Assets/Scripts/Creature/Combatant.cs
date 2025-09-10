using UnityEngine;

using VContainer;

using Common;
using Datas.ScriptableObjects;
using Creature.Action;

namespace Creature
{
    public class Combatant : ICombatant
    {
        [Inject] private IObjectResolver _iResolver = null;

        public IActor IActor { get; private set; } = null;
        public ISkillController ISkillCtr { get; private set; } = null;

        public Transform Transform { get { return IActor?.Transform; } }
        public IStat IStat { get { return IActor?.IStat; } }

        public ETeam ETeam { get; private set; } = ETeam.None;

        [Inject]
        private void InitializeInject(IObjectResolver iResolver)
        {
            Debug.Log("InitializeInject = " + iResolver);
        }

        public ICombatant Initialize(IActor iActor, Skill[] skills)
        {
            IActor = iActor;

            InitializeSkillController(skills);

            return this;
        }

        void ICombatant.SetETeam(ETeam eTeam)
        {
            ETeam = eTeam;
        }

        void ICombatant.SetPosition(Vector3 position)
        {
            Transform.position = position;
        }

        private void InitializeSkillController(Skill[] skills)
        {
            ISkillCtr = new SkillController(skills);
            _iResolver?.Inject(ISkillCtr);

            ISkillCtr?.Initialize(this);
        }
    }
}
