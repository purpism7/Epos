using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

using UI;
using UI.Panels;

namespace GameSystem
{
    public class UIManager : Singleton<UIManager>
    {
        private const string UIPath = "Assets/Resource/Prefabs";
        
        [SerializeField] 
        private RectTransform rootRectTm = null;
        [SerializeField] 
        private RectTransform worldUIRootRectTm = null;

        private List<UI.Component> _cachedUIComponentList = null;
        private Dictionary<System.Type, UI.Component> _componentDic = null;
         
        protected override void Initialize()
        {
            DontDestroyOnLoad(this);
            
            _componentDic = new();
            _componentDic.Clear();
            
            LoadAssetAsync().Forget();
        }

        private async UniTask LoadAssetAsync()
        {
            await AddressableManager.Instance.LoadAssetAsync<GameObject>("UI",
                (asyncOperationHandle) =>
                {
                    var gameObj = asyncOperationHandle.Result;
                    if (gameObj)
                    {
                        var component = gameObj.GetComponent<UI.Component>();
                        if (component == null)
                            return;
                        
                        Debug.Log(component.name);
                        _componentDic?.TryAdd(component.GetType(), component);
                    }
                });
        }

        private UI.Component Get<T>() where T : UI.Component
        {
            if (_cachedUIComponentList == null)
            {
                _cachedUIComponentList = new();
                _cachedUIComponentList.Clear();
            }

            UI.Component component = _cachedUIComponentList?.Find(component => component != null && !component.IsActivate && component.GetType() == typeof(T));
            if (component != null)
            {
                (component as T)?.Activate();
                return component as T;
            }
 
            // GameObject gameObj = null;
            if (_componentDic != null)
                _componentDic.TryGetValue(typeof(T), out component);

            // if (!gameObj)
            // {
            //     var reFullName = typeof(T).FullName?.Replace('.', '/');
            //     gameObj = AddressableManager.Instance?.LoadAssetByNameAsync<GameObject>($"{UIPath}/{reFullName}.prefab");
            //     if (!gameObj)
            //         return null;
            // }
            if (component == null)
                return null;

            var t = Instantiate(component.gameObject, rootRectTm)?.GetComponent<T>();
            if(t != null)
                _cachedUIComponentList?.Add(t);
            // component = gameObj.GetComponent<T>();
            // if(component != null)
            //     _cachedUIComponentDic?.TryAdd(typeof(T), component);
            
            return t;
        }

        public T GetPanel<T, V>(V data = null) where T : UI.Component where V : UI.Component.BaseData
        {
            var component = Get<T>();
            
            var panel = (component as Panel<V>)?.Initialize(data);
            panel?.transform.SetAsLastSibling();
 
            return panel as T;
        }
    }
}

