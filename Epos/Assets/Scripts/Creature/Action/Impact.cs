using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine.Unity;
using VContainer;

using Creator;
using GameSystem.Event;
using Parts;
using Common;

namespace Creature.Action
{
    public class Impact : Act<Impact.Param>
    {
        public class Param : ActParam
        {
            public ICombatant ICombatant { get; private set; } = null;
            public EImpactType EImpactType { get; private set; } = EImpactType.None;
            public bool PlayAnimation = true;

            public Param WithICombatant(ICombatant iCombatant)
            {
                ICombatant = iCombatant;
                return this;
            }

            public Param WithEImpactType(EImpactType eImpactType)
            {
                EImpactType = eImpactType;
                return this;
            }
        }

        [Inject] private UIFactory _uiFactory = null;

        private UICreator<CombatText, CombatText.Param> _uiCreator = null;

        public override void Execute()
        {
            if (_param == null)
                return;
            
            var iCasterIStat = _param?.ICombatant?.IActor?.IStat;
            if (iCasterIStat != null)
            {
                float value = 0;
                if(_param.EImpactType == EImpactType.Damage)
                    value = -iCasterIStat.Get(Stat.EType.Attack);
                else
                    value = 10f;
        
                _iActor?.IStat?.Add(Stat.EType.Hp, Stat.ESubType.Hp, value);

                EventHandler.Notify(new StatChangedEventData(_iActor.Id, _iActor.IStat));

                ActivateCombatText(value);
            }
        }

        private void ActivateCombatText(float damage)
        {
            var combatTextParam = new CombatText.Param
            {
                TargetTm = _iActor?.Transform,
                Offset = new Vector2(0, _iActor.Height + 0.5f),
            }
            .WithValue(damage)
            .WithEImpactType(_param.EImpactType);

            if(_uiCreator == null)
                _uiCreator = _uiFactory?.Create<CombatText, CombatText.Param>();

            var combatText = _uiCreator?
                .SetWorldUI(true)?
                .Create();
            combatText?.ActivateAsync(combatTextParam);
        }
    }
}
