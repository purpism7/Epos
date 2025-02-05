using System.Collections;
using System.Collections.Generic;
using GameSystem;
using UnityEngine;

namespace Creator
{
    public class UICreator<T, V> : Creator<UICreator<T, V>> where T : UI.Component where V : UI.Component.BaseData
    {
        private V _data = null;
  
        public UICreator<T, V> SetData(V data = null) 
        {
            _data = data;
            
            return this;
        }
        
        public T Create()
        {
            var component = UIManager.Instance?.Get<T>() as UI.Component<V>;
            component?.Initialize(_data);
            // if (_component == null)
            //     return null;
            
            
            return component as T;
        }
    }
}
