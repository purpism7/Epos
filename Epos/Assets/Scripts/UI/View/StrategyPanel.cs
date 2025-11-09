using Battle;
using Battle.Formation;
using Cysharp.Threading.Tasks;
using UI.Slot;
using UnityEngine;
using VContainer;

namespace UI.View
{
    public class StrategyPanel : Common.Component<StrategyPanel.Param>
    {
        public class Param : Common.Param
        {
            // Add parameters here if needed in the future
        }

        [SerializeField] private Animator animator = null;

        [Inject] private IFormationController _iFormationController = null;
        
        private StrategySlot[] _strategySlots = null;
        
        public override async UniTask InitializeAsync(Param param)
        {
            await base.InitializeAsync(param);

            _strategySlots = GetComponentsInChildren<StrategySlot>(true);

            await _strategySlots[0].InitializeAsync(new StrategySlot.Param(typeof(Adaptive)));
            await _strategySlots[1].InitializeAsync(new StrategySlot.Param(typeof(Offensive)));
            await _strategySlots[2].InitializeAsync(new StrategySlot.Param(typeof(Defensive)));
        }

        public override async UniTask ActivateAsync(Param param)
        {
            await base.ActivateAsync(param);

            animator?.SetBool("OnOff", false);
            
            foreach (var strategySlot in _strategySlots)
            {
                await strategySlot.ActivateAsync();
            }
        }

        public override void Deactivate()
        {
            //base.Deactivate();

            animator?.SetBool("OnOff", true);
        }
    }
}

