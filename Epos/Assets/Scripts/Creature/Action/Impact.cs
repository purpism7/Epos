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
            public ImpactType ImpactType { get; private set; } = ImpactType.None;
            public float Multiplier { get; private set; } = 1f;

            public bool PlayAnimation = true;

            public Param WithIStat(IStat iStat)
            {
                IStat = iStat;
                return this;
            }

            public Param WithImpactType(ImpactType impactType)
            {
                ImpactType = impactType;
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
                if(_param.ImpactType == ImpactType.Heal)
                {
                    value = 10f;

                    var effectParam = new Effect.Param()
                        .WithRootTm(_iActor.Transform)
                        .WithReturnParent(true);

                    _iActor?.IEffectCtr?.Activate("Eff_Hill_01", effectParam);
                }
                else
                {
                    value = -iStat.Get(Stat.EType.Attack);
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
            .WithImpactType(_param.ImpactType);

            if(_uiCreator == null)
                _uiCreator = _uiFactory?.Create<CombatText, CombatText.Param>();

            var combatText = _uiCreator?
                .SetWorldUI(true)?
                .Create();
            combatText?.ActivateAsync(combatTextParam);
        }
    }
}
