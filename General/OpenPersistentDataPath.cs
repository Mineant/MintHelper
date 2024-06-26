#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace Mineant
{
    public static class OpenPersistentDataPath
    {
        [MenuItem("Mineant/Open Persistent Data Path")]
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