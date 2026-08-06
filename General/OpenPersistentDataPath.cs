#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace MioHelper
{
    public static class OpenPersistentDataPath
    {
        [MenuItem("MioHelper/Open Persistent Data Path")]
        public static void OpenPersistentDataPaths()
        {
            Process process = new Process();
            process.StartInfo.FileName = ((Application.platform == RuntimePlatform.WindowsEditor) ? "explorer.exe" : "open");
            process.StartInfo.Arguments = "file://" + Application.persistentDataPath;
            process.Start();
        }
    }
}
#endif