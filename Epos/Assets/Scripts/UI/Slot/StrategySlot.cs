using UnityEngine;

using Cysharp.Threading.Tasks;

using Common;
using TMPro.Examples;
using TMPro;

namespace UI.Slot
{
    public class StrategySlot : BaseSlot<StrategySlot.Param>
    {
        public class Param : Common.Param
        {
            public System.Type StrategyType { get; private set; } = null;

            public Param(System.Type strategyType)
            {
                StrategyType = strategyType;
            }
        }

        [SerializeField] private RectTransform selectRootRectTm = null;
        [SerializeField] private TextMeshProUGUI strategyTypeNameTMP = null;
        [SerializeField] private UnityEngine.UI.Button selectBtn = null;

        public override async UniTask InitializeAsync(Param param = null)
        {
            await base.InitializeAsync(param);

            strategyTypeNameTMP?.SetText(param?.StrategyType?.Name);
            
            selectBtn?.onClick?.RemoveAllListeners();
            selectBtn?.onClick?.AddListener(OnClickSelect);
        }

        public override UniTask ActivateAsync(Param param = null)
        {
            base.ActivateAsync(param);
            
            return UniTask.CompletedTask;
        }

        private void OnClickSelect()
        {
            
        }
    }
}