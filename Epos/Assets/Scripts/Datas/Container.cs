using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

namespace Datas
{
    public abstract class Container
    {
        public abstract void Initialize(Container container);
    }
    
    public class Container<T, V> : Container where T : Container, new() where V : Datas.Base
    {
        private static T _instance = default(T);

        public static T Get { get { return _instance; } }
        
        protected V[] _datas = null;
        
        public override void Initialize(Container container)
        {
            try
            {
                _instance = container as T;
                
                var settings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                };
                
                // var datas = Newtonsoft.Json.JsonConvert.DeserializeObject<V[]>(jsonData.Json, settings);
                // if (datas == null)
                //     return;
                //
                // Debug.Log(jsonData.Sheet + " = " + jsonData.Json);
                
                // _datas = datas;
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log(e);
            }   
        }
        
        public V[] Datas { get { return _datas; } }
        
        public V GetData(int id)
        {
            if (_datas == null)
                return null;

            if (id <= 0)
                return null;
            
            foreach (var data in _datas)
            {
                if(data == null)
                    continue;

                if (data.Id == id)
                    return data;
            }

            return null;
        }
    }
}

