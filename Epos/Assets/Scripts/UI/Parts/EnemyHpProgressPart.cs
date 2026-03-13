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
            _combatant = param?.ICombatant;
            
            base.ActivateAsync(param);
            
            return UniTask.CompletedTask;
        }
    }
}