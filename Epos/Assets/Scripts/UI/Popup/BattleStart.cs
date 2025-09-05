using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine.Unity;

using Common;
using Spine;

namespace UI.Popup
{
    public class BattleStart : BasePopup<BattleStart.Param>
    {
        public class Param : Common.Param
        {
            public System.Action<TrackEntry> CompletedAction { get; private set; } = null;

            public Param WithCompletedAction(System.Action<TrackEntry> completedAction)
            {
                CompletedAction = completedAction;
                return this;
            }
        }

        [SerializeField] private SkeletonGraphic skeletonGraphic = null;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            skeletonGraphic?.PlayAnimation("Battle_Start_01", false, _param?.CompletedAction, out float duration);

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}