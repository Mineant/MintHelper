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
        [ContextMenu("Refresh Database and Generate DB")]
        public void RefreshAndGenerateDatabase()
        {
            StartCoroutine(_RefreshAndGenerateDatabaseCoroutine());
        }

        private IEnumerator _RefreshAndGenerateDatabaseCoroutine()
        {
            yield return BobbinCore.Instance._StartRefresh();
            GenerateDatabase();
        }

        [ContextMenu("Generate DB")]
        public void GenerateDatabase()
        {
            ItemDBConverter[] converters = GetComponents<ItemDBConverter>();

            // Get the path to database first
            List<TextAsset> databases = new();
            int found = Helpers.TryGetUnityObjectsOfTypeFromPath<TextAsset>("Assets/_Database/", databases);

            Debug.Log($"Found {found} Database");
            bool requireRecompile = false;
            foreach (ItemDBConverter converter in converters)
            {
                converter.GenerateDatabase(databases);
                if (converter.IsRecompileRequired()) requireRecompile = true;
            }

            if (requireRecompile) UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
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
            return s.Split(seperator).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => float.Parse(s)).ToList();
        }

        public static List<string> ToStringList(this string s, char seperator = ',', int defaultLength = -1, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                List<string> strings = new List<string>();
                for (int i = 0; i < defaultLength; i++)
                {
                    strings.Add(defaultValue);
                }
                return strings;
            }

            // Strings are special
            // use for loop to split string by seperator "," and stored them into a list
            List<string> splittedStrings = s.Split(seperator).Select(s => s.Trim()).ToList();
            List<string> newStrings = new List<string>();
            List<string> subList = new List<string>();
            for (int i = 0; i < splittedStrings.Count; i++)
            {
                string s1 = splittedStrings[i];

                // We detected a new list thingy
                if (subList.Count == 0 && s1[0] == '[')
                {
                    // we store this into subList
                    s1 = s1.Remove(0, 1);
                    subList.Add(s1);
                }
                else if (subList.Count > 0 && s1.Last() == ']')
                {
                    // we end the storing, and store the whole list into the new string
                    s1 = s1.Remove(s1.Length - 1, 1);
                    subList.Add(s1);
                    newStrings.Add(string.Join(",", subList));
                    subList.Clear();
                }
                else if (subList.Count > 0)
                {
                    // currently in array, store list into sub list
                    subList.Add(s1);
                }
                else
                {
                    // normal store
                    newStrings.Add(s1);
                }
            }

            if (subList.Count > 0) Debug.LogError("have not ] somewhere! String list cannot parse.");


            return newStrings;
        }

        public static List<int> ToIntList(this string s, char seperator = ',', int defaultLength = -1, int defaultValue = -9999999)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                List<int> ints = new List<int>();
                for (int i = 0; i < defaultLength; i++)
                {
                    ints.Add(defaultValue);
                }
                return ints;
            }
            return s.Split(seperator).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => int.Parse(s)).ToList();
        }

        public static float ToFloat(this string s)
        {
            if (float.TryParse(s, out float result)) return result;
            return Mathf.NegativeInfinity;  // To prevent errors, see them immediately.
        }

        public static int ToInt(this string s)
        {
            if (int.TryParse(s, out int result)) return result;
            return -99999999;  // To prevent errors, see them immediately.
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
        /// <summary>
        /// Parses "Health:100%" / "Attack:20.3"
        /// Into ("Health", 100, true) / ("Attack", 20.3, false)
        /// the bool indicates if the value is a percentage.
        /// </summary>
        public static (string, float, bool) ToStringValueIsPercentage(this string s)
        {
            List<string> args = s.Split(':').Select(s => s.Trim()).ToList();
            string text = args[0];
            string value = args[1];
            bool isPercentage = false;

            if (value.Last() == '%')
            {
                value = value.Substring(0, value.Length - 1);
                isPercentage = true;
            }

            return (text, value.ToFloat(), isPercentage);
        }
    }
}



#endif