using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Mineant
{

    public enum FileType { txt, csv, json, xml, jpg, png, bytes }
    [System.Serializable]
    public class BobbinPath : TreeElement
    {
        public bool enabled = true;
        public FileType fileType;
        public string url, filePath, lastFileHash, label;
        public Object assetReference;

        public BobbinPath(string name, int depth, int id) : base(name, depth, id)
        {

        }
    }

    public class BobbinSettings : ScriptableObject
    {
        public List<BobbinPath> paths = new List<BobbinPath>();


        public bool autoRefresh = false;
        public double refreshInterval = 60.0;

        #region Singleton Behaviour
        public static bool EnableBobbin = false;
        private static BobbinSettings instance;
        public static BobbinSettings Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                // attempt to get instance from disk. If already exist, if bobbin is not enabled, then will enable
                var possibleTempData = AssetDatabase.LoadAssetAtPath<BobbinSettings>(GetSettingsFilePath());
                if (possibleTempData != null)
                {
                    if (!EnableBobbin) EnableBobbin = true;
                    instance = possibleTempData;
                    return instance;
                }

                // Mine
                if (!EnableBobbin) return null;

                // no instance exists, create a new instance.
                instance = CreateInstance<BobbinSettings>();
                instance.paths.Add(new BobbinPath("root", -1, 0));
                AssetDatabase.CreateAsset(instance, GetSettingsFilePath());
                AssetDatabase.SaveAssets();
                return instance;
            }
        }

        #endregion

        // pasted in from https://github.com/radiatoryang/merino/commit/6bbc24f4a50262b32d99c417f37e371eb8741ece ... thanks @charblar
        /// <summary>
        /// Returns the path of the Bobbin folder, based on the location of BobbinWindow.cs since that should always be in there.
        /// </summary>
        public static string LocateBobbinFolder()
        {
            string[] results = Directory.GetFiles(Application.dataPath, "BobbinCore.cs", SearchOption.AllDirectories);
            if (results.Length > 0)
            {
                var parent = Directory.GetParent(results[0]);
                while (parent.Name != "Bobbin")
                    parent = parent.Parent;

                return parent.FullName;
            }
            else
            {
                var directory = Directory.CreateDirectory($"{Application.dataPath}/Bobbin");
                Directory.CreateDirectory($"{Application.dataPath}/Bobbin/Editor");

                return directory.FullName;
            }
        }

        /// <summary>
        /// Returns the path Bobbin settings data should live.
        /// </summary>
        public static string GetSettingsFilePath()
        {
            var path = LocateBobbinFolder(); //find folder in project...
            path += "\\Editor\\BobbinSettings.asset"; //append on the path for temp data;
            Debug.Log($"{path}");
            path = path.Substring(path.IndexOf("Assets")); //remove path before the assets folder
            return (path);
        }

    }

}
