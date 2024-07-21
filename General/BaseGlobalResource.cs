using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{
    public abstract class BaseGlobalResource : ScriptableObject
    {

    }

    public abstract class BaseGlobalResource<TClass> : BaseGlobalResource where TClass : BaseGlobalResource<TClass>
    {
        public static TClass Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load("Global Resource") as TClass;
                }

                return _instance;
            }
        }

        protected static TClass _instance;


    }
}