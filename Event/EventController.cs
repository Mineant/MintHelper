//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_BROADCAST_MESSAGE
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.Event
{
    /// <summary>
    /// 事件控制器类，处理事件逻辑
    /// </summary>
    public class EventController
    {
        /// <summary>
        /// 事件路由器
        /// </summary>
        private Dictionary<string, Delegate> m_theRouter = new Dictionary<string, Delegate>();

        /// <summary>
        /// 事件路由器
        /// </summary>
        public Dictionary<string, Delegate> TheRouter
        {
            get { return m_theRouter; }
        }

        /// <summary>
        /// 永久注册的事件列表
        /// </summary>
        private List<string> m_permanentEvents = new List<string>();

        /// <summary>
        /// 标记为永久注册事件
        /// </summary>
        /// <param name="eventType"></param>
        public void MarkAsPermanent(string eventType)
        {
#if LOG_ALL_MESSAGES
			DebugUtil.Info("Messenger MarkAsPermanent \t\"" + eventType + "\"");
#endif
            m_permanentEvents.Add(eventType);
        }

        /// <summary>
        /// 判断是否已经包含事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <returns>包含事件返回true</returns>
        public bool ContainsEvent(string eventType)
        {
            return m_theRouter.ContainsKey(eventType);
        }

        /// <summary>
        /// 清除非永久性注册的事件
        /// </summary>
        public void Cleanup()
        {
#if LOG_ALL_MESSAGES
			DebugUtil.Info("MESSENGER Cleanup. Make sure that none of necessary listeners are removed.");
#endif
            List<string> eventToRemove = new List<string>();// 用于清除的非永久注册事件列表（待清除列表）
            // 遍历事件路由器，将匹配非永久注册事件的键添加到清除列表中
            foreach (KeyValuePair<string, Delegate> pair in m_theRouter)
            {
                bool wasFound = false;
                foreach (string Event in m_permanentEvents)
                {
                    if (pair.Key == Event)// 是永久事件的不进行删除，跳出本次循环
                    {
                        wasFound = true;
                        break;
                    }
                }

                if (!wasFound)
                    eventToRemove.Add(pair.Key);
            }
            // 删除带清除列表
            foreach (string Event in eventToRemove)
            {
                m_theRouter.Remove(Event);
            }
        }

        /// <summary>
        /// 处理增加监听器前的事项， 检查 参数等
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="listenerBeingAdded">将要添加监听的委托</param>
        private void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
        {
#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
			DebugUtil.Info("MESSENGER OnListenerAdding \t\"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
#endif

            if (!m_theRouter.ContainsKey(eventType))
            {
                m_theRouter.Add(eventType, null);
            }
            // 检查如果将要添加的已存字典并且类型和将要添加的不符，抛出一个异常
            Delegate d = m_theRouter[eventType];
            if (d != null && d.GetType() != listenerBeingAdded.GetType())
            {
                throw new Exception(string.Format(
                    "Try to add not correct event {0}. Current type is {1}, adding type is {2}.",
                    eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
            }
        }

        /// <summary>
        /// 移除监听器之前的检查
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="listenerBeingRemoved">将要移除的委托</param>
        private bool OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
        {
#if LOG_ALL_MESSAGES
			DebugUtil.Info("MESSENGER OnListenerRemoving \t\"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
#endif
            // 不存在字典中，不需要移除
            if (!m_theRouter.ContainsKey(eventType))
            {
                return false;
            }
            // 如果存在字典中但和将要移除的事件类型不一致抛异常
            Delegate d = m_theRouter[eventType];
            if ((d != null) && (d.GetType() != listenerBeingRemoved.GetType()))
            {
                throw new Exception(string.Format(
                    "Remove listener {0}\" failed, Current type is {1}, adding type is {2}.",
                    eventType, d.GetType(), listenerBeingRemoved.GetType()));
            }
            else
                return true;
        }

        /// <summary>
        /// 移除监听器之后的处理。删掉事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        private void OnListenerRemoved(string eventType)
        {
            if (m_theRouter.ContainsKey(eventType) && m_theRouter[eventType] == null)
            {
                m_theRouter.Remove(eventType);
            }
        }

        #region 增加监听器
        /// <summary>
        ///  增加监听器， 不带参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">监听器事件</param>
        public void AddListener(string eventType, Action handler)
        {
            OnListenerAdding(eventType, handler);
            // 为指定事件类型添加事件体（为字典赋值委托）
            m_theRouter[eventType] = (Action)m_theRouter[eventType] + handler;
        }

        /// <summary>
        ///  增加监听器， 1个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void AddListener<T>(string eventType, Action<T> handler)
        {
            OnListenerAdding(eventType, handler);
            // 为指定事件类型添加事件体（为字典赋值委托）
            m_theRouter[eventType] = (Action<T>)m_theRouter[eventType] + handler;
        }

        /// <summary>
        ///  增加监听器， 2个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void AddListener<T, U>(string eventType, Action<T, U> handler)
        {
            OnListenerAdding(eventType, handler);
            // 为指定事件类型添加事件体（为字典赋值委托）
            m_theRouter[eventType] = (Action<T, U>)m_theRouter[eventType] + handler;
        }

        /// <summary>
        ///  增加监听器， 3个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void AddListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            OnListenerAdding(eventType, handler);
            // 为指定事件类型添加事件体（为字典赋值委托）
            m_theRouter[eventType] = (Action<T, U, V>)m_theRouter[eventType] + handler;
        }

        /// <summary>
        ///  增加监听器， 4个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void AddListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            OnListenerAdding(eventType, handler);
            // 为指定事件类型添加事件体（为字典赋值委托）
            m_theRouter[eventType] = (Action<T, U, V, W>)m_theRouter[eventType] + handler;
        }
        #endregion

        #region 移除监听器

        /// <summary>
        ///  移除监听器， 不带参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void RemoveListener(string eventType, Action handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                m_theRouter[eventType] = (Action)m_theRouter[eventType] - handler;
                OnListenerRemoved(eventType);
            }
        }

        /// <summary>
        ///  移除监听器， 1个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void RemoveListener<T>(string eventType, Action<T> handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                m_theRouter[eventType] = (Action<T>)m_theRouter[eventType] - handler;
                OnListenerRemoved(eventType);
            }
        }

        /// <summary>
        ///  移除监听器， 2个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void RemoveListener<T, U>(string eventType, Action<T, U> handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                m_theRouter[eventType] = (Action<T, U>)m_theRouter[eventType] - handler;
                OnListenerRemoved(eventType);
            }
        }

        /// <summary>
        ///  移除监听器， 3个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void RemoveListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                m_theRouter[eventType] = (Action<T, U, V>)m_theRouter[eventType] - handler;
                OnListenerRemoved(eventType);
            }
        }

        /// <summary>
        ///  移除监听器， 4个参数
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void RemoveListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            if (OnListenerRemoving(eventType, handler))
            {
                m_theRouter[eventType] = (Action<T, U, V, W>)m_theRouter[eventType] - handler;
                OnListenerRemoved(eventType);
            }
        }
        #endregion

        #region 触发事件
        /// <summary>
        ///  触发事件， 不带参数触发
        /// </summary>
        /// <param name="eventType">事件类型</param>
        public void TriggerEvent(string eventType)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
			DebugUtil.Info("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif

            Delegate d;// 将要获取的事件委托
            if (!m_theRouter.TryGetValue(eventType, out d))// 根据事件类型获取委托，失败返回
            {
                return;
            }
            // 获取委托调用列表
            var callbacks = d.GetInvocationList();
            for (int i = 0; i < callbacks.Length; i++)// 遍历调用列表
            {
                Action callback = callbacks[i] as Action;

                if (callback == null)
                {
                    throw new Exception(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                }
                callback();// 调用委托执行
                //try
                //{

                    
                //}
                //catch (Exception ex)
                //{
                //    Debug.Log(ex);
                //}
            }
        }

        /// <summary>
        ///  触发事件， 带1个参数触发
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="arg1">参数一</param>
        public void TriggerEvent<T>(string eventType, T arg1)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
			DebugUtil.Info("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            // 获取委托调用列表
            Delegate d;
            if (!m_theRouter.TryGetValue(eventType, out d))// 根据事件类型获取委托，失败返回
            {
                return;
            }
            // 获取委托调用列表
            var callbacks = d.GetInvocationList();// 遍历调用列表
            for (int i = 0; i < callbacks.Length; i++)
            {
                Action<T> callback = callbacks[i] as Action<T>;

                if (callback == null)
                {
                    throw new Exception(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                }

                try
                {
                    callback(arg1);// 调用委托执行
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
        }

        /// <summary>
        /// 触发事件， 带2个参数触发
        /// </summary>
        /// <typeparam name="T">泛型一</typeparam>
        /// <typeparam name="U">泛型二</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="arg1">参数一</param>
        /// <param name="arg2">参数二</param>
        public void TriggerEvent<T, U>(string eventType, T arg1, U arg2)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
			DebugUtil.Info("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            // 获取委托调用列表
            Delegate d;
            if (!m_theRouter.TryGetValue(eventType, out d))// 根据事件类型获取委托，失败返回
            {
                return;
            }
            // 获取委托调用列表
            var callbacks = d.GetInvocationList();// 遍历调用列表
            for (int i = 0; i < callbacks.Length; i++)
            {
                Action<T, U> callback = callbacks[i] as Action<T, U>;

                if (callback == null)
                {
                    throw new Exception(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                }

                try
                {
                    callback(arg1, arg2);// 调用委托执行
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
        }

        /// <summary>
        /// 触发事件， 带3个参数触发
        /// </summary>
        /// <typeparam name="T">泛型一</typeparam>
        /// <typeparam name="U">泛型二</typeparam>
        /// <typeparam name="V">泛型三</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="arg1">参数一</param>
        /// <param name="arg2">参数二</param>
        /// <param name="arg3">参数三</param>
        public void TriggerEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
			DebugUtil.Info("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            // 获取委托调用列表
            Delegate d;
            if (!m_theRouter.TryGetValue(eventType, out d))// 根据事件类型获取委托，失败返回
            {
                return;
            }
            // 获取委托调用列表
            var callbacks = d.GetInvocationList();// 遍历调用列表
            for (int i = 0; i < callbacks.Length; i++)
            {
                Action<T, U, V> callback = callbacks[i] as Action<T, U, V>;

                if (callback == null)
                {
                    throw new Exception(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                }
                try
                {
                    callback(arg1, arg2, arg3);// 调用委托执行
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log(ex);
                }
            }
        }

        /// <summary>
        /// 触发事件， 带4个参数触发
        /// </summary>
        /// <typeparam name="T">泛型一</typeparam>
        /// <typeparam name="U">泛型二</typeparam>
        /// <typeparam name="V">泛型三</typeparam>
        /// <typeparam name="W">泛型四</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="arg1">参数一</param>
        /// <param name="arg2">参数二</param>
        /// <param name="arg3">参数三</param>
        /// <param name="arg4">参数四</param>
        public void TriggerEvent<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
			DebugUtil.Info("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
            // 获取委托调用列表
            Delegate d;
            if (!m_theRouter.TryGetValue(eventType, out d))// 根据事件类型获取委托，失败返回
            {
                return;
            }
            // 获取委托调用列表
            var callbacks = d.GetInvocationList();// 遍历调用列表
            for (int i = 0; i < callbacks.Length; i++)
            {
                Action<T, U, V, W> callback = callbacks[i] as Action<T, U, V, W>;

                if (callback == null)
                {
                    throw new Exception(string.Format("TriggerEvent {0} error: types of parameters are not match.", eventType));
                }
                try
                {
                    callback(arg1, arg2, arg3, arg4);// 调用委托执行
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
        }

        #endregion
    }

}