using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mineant
{
    public abstract class ICommand : MonoBehaviour
    {
        public bool IsCompleted { get; protected set; }
        Action _completedAction;

        public ICommand()
        {
            IsCompleted = false;
        }

        public void OnComplete(Action action)
        {
            _completedAction = action;
        }

        /// <summary>
        /// the command must signal when it is completed. Call onComplete.Invoke(this) at the end. this is used because the system needs to be asynchronus 
        /// </summary>
        public abstract void Execute(Action<ICommand> onComplete);

        public void MarkAsComplete()
        {
            if (_completedAction != null) _completedAction.Invoke();
            IsCompleted = true;
        }
    }

}
