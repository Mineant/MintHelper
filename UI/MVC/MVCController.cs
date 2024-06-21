using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{
    public abstract class MVCController<Data> : MonoBehaviour where Data : class
    {
        // Update data, the data is passed as a parameter
        [SerializeField]
        protected Data _data;

        protected virtual void Awake()
        {
            ListenToData();
        }

        public abstract void ListenToData();

        public virtual void UpdateView()
        {
            
        }
    }

}
