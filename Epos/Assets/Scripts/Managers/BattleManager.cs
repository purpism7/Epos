using Commons;
using UnityEngine;

namespace Managers
{
    public interface IBattleManager
    {
        // void BeginBattle<>
    }
    
    public class BattleManager : Entity
    {
        
        
        public void Initialize()
        {
            
        }
        
        public override void Activate()
        {
            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        public override void ChainUpdate()
        {
            base.ChainUpdate();
        }
    }
}

