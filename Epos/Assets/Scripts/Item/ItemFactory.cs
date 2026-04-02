using UnityEngine;

using Common;

namespace Item
{
    public class ItemFactory : Factory
    {
        // protected readonly UIManager _uiManager = null;
        
        public ItemFactory()
        {
            
        }
        
        protected override TElement OnCreate<TElement>(Transform rootTr, out bool initialize)
        {
            initialize = false;
            
            RectTransform rootRectTr = null;
            if (rootTr != null)
                rootRectTr = rootTr.GetComponent<RectTransform>();

            return null;
            // return _uiManager?.Create<TElement>(rootRectTr, out initialize);
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
