using UnityEngine;

namespace MioHelper
{
    /// <summary>
    /// 持久化单例：跨场景加载时不会被销毁（DontDestroyOnLoad）。
    /// 用法：public class MyClass : PersistentSingleton&lt;MyClass&gt;
    /// </summary>
    public class PersistentSingleton<T> : Singleton<T> where T : PersistentSingleton<T>
    {
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
                DontDestroyOnLoad(gameObject);
        }
    }
}
