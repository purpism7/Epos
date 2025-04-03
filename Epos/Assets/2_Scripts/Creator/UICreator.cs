using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

using GameSystem;
using UI;
using Unity.VisualScripting;

namespace Creator
{
    public class UICreator<T, V> : Creator<UICreator<T, V>> where T : UI.Component where V : UI.Component.Data
    {
        private V _data = null;
        private RectTransform _rootRectTm = null;
  
        public UICreator<T, V> SetData(V data = null) 
        {
            _data = data;
            
            return this;
        }
        
        public UICreator<T, V> SetRoot(RectTransform rootRectTm)
        {
            _rootRectTm = rootRectTm;   
            
            return this;
        }
        
        public T Create()
        {
            var component = UIManager.Instance?.Get<T>(_rootRectTm) as UI.Component<V>;
            
            var rectTm = component?.GetComponent<RectTransform>();
            if (rectTm)
            {
                rectTm.anchoredPosition3D = Vector3.zero;
                rectTm.sizeDelta = Vector2.zero;
                rectTm.transform.localScale = Vector3.one;   
            }

            if (component is Panel<V>)
                UIManager.Instance?.SetPanel(component);

            if (component as Popup<V>)
            {
                Debug.Log("popup");
            }
            
            component?.Initialize(_data);
            
            // if (_component == null)
            //     return null;
            
            
            return component as T;
        }
    }
}
