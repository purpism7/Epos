using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Creature.Action
{
    public class Skill : Act<Skill.Data>
    {
        public class Data : BaseData
        {
            public IListener IListener = null;
        }

        public interface IListener
        {
            void BeforeUse();
            void InUse();
            void AfterUse();
        }

        private ISkillController _iSkillCtr = null;

        public override void Initialize(IActor iActor)
        {
            base.Initialize(iActor);

            var iActorTm = _iActor?.Transform;
            if (!iActorTm)
                return;
            
            _iSkillCtr = iActorTm.gameObject.GetOrAddComponent<SkillController>();
            _iSkillCtr?.Initialize(_iActor as ICaster);
        }

        public override void Execute()
        {
            if (_data == null)
                return;

        }
    }
}

