
using System;
using System.Collections.Generic;

namespace Mineant.Event
{
    /// <summary>
    /// 事件分发器
    /// 提供事件注册， 反注册， 事件触发
    /// 不足处：调用时候没有检测参数的机制，调用形参传递不妥运行报错降低开发效率
    /// 采用 delegate, dictionary 实现(其实就是整合委托的机制)
    /// 支持自定义事件。 事件采用字符串方式标识
    /// 支持 0，1，2，3 等4种不同参数个数的回调函数
    /// 使用方法：
    /// Events里面添加相应的标识，然后在适当地方添加AddListener、RemoveListener
    /// 用于初始化和释放资源，然后在需要的地方调用TriggerEvent激活事件即可
    /// 注册的事件类型适合单独并且不重复使用的，例如角色受伤可以是npc可以是玩家或者敌人，这时候如果用此事件路由
    /// 需要在Events里添加多个不同类型的，这样还不如在BaseCharacter基类写一个抽象的受伤Action
    /// </summary>
    public class EventDispatcher
    {
        private static EventController _eventController = new EventController();
        /// <summary>
        /// 事件路由器
        /// </summary>
        public static Dictionary<string, Delegate> TheRouter
        {
			get { return _eventController.TheRouter; }
        }

        /// <summary>
        /// 标记为永久注册事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        static public void MarkAsPermanent(string eventType)
        {
			_eventController.MarkAsPermanent(eventType);
        }

        /// <summary>
        /// 清除非永久性注册的事件
        /// </summary>
        static public void Cleanup()
        {
			_eventController.Cleanup();
        }

        #region 增加监听器
        /// <summary>
        /// 增加监听器， 不带参数
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件委托</param>
        static public void AddListener(string eventType, Action handler)
        {
			_eventController.AddListener(eventType, handler);
        }

        /// <summary>
        /// 增加监听器， 1个参数
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件委托</param>
        /*调用：
         *  EventDispatcher.AddListener<int>(Events.LoadingEvent.OnLoadingProcess, OnLoading);
         *  OnLoading函数的定义
         *  void OnLoading(int process){ do something... }
         */
        static public void AddListener<T>(string eventType, Action<T> handler)
        {
			_eventController.AddListener(eventType, handler);
        }

        /// <summary>
        /// 增加监听器， 2个参数
        /// </summary>
        /// <typeparam name="T">泛型参数一</typeparam>
        /// <typeparam name="U">泛型参数二</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件委托</param>
        /*调用：
         *  EventDispatcher.AddListener<int, int>(Events.LoadingEvent.OnLoadingProcess, OnLoading);
         *  OnLoading函数的定义
         *  void OnLoading(int process, int tmp){ do something... }
         */
        static public void AddListener<T, U>(string eventType, Action<T, U> handler)
        {
			_eventController.AddListener(eventType, handler);
        }

        /// <summary>
        /// 增加监听器， 3个参数
        /// </summary>
        /// <typeparam name="T">泛型参数一</typeparam>
        /// <typeparam name="U">泛型参数二</typeparam>
        /// <typeparam name="V">泛型参数三</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件委托</param>
        /*调用：
         *  EventDispatcher.AddListener<int, int , int>(Events.LoadingEvent.OnLoadingProcess, OnLoading);
         *  OnLoading函数的定义
         *  void OnLoading(int process, int tmp, int tmp2){ do something... }
         */
        static public void AddListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
			_eventController.AddListener(eventType, handler);
        }

        /// <summary>
        /// 增加监听器， 4个参数
        /// </summary>
        /// <typeparam name="T">泛型参数一</typeparam>
        /// <typeparam name="U">泛型参数二</typeparam>
        /// <typeparam name="V">泛型参数三</typeparam>
        /// <typeparam name="W">泛型参数四</typeparam>
        /// <param name="eventType">事件类型</param>
        /// <param name="handler">事件委托</param>
        /*调用：
         *  EventDispatcher.AddListener<int, float, int, int>(Events.LoadingEvent.OnLoadingProcess, OnLoading);
         *  OnLoading函数的定义
         *  void OnLoading(int tmp1, float tmp2, int tmp3, int mpt4){ do something... }
         */
        static public void AddListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
			_eventController.AddListener(eventType, handler);
        }
        #endregion

        #region 移除监听器
        /// <summary>
        ///  移除监听器， 不带参数
        /// </summary>
        static public void RemoveListener(string eventType, Action handler)
        {
			_eventController.RemoveListener(eventType, handler);
        }

        /// <summary>
        ///  移除监听器， 1个参数，例子请看AddListener带四个参数的
        /// </summary>
        static public void RemoveListener<T>(string eventType, Action<T> handler)
        {
			_eventController.RemoveListener(eventType, handler);
        }

        /// <summary>
        ///  移除监听器， 2个参数，例子请看AddListener带四个参数的
        /// </summary>
        static public void RemoveListener<T, U>(string eventType, Action<T, U> handler)
        {
			_eventController.RemoveListener(eventType, handler);
        }

        /// <summary>
        ///  移除监听器， 3个参数，例子请看AddListener带四个参数的
        /// </summary>
        static public void RemoveListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
			_eventController.RemoveListener(eventType, handler);
        }

        /// <summary>
        ///  移除监听器， 4个参数，例子请看AddListener带四个参数的
        /// </summary>
        static public void RemoveListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
			_eventController.RemoveListener(eventType, handler);
        }
        #endregion

        #region 触发事件
        /// <summary>
        ///  触发事件， 不带参数触发
        /// </summary>
        static public void TriggerEvent(string eventType)
        {
			_eventController.TriggerEvent(eventType);
        }

        /// <summary>
        ///  触发事件， 带1个参数触发，参数可发送给外部调用程序用
        /// </summary>
        static public void TriggerEvent<T>(string eventType, T arg1)
        {
			_eventController.TriggerEvent(eventType, arg1);
        }

        /// <summary>
        ///  触发事件， 带2个参数触发，参数可发送给外部调用程序用
        /// </summary>
        static public void TriggerEvent<T, U>(string eventType, T arg1, U arg2)
        {
			_eventController.TriggerEvent(eventType, arg1, arg2);
        }

        /// <summary>
        ///  触发事件， 带3个参数触发，参数可发送给外部调用程序用
        /// </summary>
        static public void TriggerEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
			_eventController.TriggerEvent(eventType, arg1, arg2, arg3);
        }

        /// <summary>
        ///  触发事件， 带4个参数触发，参数可发送给外部调用程序用
        /// </summary>
        static public void TriggerEvent<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
			_eventController.TriggerEvent(eventType, arg1, arg2, arg3, arg4);
        }

        #endregion
    }
}