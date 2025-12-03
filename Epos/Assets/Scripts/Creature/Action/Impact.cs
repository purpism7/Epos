using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using Spine.Unity;
using VContainer;

using GameSystem.Event;
using Common;
using Creator;
using Datas.ScriptableObjects;
using Parts;

namespace Creature.Action
{
    public class Impact : Act<Impact.Param>
    {
        public class Param : ActParam
        {
            public IStat IStat { get; private set; } = null;
            public EImpactType EImpactType { get; private set; } = EImpactType.None;
            public float Multiplier { get; private set; } = 1f;

            public bool PlayAnimation = true;

            public Param WithIStat(IStat iStat)
            {
                IStat = iStat;
                return this;
            }

            public Param WithEImpactType(EImpactType eImpactType)
            {
                EImpactType = eImpactType;
                return this;
            }

            public Param WithMultiplier(float multiplier)
            {
                Multiplier = multiplier;
                return this;
            }
        }

        [Inject] private UIFactory _uiFactory = null;

        private UICreator<CombatText, CombatText.Param> _uiCreator = null;

        public override void Execute()
        {
            if (_param == null)
                return;

            var iStat = _param?.IStat;
            if (iStat != null)
            {
                float value = 0;
                if(_param.EImpactType == EImpactType.Damage)
                    value = -iStat.Get(Stat.EType.Attack);
                else
                {
                    value = 10f;

                    _iActor?.IEffectCtr?.Activate("Eff_Hill_01", new Effect.Param().WithTargetPosition(_iActor.Transform.position));
                }

                value *= _param.Multiplier;
                _iActor?.IStat?.Add(Stat.EType.Hp, Stat.ESubType.Hp, value);

                if(_iActor != null)
                    GameSystem.Event.EventHandler.Notify(new StatChangedEventData(_iActor.Id, _iActor.IStat));

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
