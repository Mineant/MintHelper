using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Mineant;
using Mineant.Inventory;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace MintHelper
{
    public class DatabaseConverterManager : MonoBehaviour
    {

        [ContextMenu("Generate DB")]
        public void GenerateDatabase()
        {
            ItemDBConverter[] converters = GetComponents<ItemDBConverter>();

            // Get the path to database first
            List<TextAsset> databases = new();
            int found = Helpers.TryGetUnityObjectsOfTypeFromPath<TextAsset>("Assets/_Database/", databases);

            Debug.Log($"Found {found} Database");
            foreach (ItemDBConverter converter in converters)
            {
                converter.GenerateDatabase(databases);
            }
        }
    }


    public static class DatabaseExtensions
    {
        public static List<float> ToFloatList(this string s, char seperator = ',', int defaultLength = -1, float defaultValue = Mathf.NegativeInfinity)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                List<float> floats = new List<float>();
                for (int i = 0; i < defaultLength; i++)
                {
                    floats.Add(defaultValue);
                }
                return floats;
            }
            return s.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => float.Parse(s)).ToList();
        }

        public static float ToFloat(this string s)
        {
            if (float.TryParse(s, out float result)) return result;
            return Mathf.NegativeInfinity;  // To prevent errors, see them immediately.
        }

        public static Vector2 ToVector2(this string s)
        {
            List<float> args = s.Split(',').Select(s => s.Trim().ToFloat()).ToList();
            return new Vector2(args[0], args[1]);
        }

        public static Vector2 ToVector3(this string s)
        {
            List<float> args = s.Split(',').Select(s => s.Trim().ToFloat()).ToList();
            return new Vector3(args[0], args[1], args[2]);
        }
    }
}



#endif