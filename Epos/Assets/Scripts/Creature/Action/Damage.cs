using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using Creator;
using GameSystem.Event;
using Parts;
using UnityEditor;
using Spine.Unity;

namespace Creature.Action
{
    public class Damage : Act<Damage.Param>
    {
        public class Param : ActParam
        {
            public ICaster ICaster = null;
            public bool PlayAnimation = true;
        }

        //private TextDamage _textDamage = null;

        public override void Execute()
        {
            if (_param == null)
                return;
            
            //if(_data.PlayAnimation)
                //SetAnimation(_data.AnimationKey, false);
            //_iActor.SkeletonAnimation.

            var iCasterIStat = _param?.ICaster?.IStat;
            if (iCasterIStat != null)
            {
                var damage = iCasterIStat.Get(Stat.EType.Attack);
                _iActor?.IStat?.Add(Stat.EType.Hp, -damage);

                EventHandler.Notify(new StatChangedEventData(_iActor.Id, _iActor.IStat));

                ActivateTextDamage(damage);
            }
        }

        private void ActivateTextDamage(float damage)
        {
            var textDamageParam = new TextDamage.Param
            {
                TargetTm = _iActor?.Transform,
                Offset = new Vector2(0, _iActor.Height + 0.5f),

            }.WithDamage(damage);

            //if (_textDamage == null)
            //var uiCreator = _uiFactory?.Create<UI.Popup.BattleStart, UI.Popup.BattleStart.Param>();
            //var textDamage = UICreator<TextDamage, TextDamage.Param>.Get.Create();
            //textDamage?.Activate(textDamageParam);
        }
    }
}
