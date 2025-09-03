using Cysharp.Threading.Tasks;
using GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;

using UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Creator
{
    public class UIFactory
    {
        readonly Func<Type, Type, object> _factory;
        readonly private IObjectResolver _iResolver;

        [Inject]
        public UIFactory(IObjectResolver iResolver)
        {
            _iResolver = iResolver;
        }

        public UICreator<T, V> Create<T, V>() where T : Common.Component, new() where V : Common.Param
        {

            return _iResolver?.Resolve<UICreator<T, V>>();
            //return (UICreator<T, V>)_factory(typeof(T), typeof(V));
        }
    }

    public class UICreator<T, V> where T : Common.Component where V : Common.Param
    {
        [Inject] private UIManager _uiManager = null;

        private V _param = null;
        private RectTransform _rootRectTm = null;
  
        public UICreator<T, V> SetParam(V param = null) 
        {
            _param = param;
            
            return this;
        }
        
        public UICreator<T, V> SetRoot(RectTransform rootRectTm)
        {
            _rootRectTm = rootRectTm;   
            
            return this;
        }
        

        public T Create()
        {
            var component = GetComponent();
            component?.InitializeAsync(_param);

            return component as T;
        }

        public async UniTask<T> CreateAsync()
        {
            var component = GetComponent(); 
            await component.InitializeAsync(_param);
            
            return component as T;
        }

        private Common.Component<V> GetComponent()
        {
            var component = _uiManager?.Get<T, V>(_rootRectTm) as Common.Component<V>;
            var rectTm = component?.GetComponent<RectTransform>();
            if (rectTm)
            {
                rectTm.anchoredPosition3D = Vector3.zero;
                rectTm.sizeDelta = Vector2.zero;
                rectTm.transform.localScale = Vector3.one;
            }
  
            if (component is BasePopup<V> popup)
                Debug.Log(popup);

            return component;
        }
    }
}
