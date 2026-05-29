using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using Spine.Unity;

using Common;
using Spine;
using UnityEngine.UI;

namespace UI.Popup
{
    public class BattleWinPopup : BasePopup<BattleWinPopup.Param>
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
        
        [SerializeField] private SkeletonGraphic[] skeletonGraphics = null;
        [SerializeField] private Button btn = null;

        private const string StartAnimationName = "01_Start";
        private const string IdleAnimationName = "02_Idle";
        private const float ButtonActivationDelay = 3f;

        private int _buttonActivationSequenceId = 0;
        private bool _isButtonActivationScheduled = false;
        private bool _isCompleted = false;

        public override UniTask InitializeAsync(Param param)
        {
            base.InitializeAsync(param);

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnClickButton);
                btn.SetActive(false);
            }

            return UniTask.CompletedTask;
        }

        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);

            ++_buttonActivationSequenceId;
            _isButtonActivationScheduled = false;
            _isCompleted = false;
            btn?.SetActive(false);

            int playingStartAnimationCount = 0;
            if (skeletonGraphics != null)
            {
                for (int i = 0; i < skeletonGraphics.Length; ++i)
                {
                    var skeletonGraphic = skeletonGraphics[i];
                    if (skeletonGraphic == null)
                        continue;

                    ++playingStartAnimationCount;
                    if (!skeletonGraphic.PlayAnimation(StartAnimationName, false, _ => OnFinishStartAnimation(skeletonGraphic), out _))
                        --playingStartAnimationCount;
                }
            }

            if (playingStartAnimationCount <= 0)
                ScheduleButtonActivation();

            // skeletonGraphic?.PlayAnimation("Battle_End_Win_01", false, _param?.CompletedAction, out float duration);

            return UniTask.CompletedTask;
        }

        public override void Deactivate()
        {
            ++_buttonActivationSequenceId;
            _isButtonActivationScheduled = false;
            btn?.SetActive(false);

            base.Deactivate();
        }

        private void OnFinishStartAnimation(SkeletonGraphic skeletonGraphic)
        {
            if (!IsActivate || _isCompleted)
                return;

            if (skeletonGraphic == null ||
                !skeletonGraphic.PlayAnimation(IdleAnimationName, true, null, out _))
            {
                ScheduleButtonActivation();
                return;
            }

            ScheduleButtonActivation();
        }

        private void ScheduleButtonActivation()
        {
            if (!IsActivate || _isCompleted || _isButtonActivationScheduled)
                return;

            _isButtonActivationScheduled = true;
            ActivateButtonAfterDelayAsync(_buttonActivationSequenceId).Forget();
        }

        private async UniTaskVoid ActivateButtonAfterDelayAsync(int sequenceId)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ButtonActivationDelay), DelayType.UnscaledDeltaTime);

            if (!IsActivate || _isCompleted || sequenceId != _buttonActivationSequenceId)
                return;

            btn?.SetActive(true);
        }

        private void OnClickButton()
        {
            Complete(trackEntry: null);
        }

        private void Complete(TrackEntry trackEntry)
        {
            if (_isCompleted)
                return;

            _isCompleted = true;
            btn?.SetActive(false);
            _param?.CompletedAction?.Invoke(trackEntry);
        }
    }
}
