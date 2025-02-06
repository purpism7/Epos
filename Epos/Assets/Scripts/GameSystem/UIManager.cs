using System;
using System.Collections;
using System.Collections.Generic;
using Creator;
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

        public UI.Component Get<T>(Transform rootTm = null, bool worldUI = false) where T : UI.Component
        {
            if (_cachedUIComponentList == null)
            {
                _cachedUIComponentList = new();
                _cachedUIComponentList.Clear();
            }

            UI.Component component = _cachedUIComponentList?.Find(component => component != null && !component.IsActivate && component.GetType() == typeof(T));
            if (component != null)
                return component as T;
            else
            {
                if (_componentDic != null)
                    _componentDic.TryGetValue(typeof(T), out component);

                if (component == null)
                    return null;
                
                component = Instantiate(component.gameObject)?.GetComponent<T>();
                if (component != null)
                {
                    _cachedUIComponentList?.Add(component);
                }
            }
            
            if (!rootTm)
            {
                if (worldUI)
                    rootTm = worldUIRootRectTm;
                else
                    rootTm = rootRectTm;
            }
            
            component.transform.SetParent(rootTm);

            return component;
        }
        
        private UI.Component Get<T, V>(V data, Transform rootTm, out bool initialize) where T : UI.Component where V : UI.Component.BaseData
        {
            initialize = false;
            
            if (_cachedUIComponentList == null)
            {
                _cachedUIComponentList = new();
                _cachedUIComponentList.Clear();
            }

            UI.Component component = _cachedUIComponentList?.Find(component => component != null && !component.IsActivate && component.GetType() == typeof(T));
            if (component != null)
                return component as T;
 
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

            component = Instantiate(component.gameObject, rootTm)?.GetComponent<T>();
            if(component != null)
                _cachedUIComponentList?.Add(component);

            initialize = true;
            // component?.GetComponent<T>()?.Initialize(data);
 
            return component;
        }

        public T GetPanel<T, V>(V data = null) where T : UI.Component where V : UI.Component.BaseData
        {
            bool initialize = false;
            var component = Get<T, V>(data, rootRectTm, out initialize);

            var panel = component as Panel<V>;
            if(initialize)
                panel?.Initialize(data);
            
            component?.transform.SetAsLastSibling();
            
            panel?.Activate(data);
 
            return panel as T;
        }
        
        public T GetPart<T, V>(V data = null, bool worldUI = false, Transform rootTm = null) where T : UI.Component where V : UI.Component.BaseData
        {
            if (!rootTm)
                rootTm = worldUI ? worldUIRootRectTm : rootRectTm;
            
            bool initialize = false;
            var component = Get<T, V>(data, rootTm, out initialize);
            
            var part = component as Part<V>;
            if(initialize)
                part?.Initialize(data);
            
            component?.transform.SetAsLastSibling();
            
            part?.Activate(data);
 
            return part as T;
        }
    }
}

