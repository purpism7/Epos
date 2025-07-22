using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creator;
using GameSystem.Event;
using Parts;

namespace Creature.Action
{
    public class Damage : Act<Damage.Param>
    {
        public class Param : ActParam
        {
            public ICaster ICaster = null;
            public bool PlayAnimation = true;
        }
        
        public override void Execute()
        {
            if (_param == null)
                return;
            
            //if(_data.PlayAnimation)
                //SetAnimation(_data.AnimationKey, false);

            var iCasterIStat = _param?.ICaster?.IStat;
            if (iCasterIStat != null)
            {
                var damage = iCasterIStat.Get(Stat.EType.Attack);
                _iActor?.IStat?.Add(Stat.EType.Hp, -damage);

                var textDamageParam = new TextDamage.Param
                {
                    TargetTm = _iActor?.Transform,
                    Offset = new Vector2(0, _iActor.Height + 0.5f),

                }.WithDamage(damage);

                UICreator<TextDamage, TextDamage.Param>.Get?
                    .Create()?
                    .Activate(textDamageParam);
            }
        }
    }
}
