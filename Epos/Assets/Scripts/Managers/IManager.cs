using UnityEngine;

namespace Managers
{
    public interface IManager
    {
        void Activate();
        void Deactivate();
        
        void ChainUpdate();
        void ChainLateUpdate();
    }
}
