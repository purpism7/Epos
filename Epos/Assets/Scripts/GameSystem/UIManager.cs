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
        
        [SerializeField] 
        private GameObject[] panelGameObjs = null;

        private Dictionary<System.Type, Panel> _cachedPanelDic = null;
        private Dictionary<System.Type, GameObject> _uIGameObjDic = null;
         
        protected override void Initialize()
        {
            DontDestroyOnLoad(this);
            
            _uIGameObjDic = new();
            _uIGameObjDic.Clear();
            
            LoadAssetAsync().Forget();
        }

        private async UniTask LoadAssetAsync()
        {
            await AddressableManager.Instance.LoadAssetAsync<GameObject>("UI",
                (asyncOperationHandle) =>
                {
                    var gameObj = asyncOperationHandle.Result;
                    if (gameObj)
                        _uIGameObjDic?.TryAdd(gameObj.GetType(), gameObj);
                });
        }

        public T GetPanel<T, V>(V data = null) where T : Panel where V : Panel<V>.Base 
        {
            if (_cachedPanelDic == null)
            {
                _cachedPanelDic = new();
                _cachedPanelDic.Clear();
            }

            Panel basePanel = null;
            if (_cachedPanelDic.TryGetValue(typeof(T), out basePanel))
                return basePanel as T;
            
            var reFullName = typeof(T).FullName?.Replace('.', '/');
            var gameObj = AddressableManager.Instance?.LoadAssetByNameAsync<GameObject>($"{UIPath}/{reFullName}.prefab");
            if (!gameObj)
                return null;

            basePanel = gameObj.GetComponent<T>();
            
            var panel = (basePanel as Panel<V>)?.Initialize(data);
            if(panel != null)
                _cachedPanelDic?.TryAdd(typeof(T), panel);
                    
            basePanel?.transform.SetAsLastSibling();
            
            return panel as T;
        }

        public T Get<T>() where T : Object
        {
            var reFullName = typeof(T).FullName?.Replace('.', '/');
            var t = AddressableManager.Instance?.LoadAssetByNameAsync<T>($"{UIPath}/{reFullName}.prefab");

            
            return t;
        }
    }
}

