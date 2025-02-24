using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                
                _iActor?.IStat?.Add(Stat.EType.Hp, damage);
            }
            
            // _iActor?.EventHandler?.Invoke(_data?.ICaster);
        }
    }
}
