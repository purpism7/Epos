using UnityEditor.Rendering;
using UnityEngine;

namespace InputStates
{
    public interface IInputState
    {
        void Initialize(InputParam param);
        void ChainUpdate();
    }
    
    public class InputParam
    {
        
    }
    
    public abstract class InputState<TParam> : IInputState where TParam : InputParam
    {
        protected TParam _param = null;
        
        protected virtual void Initialize(TParam param)
        {
            _param = param;
        }
        
        void IInputState.Initialize(InputParam param)
        {
            Debug.Log("InputState Initialize");
            // 안전하게 캐스팅 후 제네릭 Initialize 호출
            if (param is TParam typedParam)
            {
                Initialize(typedParam);
            }
        }

        public virtual void ChainUpdate()
        {
            
        }
    }
}

