using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public static class Extension
    {
        public static T AddOrGetComponent<T>(this GameObject gameObj) where T : Component
        {
            if (!gameObj)
                return default;
            
            var t = gameObj.GetComponent<T>();
            if (t == null)
            {
                t = gameObj.AddComponent<T>();
            }

            return t;
        }
    }
}

