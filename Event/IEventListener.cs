namespace Mineant.Event
{
    /// <summary>
    /// 监听接口
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// 添加监听器
        /// </summary>
        void AddListener();

        /// <summary>
        /// 删除监听器
        /// </summary>
        void RemoveListener();

    }
}
