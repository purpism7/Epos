using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Ability;

namespace Creature.Action
{
    public class Casting : Act<Casting.Data>
    {
        public class Data : BaseData
        {
            public IListener IListener = null;
            public Skill Skill = null;
            public List<ICombatant> TargetList = null;
        }

        public interface IListener
        {
            void BeforeCasting();
            void InUse();
            void AfterCasting();
        }

        public override void Initialize(IActor iActor)
        {
            base.Initialize(iActor);
        }

        public override void Execute()
        {
            if (_data == null)
                return;

            // _iSkillCtr?.Casting(_data.TargetList);
        }
    }
}

