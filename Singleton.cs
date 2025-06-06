using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T m_Instance;

        /// <summary>
        /// 单例对象
        /// </summary>
        public static T Instance
        {
            get => m_Instance;
        }

        protected virtual void Awake()
        {
            if (m_Instance != null)
                Destroy(gameObject);
            else
            {
                m_Instance = (T)this;
                AwakeSingleton();
            }
        }

        /// <summary>
        /// This is called only once when the singleton is initialized. Will not call on duplicated singletons.
        /// </summary>
        protected virtual void AwakeSingleton()
        {

        }

        protected virtual void OnDestroy()
        {
            if (m_Instance == this)
                m_Instance = null;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Init() { }
    }
}