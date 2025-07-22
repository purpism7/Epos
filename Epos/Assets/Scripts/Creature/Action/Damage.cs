using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Creator;
using GameSystem.Event;
using Parts;

namespace Creature.Action
{
    public class Damage : Act<Damage.Data>
    {
        public class Data : BaseData
        {
            public ICaster ICaster = null;
            public bool PlayAnimation = true;
        }
        
        public override void Execute()
        {
            if (_data == null)
                return;
            
            //if(_data.PlayAnimation)
                //SetAnimation(_data.AnimationKey, false);

            var iCasterIStat = _data?.ICaster?.IStat;
            if (iCasterIStat != null)
            {
                var damage = iCasterIStat.Get(Stat.EType.Attack);
                _iActor?.IStat?.Add(Stat.EType.Hp, -damage);

                var data = new TextDamage.Data
                {
                    TargetTm = _iActor?.Transform,
                    Offset = new Vector2(0, _iActor.Height + 0.5f),

                }.WithDamage(damage);

                UICreator<TextDamage, TextDamage.Data>.Get?
                    .Create()?
                    .Activate(data);
            }
        }
    }
}
