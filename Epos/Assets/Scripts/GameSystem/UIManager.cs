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
         
        protected override void Initialize()
        {
            LoadAssetAsync().Forget();


            var reFullName = typeof(BattleForces).FullName.Replace('.', '/');
            var component =
                AddressableManager.Instance?.LoadAssetByNameAsync<BattleForces>(
                    $"{UIPath}/{reFullName}.prefab");
        }

        private async UniTask LoadAssetAsync()
        {
            // await AddressableManager.Instance.LoadAssetAsync<Component>("UI",
            //     (asyncOperationHandle) =>
            //     {
            //         var spriteAtlas = asyncOperationHandle.Result;
            //         if (spriteAtlas != null)
            //             _spriteAtlasDic?.TryAdd(spriteAtlas.name, spriteAtlas);
            //     });
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
            
            foreach (var panelGameObj in panelGameObjs)
            {
                if(!panelGameObj)
                    continue;
                
                if (panelGameObj.GetComponent<T>() != null)
                { 
                    basePanel = Instantiate(panelGameObj, rootRectTm)?.GetComponent<T>();
                    var panel = (basePanel as Panel<V>)?.Initialize(data);
                   
                    if(panel != null)
                        _cachedPanelDic?.TryAdd(typeof(T), panel);
                    
                    basePanel?.transform.SetAsLastSibling();
                    
                    return panel as T;
                }
            }

            return null;
        }

        public T Get<T>() where T : Object
        {
            var reFullName = typeof(T).FullName?.Replace('.', '/');
            var t = AddressableManager.Instance?.LoadAssetByNameAsync<T>($"{UIPath}/{reFullName}.prefab");

            
            return t;
        }
    }
}

