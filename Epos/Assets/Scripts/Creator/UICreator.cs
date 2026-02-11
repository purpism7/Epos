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
        /// <summary>씬/필드 스코프의 IObjectResolver를 넘겨 UICreator를 해당 스코프에서 생성합니다.</summary>
        public UICreator<T, V> Create<T, V>(IObjectResolver resolver) where T : Common.Component, new() where V : Common.Param
        {
            return resolver?.Resolve<UICreator<T, V>>();
        }
    }

    public class UICreator<T, V> where T : Common.Component where V : Common.Param
    {
        [Inject] private UIManager _uiManager = null;
        [Inject] private IObjectResolver _iResolver = null;

        private V _param = null;
        private RectTransform _rootRectTm = null;
        private bool _isWorldUI = false;
        // private bool _resetSizeDelta = true;
  
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

        public UICreator<T, V> SetWorldUI(bool isWorldUI)
        {
            _isWorldUI = isWorldUI;
            return this;
        }

        // public UICreator<T, V> SetResetSizeDelta(bool resetSizeDelta)
        // {
        //     _resetSizeDelta = resetSizeDelta;
        //     return this;
        // }

        public T Create()
        {
            var component = GetComponent(out bool isInitialize);
            if (isInitialize)
                component?.InitializeAsync(_param);

            return component as T;
        }

        public async UniTask<T> CreateAsync()
        {
            var component = GetComponent(out bool isInitialize);
            if(isInitialize)
                await component.InitializeAsync(_param);
            
            return component as T;
        }

        private Common.Component<V> GetComponent(out bool isInitialize)
        {
            isInitialize = false;
            var component = _uiManager?.Get<T, V>(_iResolver, _rootRectTm, out isInitialize, worldUI: _isWorldUI) as Common.Component<V>;

            var rectTm = component?.GetComponent<RectTransform>();
            if (rectTm)
            {
                rectTm.sizeDelta = Vector2.zero;
                rectTm.anchoredPosition3D = Vector3.zero;
                rectTm.transform.localScale = Vector3.one;

                rectTm.SetAsLastSibling();
            }

            return component;
        }
    }
}
