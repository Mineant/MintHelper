using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MioHelper
{

    public enum FileType { txt, csv, json, xml, jpg, png, bytes }
    [System.Serializable]
    public class MioFetchPath : TreeElement
    {
        public bool enabled = true;
        public FileType fileType;
        public string url, filePath, lastFileHash, label;
        public Object assetReference;

        public MioFetchPath(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }

    public class MioFetchSettings : ScriptableObject
    {
        public List<MioFetchPath> paths = new List<MioFetchPath>();


        public bool autoRefresh = false;
        public double refreshInterval = 60.0;

        #region Singleton Behaviour
        public static bool EnableMioFetch = false;
        private static MioFetchSettings instance;
        public static MioFetchSettings Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                // attempt to get instance from disk. If already exist, if miofetch is not enabled, then will enable
                var possibleTempData = AssetDatabase.LoadAssetAtPath<MioFetchSettings>(GetSettingsFilePath());
                if (possibleTempData != null)
                {
                    if (!EnableMioFetch) EnableMioFetch = true;
                    instance = possibleTempData;
                    return instance;
                }

                // Mine
                if (!EnableMioFetch) return null;

                // no instance exists, create a new instance.
                instance = CreateInstance<MioFetchSettings>();
                instance.paths.Add(new MioFetchPath("root", -1, 0));
                AssetDatabase.CreateAsset(instance, GetSettingsFilePath());
                AssetDatabase.SaveAssets();
                return instance;
            }
        }

        #endregion

        /// <summary>
        /// Returns the path of the MioFetch folder, based on the location of MioFetchCore.cs since that should always be in there.
        /// </summary>
        public static string LocateMioFetchFolder()
        {
            string[] results = Directory.GetFiles(Application.dataPath, "MioFetchCore.cs", SearchOption.AllDirectories);
            if (results.Length > 0)
            {
                var parent = Directory.GetParent(results[0]);
                while (parent.Name != "MioFetch")
                    parent = parent.Parent;

                return parent.FullName;
            }
            else
            {
                var directory = Directory.CreateDirectory($"{Application.dataPath}/MioFetch");
                Directory.CreateDirectory($"{Application.dataPath}/MioFetch/Editor");

                return directory.FullName;
            }
        }

        /// <summary>
        /// Returns the path MioFetch settings data should live.
        /// </summary>
        public static string GetSettingsFilePath()
        {
            var path = LocateMioFetchFolder(); //find folder in project...
            path += "\\Editor\\MioFetchSettings.asset"; //append on the path for temp data;
            Debug.Log($"{path}");
            path = path.Substring(path.IndexOf("Assets")); //remove path before the assets folder
            return (path);
        }

    }

}
