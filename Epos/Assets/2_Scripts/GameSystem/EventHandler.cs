using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    // public interface IEventHandler<T> where T : IEventHandler<T>.Data
    public class EventData
    {
        // public class Data
    }
    
    public interface IEventHandler<T> where T : EventData
    {
        void OnChanged(T t);
    }
    
    public class EventHandler<T, V> where T : Object where V : EventData
    {
        private static Dictionary<System.Type, List<IEventHandler<V>>> _iEventHandlerDic = null;
        
        public static void Add(IEventHandler<V> iEventHandler)
        {
            if (_iEventHandlerDic == null)
            {
                _iEventHandlerDic = new Dictionary<System.Type, List<IEventHandler<V>>>();
                _iEventHandlerDic.Clear();
            }
            List<IEventHandler<V>> eventHadlerList = null;
            if (_iEventHandlerDic.TryGetValue(typeof(T), out eventHadlerList))
            {
                for (int i = 0; i < eventHadlerList?.Count; ++i)
                {
                    if(eventHadlerList[i] == null)
                        continue;   
                    if (eventHadlerList[i] == iEventHandler)
                        return;
                }
            }
            else
            {
                eventHadlerList = new();
                eventHadlerList.Clear();
            }
            eventHadlerList?.Add(iEventHandler);
            _iEventHandlerDic?.TryAdd(typeof(T), eventHadlerList);
        }
        
        public static void Remove(IEventHandler<V> iEventHandler)
        {
            if (_iEventHandlerDic == null)
                return;
            if (_iEventHandlerDic.TryGetValue(typeof(T), out List<IEventHandler<V>> eventHadlerList))
            {
                for (int i = 0; i < eventHadlerList?.Count; ++i)
                {
                    if(eventHadlerList[i] == null)
                        continue;
                    if (eventHadlerList[i] == iEventHandler)
                    {
                        _iEventHandlerDic[typeof(T)]?.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        
        public static void Notify(V eventData = null)
        {
            if (_iEventHandlerDic == null)
                return;
            if (_iEventHandlerDic.TryGetValue(typeof(T), out List<IEventHandler<V>> eventHadlerList))
            {
                for (int i = 0; i < eventHadlerList?.Count; ++i)
                {
                    if(eventHadlerList[i] == null)
                        continue;
                    eventHadlerList[i]?.OnChanged(eventData);
                }
            }
        }
    }
}
