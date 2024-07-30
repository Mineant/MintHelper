using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Mineant.Inventory
{
    public class TestInventorySave : MonoBehaviour
    {
        public BaseInventory TargetInventory;

        public string LoadString;

        [ContextMenu("Save")]
        protected void Save()
        {
            JsonSerializerSettings settings = GetContentJsonSerializeSettings();
            LoadString = JsonConvert.SerializeObject(TargetInventory.GetContent(), settings);
            Debug.Log(LoadString);
        }



        [ContextMenu("Load")]
        protected void Load()
        {
            if (LoadString == "")
            {
                Debug.Log("No load string specified");
                return;
            }

            JsonSerializerSettings settings = GetContentJsonSerializeSettings();
            var content = JsonConvert.DeserializeObject<BaseGameItem[]>(LoadString, settings);
            // TargetInventory.(content);
            Debug.Log("Loaded " + content.Length + " items");
        }



        private static JsonSerializerSettings GetContentJsonSerializeSettings()
        {
            JsonSerializerSettings settings = new();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.TypeNameHandling = TypeNameHandling.Objects;
            settings.Formatting = Formatting.Indented;
            return settings;
        }
    }
}
