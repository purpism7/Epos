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
        private static readonly Dictionary<Type, Delegate> EventHandlers = new();

        public static void Add<T>(Action<T> action) where T : Event.EventData
        {
            // Debug.Log(typeof(T));
            
            if (EventHandlers.TryGetValue(typeof(T), out var handler))
                EventHandlers[typeof(T)] = (Action<T>)handler + action;
            else
                EventHandlers[typeof(T)] = action;
            // if (_eventHandlers.ContainsKey(typeof(T)))
            //      _eventHandlers[typeof(T)] = (Action<T>)existing + handler;
            // else
            //     _eventHandlers.Add(typeof(T), handler);
        }

        public static void Remove<T>(Action<T> action) where T : Event.EventData
        {
            // lock (_lockObj)
            {
                if (EventHandlers.TryGetValue(typeof(T), out var handler))
                {
                    var updated = (Action<T>)handler - action;
                    if (updated == null)
                        EventHandlers.Remove(typeof(T));
                    else
                        EventHandlers[typeof(T)] = updated;
                }
            }
        }

        public static void Notify<T>(T eventData) where T : Event.EventData
        {
            if (eventData == null) 
                return;

            if (EventHandlers.TryGetValue(typeof(T), out var handler))
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
