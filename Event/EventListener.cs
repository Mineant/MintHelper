using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Mineant;
// using CrystalFramework.Common;

namespace Mineant.Event
{
    /// <summary>
    /// 所有Touch类型的枚举
    /// </summary>
    public enum E_TouchType : byte
    {
        OnClick,
        OnDoubleClick,
        OnDown,
        OnUp,
        OnEnter,
        OnExit,
        OnSelect,
        OnUpdateSelect,
        OnDeSelect,
        OnDragBegin,
        OnDrag,
        OnDragEnd,
        OnDrop,
        OnScroll,
        OnMove,
    }
    /// <summary>
    /// 触摸委托
    /// </summary>
    /// <param name="_listener">监听器对象</param>
    /// <param name="_args">参数</param>
    /// <param name="_params">参数数组</param>
    public delegate void OnTouchHandle(GameObject _listener, object _args, params object[] _params);
    /// <summary>
    /// 触摸句柄，定义了触摸的相关属性
    /// </summary>
    public class TouchHandle
    {
        /// <summary>
        /// 触摸枚举类型
        /// </summary>
        public E_TouchType TouchType;
        /// <summary>
        /// 事件句柄对象
        /// </summary>
        private event OnTouchHandle eventHandle = null;
        /// <summary>
        /// 句柄参数
        /// </summary>
        private object[] handleParams;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_handle">委托</param>
        /// <param name="_params">参数数组对象</param>
        public TouchHandle(OnTouchHandle _handle, params object[] _params)
        {
            SetHandle(_handle, _params);
        }

        public TouchHandle()
        {

        }
        /// <summary>
        /// 设置句柄
        /// </summary>
        /// <param name="_handle">委托句柄</param>
        /// <param name="_params">参数数组对象</param>
        public void SetHandle(OnTouchHandle _handle, params object[] _params)
        {
            DestoryHandle();
            eventHandle += _handle;
            handleParams = _params;
        }
        /// <summary>
        /// 调用事件句柄
        /// </summary>
        /// <param name="_lsitener">监听对象</param>
        /// <param name="_args">对象参数</param>
        public void CallEventHandle(GameObject _lsitener, object _args)
        {
            if (null != eventHandle)
            {
                eventHandle(_lsitener, _args, handleParams);
            }
        }
        /// <summary>
        /// 删除句柄
        /// </summary>
        public void DestoryHandle()
        {
            if (null != eventHandle)
            {
                eventHandle -= eventHandle;
                eventHandle = null;
            }
        }

    }
    /// <summary>
    /// 事件监听器，将事件监听归类成一块
    /// </summary>
    public class EventListener : MonoBehaviour,
    #region interface
        IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler,

    ISelectHandler,
    IUpdateSelectedHandler,
    IDeselectHandler,

    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler,
    IDropHandler,
    IScrollHandler,
    IMoveHandler
    #endregion
    {
        /// <summary>
        /// 句柄字典
        /// </summary>
        public Dictionary<E_TouchType, TouchHandle> dicHandles = new Dictionary<E_TouchType, TouchHandle>();

        /// <summary>
        /// 获取参数对象的EventListener控件
        /// </summary>
        /// <param name="go">指定的GameObject对象</param>
        /// <returns>EventListener控件对象</returns>
        static public EventListener Get(GameObject go)
        {
            return Helpers.SmartGetComponent<EventListener>(go);

        }
        /// <summary>
        /// 获取参数对象的EventListener控件
        /// </summary>
        /// <param name="tran">指定的Transform对象</param>
        /// <returns>EventListener控件对象</returns>
        static public EventListener Get(Transform tran)
        {
            return Helpers.SmartGetComponent<EventListener>(tran.gameObject);
        }
        /// <summary>
        /// 获取参数对象的EventListener控件
        /// </summary>
        /// <param name="btn">指定的Button对象</param>
        /// <returns>EventListener控件对象</returns>
        static public EventListener Get(Button btn)
        {
            return Helpers.SmartGetComponent<EventListener>(btn.gameObject);
        }

        void OnDestory()
        {
            this.RemoveAllHandle();
        }

        private void RemoveAllHandle()
        {
            foreach (var item in dicHandles)
            {
                item.Value.DestoryHandle();
            }
            dicHandles.Clear();
        }

        #region Handler 实现 (根据E_TouchType的类型自动调用下列函数)

        #region IDragHandler 实现

        /// <summary>
        /// 开始拖拽
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnDragBegin);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        /// <summary>
        /// 拖拽中
        /// </summary>
        /// <param name="eventData">PointerEventData对象</param>
        public void OnDrag(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnDrag);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IEndDragHandler 实现
        /// <summary>
        /// 结束的拖拽方法
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnDragEnd);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IDropHandler 实现
        /// <summary>
        /// 拖拽的物体拖拽结束停留在实现IDropHandler的物体上时调用此函数，即a物体拖拽放到b物体身上的时候b会激活这个函数。多用于交换，判断b已经坚持到有人在它身上
        /// </summary>
        public void OnDrop(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnDrop);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IPointerClickHandler 实现

        public void OnPointerClick(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnClick);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IPointerDownHandler 实现

        public void OnPointerDown(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnDown);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IPointerUpHandler 实现

        public void OnPointerUp(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnUp);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }
        #endregion

        #region IPointerEnterHandler 实现

        public void OnPointerEnter(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnEnter);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IPointerExitHandler 实现

        public void OnPointerExit(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnExit);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region ISelectHandler 实现

        public void OnSelect(BaseEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnSelect);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IUpdateSelectedHandler 实现

        public void OnUpdateSelected(BaseEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnUpdateSelect);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IDeselectHandler 实现

        public void OnDeselect(BaseEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnDeSelect);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IScrollHandler 实现

        public void OnScroll(PointerEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnScroll);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #region IMoveHandler 实现

        public void OnMove(AxisEventData eventData)
        {
            TouchHandle handle = GetHandle(E_TouchType.OnMove);
            if (handle != null)
            {
                handle.CallEventHandle(this.gameObject, eventData);
            }
        }

        #endregion

        #endregion
        /// <summary>
        /// 获取句柄
        /// </summary>
        /// <param name="type">触摸的类型</param>
        /// <returns></returns>
        public TouchHandle GetHandle(E_TouchType type)
        {
            TouchHandle handle;
            if (dicHandles.TryGetValue(type, out handle))
            {
                return handle;
            }
            return null;
        }
        /// <summary>
        /// 设置监听事件
        /// </summary>
        /// <param name="_type">触摸的类型</param>
        /// <param name="_handle">委托句柄</param>
        /// <param name="_params">参数数组</param>
        public void SetEventListener(E_TouchType _type, OnTouchHandle _handle, params object[] _params)
        {
            TouchHandle handle = GetHandle(_type);
            if (handle == null)
            {
                handle = new TouchHandle();
                dicHandles.Add(_type, handle);
            }
            dicHandles[_type].TouchType = _type;
            dicHandles[_type].SetHandle(_handle, _params);
        }
    }

}