using UnityEngine;
using UnityEngine.UI;

using Cysharp.Threading.Tasks;
using DG.Tweening;

using Creature;
using GameSystem.Event;
using System;

namespace  UI.Parts
{
    public class HpProgress : BaseHpProgressPart<HpProgress.Param>
    {
        public class Param : BaseHpProgressPart<HpProgress.Param>.Param
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            _ICombatant = param?.ICombatant;
            
            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}

