using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer.Unity;

using UI;
using UI.Panels;
using VContainer;

namespace GameSystem
{
    public class UIManager :  Singleton<UIManager>
    {
        // private const string UIPath = "Assets/Resource/Prefabs";

        [SerializeField] private Camera uiCamera = null;
        [SerializeField] private RectTransform rootRectTm = null;
        [SerializeField] private RectTransform worldUIRootRectTm = null;
        
        //private List<Common.Component> _cachedComponentList = null;
        private Dictionary<System.Type, Common.Component> _componentDic = null;

        public Camera UICamera => uiCamera;
        public RectTransform WorldUIRootRectTm => worldUIRootRectTm;
        public Common.Component CurrPanel { get; private set; } = null;

        public bool IsEndLoad { get; private set; } = false;

        [Inject]
        private ObjectPooler _objectPooler = null;
        
        protected override void Initialize()
        {
            DontDestroyOnLoad(this);
        
            //_objectPooler = FindFirstObjectByType<ObjectPooler>();
            
            _componentDic = new();
            _componentDic.Clear();
            
            LoadAssetAsync().Forget();
        }

        public async UniTask InitializeAsync()
        {
            
        }

        private async UniTask LoadAssetAsync()
        {
            IsEndLoad = false;

            await AddressableManager.Instance.LoadAssetAsync<GameObject>("UI",
                (asyncOperationHandle) =>
                {
                    var gameObj = asyncOperationHandle.Result;
                    if (gameObj)
                    {
                        var component = gameObj.GetComponent<Common.Component>();
                        if (component == null)
                            return;
                        
                         //Debug.Log(component.name);
                        _componentDic?.TryAdd(component.GetType(), component);
                    }
                });

            IsEndLoad = true;
        }

        public Common.Component Get<T>(Transform rootTm = null, bool worldUI = false) where T : Common.Component
        {
            var iPoolable = _objectPooler.Get<T>();
            if (iPoolable != null)
                return iPoolable;

            Common.Component component = null;
            if (_componentDic != null)
                _componentDic.TryGetValue(typeof(T), out component);

            if (component == null)
                return null;
                
            component = Instantiate(component.gameObject)?.GetComponent<T>();
            if (component != null)
                _objectPooler?.Add(component);
       
            if (!rootTm)
            {
                if (worldUI)
                    rootTm = worldUIRootRectTm;
                else
                    rootTm = rootRectTm;
            }
            
            component?.transform.SetParent(rootTm);

            return component;
        }
        
        private Common.Component Get<T, V>(V data, Transform rootTm, out bool initialize) where T : Common.Component where V : Common.ComponentData
        {
            initialize = false;

            var iPoolable = _objectPooler.Get<T>();
            if (iPoolable != null)
                return iPoolable;

            Common.Component component = null;
            // GameObject gameObj = null;
            if (_componentDic != null)
                _componentDic.TryGetValue(typeof(T), out component);

            if (component == null)
                return null;

            component = Instantiate(component.gameObject, rootTm)?.GetComponent<T>();
            if(component != null)
                _objectPooler?.Add(component);
      
            initialize = true;
    
            return component;
        }

        public T GetPanel<T, V>(V data = null) where T : Common.Component where V : Common.ComponentData
        {
            bool initialize = false;
            var component = Get<T, V>(data, rootRectTm, out initialize);

            var panel = component as Panel<V>;
            if(initialize)
                panel?.Initialize(data);
            
            component?.transform.SetAsLastSibling();
            panel?.Activate(data);
            
            // CurrPanel = panel;
            
            return panel as T;
        }
        
        public T GetPopup<T, V>(V data = null) where T : Common.Component where V : Common.ComponentData
        {
            bool initialize = false;
            var component = Get<T, V>(data, rootRectTm, out initialize);

            var panel = component as Panel<V>;
            if(initialize)
                panel?.Initialize(data);
            
            component?.transform.SetAsLastSibling();
            panel?.Activate(data);
            
            // CurrPanel = panel;
            
            return panel as T;
        }
        
        // public T GetPart<T, V>(V data = null, bool worldUI = false, Transform rootTm = null) where T : UI.Component where V : UI.Component.Data
        // {
        //     if (!rootTm)
        //         rootTm = worldUI ? worldUIRootRectTm : rootRectTm;
        //     
        //     bool initialize = false;
        //     var component = Get<T, V>(data, rootTm, out initialize);
        //     
        //     var part = component as Part<V>;
        //     if(initialize)
        //         part?.Initialize(data);
        //     
        //     component?.transform.SetAsLastSibling();
        //     
        //     part?.Activate(data);
        //
        //     return part as T;
        // }

        public void SetPanel(Common.Component component)
        {
            CurrPanel = component;
        }
    }
}

