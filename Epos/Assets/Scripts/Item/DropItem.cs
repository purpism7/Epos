using UnityEngine;

using Common;

namespace Item
{
    public class DropItem : Element
    {
        public class Param : ElementParam
        {
            public int SortingOrder { get; private set; } = 0;
            public Vector3 Position { get; private set; } = Vector3.zero;

            public Param(int sortingOrder, Vector3 position)
            {
                SortingOrder = sortingOrder;
                Position = position;
            }
        }

        [SerializeField] private SpriteRenderer spriteRenderer = null;
        
        private Param _param = null;
        
        public void Activate(Param param)
        {
            base.Activate();

            _param = param;

            SetSortingOrder();
            SetDropPosition();
        }

        private void SetSortingOrder()
        {
            if (_param == null)
                return;

            if (spriteRenderer == null)
                return;

            spriteRenderer.sortingOrder = _param.SortingOrder;
        }

        private void SetDropPosition()
        {
            if (_param == null)
                return;
            
            transform.position = _param.Position;
        }
    }
} 

