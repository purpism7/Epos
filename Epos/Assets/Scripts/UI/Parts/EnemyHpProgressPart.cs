using UnityEngine;

using Cysharp.Threading.Tasks;

namespace UI.Parts
{
    public class EnemyHpProgressPart : BaseHpProgressPart<EnemyHpProgressPart.Param>
    {
        public class Param : BaseHpProgressPart<EnemyHpProgressPart.Param>.Param
        {
            
        }
        
        public override UniTask ActivateAsync(Param param)
        {
            base.ActivateAsync(param);
            
            _ICombatant = param?.ICombatant;

            // GameSystem.Event.EventHandler.Add<StatChangedEventData>(OnStatChanged);

            return UniTask.CompletedTask;
        }
    }
}