using UnityEngine;

using Common;
using VContainer;

namespace Item
{
    public class ItemFactory : Factory
    {
        [Inject] private IItemManager _itemManager = null;
        // protected readonly UIManager _uiManager = null;
        
        public ItemFactory()
        {
            
        }
        
        protected override TElement OnCreate<TElement>(Transform rootTr, out bool initialize)
        {
            initialize = false;
            
            // RectTransform rootRectTr = null;
            // if (rootTr != null)
            //     rootRectTr = rootTr.GetComponent<RectTransform>();

            return _itemManager?.Get<TElement>(rootTr, out initialize);
        }
        
        public TElement Create<TElement>(RectTransform rootRectTr)  where TElement : Element
        {
            var element = OnCreate<TElement>(rootRectTr, out var initialize);
            element?.Activate();
            
            return element;
        }

        public TElement Create<TElement, TParam>(RectTransform rootRectTr, TParam param = null)  where TElement : Element where TParam : ElementParam
        {
            var element = OnCreate<TElement>(rootRectTr, out var initialize);
            if (initialize)
            {
                // if(element is Slot<TParam> slot)
                //     slot.Initialize(param);
            }
            
            element?.Activate();
            
            return element;
        }
    }
}
