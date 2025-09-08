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

        private readonly ConditionalWeakTable<IActor, ICombatant> _map = new();

        private void Bind(IActor actor, ICombatant combatant) => _map.Add(actor, combatant);
        public bool TryGet(IActor actor, out ICombatant c) => _map.TryGetValue(actor, out c);

        public ICombatant Initialize(IActor iActor, Skill[] skills)
        {
            IActor = iActor;
            Bind(iActor, this);

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
