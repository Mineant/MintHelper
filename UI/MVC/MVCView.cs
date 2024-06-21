using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{
    public class MVCView<TController, TData> : MonoBehaviour where TController : MVCController<TData> where TData : class
    {
        // User interact with the view
        // The view can send ui events to the controller, and ask for the data needed.
        // Similar to enhance scroller, when interacting with the scroller, the controller will provide the data needed.
        // The view should not find its owner, the controller should provide it. This way, we could easily bind or unbind the views.

        protected TController _controller;

        protected virtual void Awake()
        {

        }

        public virtual void SetController(TController controller)
        {
            _controller = controller;
        }

        public void UpdateView(TData data)
        {

        }
    }

}
