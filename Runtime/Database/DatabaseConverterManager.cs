using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MioHelper;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace MioHelper
{
    public class DatabaseConverterManager : MonoBehaviour
    {
        [ContextMenu("Refresh Database and Generate DB")]
        public void RefreshAndGenerateDatabase()
        {
            EditorCoroutines.StartCoroutine(_RefreshAndGenerateDatabaseCoroutine(), this);
        }

        private IEnumerator _RefreshAndGenerateDatabaseCoroutine()
        {
            yield return EditorCoroutines.StartCoroutine(MioFetchCore.Instance._StartRefresh(), this);
            GenerateDatabase();
        }

        [ContextMenu("Generate DB")]
        public void GenerateDatabase()
        {
            ItemDBConverter[] converters = GetComponents<ItemDBConverter>();

            // Get the path to database first
            List<TextAsset> databases = new();
            int found = DatabaseAssetLoader.TryGetUnityObjectsOfTypeFromPath<TextAsset>("Assets/_Database/", databases);

            Debug.Log($"Found {found} Database");
            bool requireRecompile = false;
            foreach (ItemDBConverter converter in converters)
            {
                if (converter.enabled == false) continue;
                converter.GenerateDatabase(databases);
                if (converter.IsRecompileRequired()) requireRecompile = true;
            }

            if (requireRecompile) UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }
    }


}



#endif