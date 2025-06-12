using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Event
{
    // public interface IEventHandler<T> where T : IEventHandler<T>.Data
    public abstract class EventData
    {
        // public class Data
    }
    
    // public interface IEventHandler<T> where T : EventData
    // {
    //     void OnChanged(T t);
    // }
    
    public class EventHandler
    {
        private readonly static Dictionary<Type, Delegate> _eventHandlers = new();

        public static void Add<T>(Action<T> action) where T : Event.EventData
        {
            // Debug.Log(typeof(T));
            
            if (_eventHandlers.TryGetValue(typeof(T), out var handler))
                _eventHandlers[typeof(T)] = (Action<T>)handler + action;
            else
                _eventHandlers[typeof(T)] = action;
            // if (_eventHandlers.ContainsKey(typeof(T)))
            //      _eventHandlers[typeof(T)] = (Action<T>)existing + handler;
            // else
            //     _eventHandlers.Add(typeof(T), handler);
        }

        public static void Remove<T>(Action<T> action) where T : Event.EventData
        {
            // lock (_lockObj)
            {
                if (_eventHandlers.TryGetValue(typeof(T), out var handler))
                {
                    var updated = (Action<T>)handler - action;
                    if (updated == null)
                        _eventHandlers.Remove(typeof(T));
                    else
                        _eventHandlers[typeof(T)] = updated;
                }
            }
        }

        public static void Notify<T>(T eventData) where T : Event.EventData
        {
            if (eventData == null) 
                return;

            if (_eventHandlers.TryGetValue(typeof(T), out var handler))
                ((Action<T>)handler)?.Invoke(eventData);
        }
    }
    
    // public class EventHandler<T, V> where T : Object where V : EventData
    // {
    //     private static Dictionary<System.Type, List<IEventHandler<V>>> _iEventHandlerDic = null;
    //     
    //     public static void Add(IEventHandler<V> iEventHandler)
    //     {
    //         if (_iEventHandlerDic == null)
    //         {
    //             _iEventHandlerDic = new Dictionary<System.Type, List<IEventHandler<V>>>();
    //             _iEventHandlerDic.Clear();
    //         }
    //         
    //         List<IEventHandler<V>> eventHadlerList = null;
    //         if (_iEventHandlerDic.TryGetValue(typeof(T), out eventHadlerList))
    //         {
    //             for (int i = 0; i < eventHadlerList?.Count; ++i)
    //             {
    //                 if(eventHadlerList[i] == null)
    //                     continue;   
    //                 if (eventHadlerList[i] == iEventHandler)
    //                     return;
    //             }
    //         }
    //         else
    //         {
    //             eventHadlerList = new();
    //             eventHadlerList.Clear();
    //         }
    //         eventHadlerList?.Add(iEventHandler);
    //         _iEventHandlerDic?.TryAdd(typeof(T), eventHadlerList);
    //     }
    //     
    //     public static void Remove(IEventHandler<V> iEventHandler)
    //     {
    //         if (_iEventHandlerDic == null)
    //             return;
    //         if (_iEventHandlerDic.TryGetValue(typeof(T), out List<IEventHandler<V>> eventHadlerList))
    //         {
    //             for (int i = 0; i < eventHadlerList?.Count; ++i)
    //             {
    //                 if(eventHadlerList[i] == null)
    //                     continue;
    //                 if (eventHadlerList[i] == iEventHandler)
    //                 {
    //                     _iEventHandlerDic[typeof(T)]?.RemoveAt(i);
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    //     
    //     public static void Notify(V eventData = null)
    //     {
    //         if (_iEventHandlerDic == null)
    //             return;
    //         if (_iEventHandlerDic.TryGetValue(typeof(T), out List<IEventHandler<V>> eventHadlerList))
    //         {
    //             for (int i = 0; i < eventHadlerList?.Count; ++i)
    //             {
    //                 if(eventHadlerList[i] == null)
    //                     continue;
    //                 eventHadlerList[i]?.OnChanged(eventData);
    //             }
    //         }
    //     }
    // }
}
