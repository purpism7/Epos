using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

using UI;

namespace GameSystem
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] 
        private RectTransform rootRectTm = null;
        
        [SerializeField] 
        private GameObject[] panelGameObjs = null;

        private Dictionary<System.Type, Panel> _cachedPanelDic = null;
         
        protected override void Initialize()
        {
            
        }

        public T GetPanel<T, V>() where T : Panel where V : Panel<V>.Base 
        {
            if (_cachedPanelDic == null)
            {
                _cachedPanelDic = new();
                _cachedPanelDic.Clear();
            }

            Panel basePanel = null;
            if (_cachedPanelDic.TryGetValue(typeof(T), out basePanel))
                return basePanel as T;
            
            foreach (var panelGameObj in panelGameObjs)
            {
                if(!panelGameObj)
                    continue;
                
                if (panelGameObj.GetComponent<T>() != null)
                { 
                    basePanel = Instantiate(panelGameObj, rootRectTm)?.GetComponent<T>();
                    var panel = (basePanel as Panel<V>)?.Initialize();
                   
                    if(panel != null)
                        _cachedPanelDic?.TryAdd(typeof(T), panel);
                    
                    basePanel?.transform.SetAsLastSibling();
                    
                    return panel as T;
                }
            }

            return null;
        }
    }
}

