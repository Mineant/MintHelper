using UnityEngine;

namespace Mineant
{

    public static class MobileHelpers
    {
        /// <summary>
        /// Returns the keyboard height ratio.
        /// </summary>
        public static float GetKeyboardHeightRatio(bool includeInput)
        {
            return Mathf.Clamp01((float)GetKeyboardHeight(includeInput) / Display.main.systemHeight);
        }

        /// <summary>
        /// Returns the keyboard height in display pixels.
        /// </summary>
        public static int GetKeyboardHeight(bool includeInput)
        {
#if UNITY_ANDROID
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
            var view = unityPlayer.Call<AndroidJavaObject>("getView");
            var dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
 
            if (view == null || dialog == null)
                return 0;
 
            var decorHeight = 0;
 
            if (includeInput)
            {
                var decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
 
                if (decorView != null)
                    decorHeight = decorView.Call<int>("getHeight");
            }
 
            using (var rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                view.Call("getWindowVisibleDisplayFrame", rect);
                return Display.main.systemHeight - rect.Call<int>("height") + decorHeight;
            }
        }
#else
            var height = Mathf.RoundToInt(TouchScreenKeyboard.area.height);
            return height >= Display.main.systemHeight ? 0 : height;
#endif
        }
    }
}