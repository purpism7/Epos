using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;
using VContainer.Unity;
using VContainer;

using UI;
using UI.Panels;

namespace GameSystem
{
    public class UIManager : MonoBehaviour
    {
        // private const string UIPath = "Assets/Resource/Prefabs";

        [SerializeField] private Camera uiCamera = null;
        [SerializeField] private RectTransform viewRootRectTm = null;
        [SerializeField] private RectTransform popupRootRectTm = null;
        [SerializeField] private RectTransform worldUIRootRectTm = null;
        [SerializeField] private RectTransform collectRootRectTr = null;

        [Inject] private AddressableManager _addressableManager = null;
        [Inject] private ObjectPooler _objectPooler = null;

        private Dictionary<System.Type, Common.Component> _componentDic = null;

        public Camera UICamera => uiCamera;
        public RectTransform WorldUIRootRectTm => worldUIRootRectTm;
        public RectTransform CollectRootRectTr => collectRootRectTr;
        public Common.Component CurrView { get; private set; } = null;
        public Common.Component CurrPopup { get; private set; } = null;
        public RectTransform CurrViewRectTm { get; private set; } = null;

        public bool IsEndLoad { get; private set; } = false;

        public async UniTask InitializeAsync()
        {
            _componentDic = new();
            _componentDic.Clear();

            await LoadAssetAsync();
        }

        private async UniTask LoadAssetAsync()
        {
            IsEndLoad = false;

            await _addressableManager.LoadAssetAsync<GameObject>("UI",
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

        /// <param name="resolver">씬/필드 스코프의 IObjectResolver. 뷰·팝업 생성 및 주입에 사용.</param>
        public Common.Component Get<T, V>(IObjectResolver resolver, Transform rootTm, out bool isInitialize, V data = null, bool worldUI = false) where T : Common.Component where V : Common.Param
        {
            isInitialize = false;

            if (CurrView is UI.View.BaseView<V> view)
            {
                if (view.GetType() == typeof(T))
                    return null;
            }

            if (CurrPopup is UI.Popup.BasePopup<V> popup)
            {
                if (popup.GetType() == typeof(T))
                    return null;
            }

            Common.Component component = null;

            component = _objectPooler.Get<T>();
            if (component == null)
            {
                if (_componentDic != null)
                    _componentDic.TryGetValue(typeof(T), out component);

                if (component == null)
                    return null;

                isInitialize = true;

                GameObject newObj = resolver != null
                    ? resolver.Instantiate(component.gameObject)
                    : Instantiate(component.gameObject);
                
                component = newObj?.GetComponent<T>();

                if (component != null)
                    _objectPooler?.Add(component);
            }

            RectTransform rootRectTm = null;
            if (component is UI.View.BaseView<V> baseView)
            {
                rootRectTm = viewRootRectTm;
                SetCurrView(baseView);

                if(isInitialize)
                    baseView.Configure(resolver);
            }

            if (component is UI.Popup.BasePopup<V> basePopup)
            {
                rootRectTm = popupRootRectTm;
                SetCurrPopup(basePopup);
            }

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

        private void SetCurrView(Common.Component component)
        {
            CurrView = component;
            CurrViewRectTm = component?.GetComponent<RectTransform>();
        }

        public void SetCurrPopup(Common.Component component)
        {
            CurrPopup = component;
        }

        public Vector3 ScreenToWorldPoint(Vector3 screenPoint)
        {
            if (UICamera == null)
                return screenPoint;

            return UICamera.ScreenToWorldPoint(screenPoint);
        }
    }
}

