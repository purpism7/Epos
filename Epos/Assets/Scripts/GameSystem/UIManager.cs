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

        [Inject] private IObjectResolver _iResolver = null;
        [Inject] private AddressableManager _addressableManager = null;
        [Inject] private ObjectPooler _objectPooler = null;

        private Dictionary<System.Type, Common.Component> _componentDic = null;

        public Camera UICamera => uiCamera;
        public RectTransform WorldUIRootRectTm => worldUIRootRectTm;
        public Common.Component CurrView { get; private set; } = null;
        public Common.Component CurrPopup { get; private set; } = null;
        public RectTransform CurrViewRectTm { get; private set; } = null;

        public bool IsEndLoad { get; private set; } = false;


        // protected override void Initialize()
        // {
        //     DontDestroyOnLoad(this);


        //     //LoadAssetAsync().Forget();
        // }

        public async UniTask InitializeAsync()
        {
            _componentDic = new();
            _componentDic.Clear();

            await LoadAssetAsync();
        }

        public void SetIObjectResolver(VContainer.IObjectResolver iResolver)
        {
            _iResolver = iResolver;
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

        //public Common.Component Get<T>(Transform rootTm = null, bool worldUI = false) where T : Common.Component
        //{
        //    var iPoolable = _objectPooler.Get<T>();
        //    if (iPoolable != null)
        //        return iPoolable;

        //    Common.Component component = null;
        //    if (_componentDic != null)
        //        _componentDic.TryGetValue(typeof(T), out component);

        //    if (component == null)
        //        return null;

        //    component = Instantiate(component.gameObject)?.GetComponent<T>();
        //    _container?.InjectGameObject(component?.gameObject);

        //    if (component != null)
        //        _objectPooler?.Add(component);

        //    if (!rootTm)
        //    {
        //        if (worldUI)
        //            rootTm = worldUIRootRectTm;
        //        else
        //            rootTm = rootRectTm;
        //    }

        //    component?.transform.SetParent(rootTm);

        //    return component;
        //}

        public Common.Component Get<T, V>(Transform rootTm, out bool isInitialize, V data = null, bool worldUI = false) where T : Common.Component where V : Common.Param
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

                component = Instantiate(component.gameObject)?.GetComponent<T>();
                _iResolver?.InjectGameObject(component?.gameObject);

                if (component != null)
                    _objectPooler?.Add(component);
            }

            RectTransform rootRectTm = null;
            if (component is UI.View.BaseView<V> baseView)
            {
                rootRectTm = viewRootRectTm;
                SetCurrView(baseView);

                if(isInitialize)
                    baseView.Configure(_iResolver);
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

        //public T GetPanel<T, V>(V param = null) where T : Common.Component where V : Common.Param
        //{
        //    bool initialize = false;
        //    var component = Get<T, V>(param, rootRectTm, out initialize);

        //    var panel = component as Panel<V>;
        //    if(initialize)
        //        panel?.InitializeAsync(param);

        //    component?.transform.SetAsLastSibling();
        //    panel?.Activate(param);

        //    // CurrPanel = panel;

        //    return panel as T;
        //}

        //public T GetPopup<T, V>(V data = null) where T : Common.Component where V : Common.Param
        //{
        //    bool initialize = false;
        //    var component = Get<T, V>(data, rootRectTm, out initialize);

        //    var panel = component as Panel<V>;
        //    if(initialize)
        //        panel?.Initialize(data);

        //    component?.transform.SetAsLastSibling();
        //    panel?.Activate(data);

        //    // CurrPanel = panel;

        //    return panel as T;
        //}

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

        private void SetCurrView(Common.Component component)
        {
            CurrView = component;
            CurrViewRectTm = component?.GetComponent<RectTransform>();
        }

        public void SetCurrPopup(Common.Component component)
        {
            CurrPopup = component;
        }
    }
}

