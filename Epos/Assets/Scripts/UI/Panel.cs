using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{ 
    public abstract class Panel<T> : Component where T : Component.BaseData
    {
        // public class Base
        // {
        //     
        // }

        protected T _data = null;

        public virtual Panel<T> Initialize(T data = null)
        {
            _data = data;

            return this;
        }

        public virtual void Activate(T data = null)
        {
            base.Activate();

            if (data == null)
                _data = data;
        }
        
        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}


