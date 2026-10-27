using UnityEngine;

namespace Managers
{

    public interface IManager
    {
        bool IsActive { get; }
        
        void Activate();
        void Deactivate();
        
        void ChainUpdate();
        void ChainLateUpdate();
    }
}
