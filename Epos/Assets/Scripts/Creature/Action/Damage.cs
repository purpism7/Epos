using System.Collections;
using System.Collections.Generic;
using Creator;
using UnityEngine;

using GameSystem.Event;
using Parts;

namespace Creature.Action
{
    public class Damage : Act<Damage.Data>
    {
        public class Data : BaseData
        {
            public ICaster ICaster = null;
        }
        
        public override void Execute()
        {
            if (_data == null)
                return;
            
            SetAnimation(_data.AnimationKey, false);

            var iCasterIStat = _data?.ICaster?.IStat;
            if (iCasterIStat != null)
            {
                var damage = iCasterIStat.Get(Stat.EType.Attack);
                
                _iActor?.IStat?.Add(Stat.EType.Hp, -damage);
                
                UICreator<TextDamage, TextDamage.Data>.Get?
                    .Create()?
                    .Activate(new TextDamage.Data
                    {
                        TargetTm = _iActor?.Transform,
                        Damage = damage
                    });
                
                if(_iActor != null)
                    EventHandler.Notify(new StatChangedEventData(_iActor.Id, _iActor.IStat));
            }
            
            
            
            // _iActor?.EventHandler?.Invoke(_data?.ICaster);
        }
    }
}
