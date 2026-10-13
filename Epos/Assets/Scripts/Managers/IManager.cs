using UnityEngine;

namespace Managers
{
    public interface IManager
    {
        void Initialize();
        
        void ChainUpdate();
        void ChainLateUpdate();
    }
}
