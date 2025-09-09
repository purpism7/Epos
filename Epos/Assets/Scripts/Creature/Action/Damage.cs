using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine.Unity;
using VContainer;

using Creator;
using GameSystem.Event;

using Parts;

namespace Creature.Action
{
    public class Damage : Act<Damage.Param>
    {
        public class Param : ActParam
        {
            public ICombatant ICombatant { get; private set; } = null;
            public bool PlayAnimation = true;

            public Param WithICombatant(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
                return this;
            }
        }

        [Inject] private UIFactory _uiFactory = null;

        //private TextDamage _textDamage = null;
        private UICreator<TextDamage, TextDamage.Param> _uiCreator = null;

        public override void Execute()
        {
            if (_param == null)
                return;
            
            //if(_data.PlayAnimation)
                //SetAnimation(_data.AnimationKey, false);
            //_iActor.SkeletonAnimation.

             var iCasterIStat = _param?.ICombatant?.IActor?.IStat;
            if (iCasterIStat != null)
            {
                var damage = iCasterIStat.Get(Stat.EType.Attack);
                _iActor?.IStat?.Add(Stat.EType.Hp, Stat.ESubType.Hp, -damage);

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
            if(_uiCreator == null)
                _uiCreator = _uiFactory?.Create<TextDamage, TextDamage.Param>();

            var textDamage = _uiCreator?
                .SetWorldUI(true)?
                .Create();
            textDamage?.ActivateAsync(textDamageParam);
        }
    }
}
