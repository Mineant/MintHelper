using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

#if DOTWEEN
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
#endif

namespace Mineant
{
    public static class Helpers
    {
        #region Miscellanious Extensions

        public static TValue Value<TValue>(this IList<TValue> list, int index, TValue defaultValue = default)
        {
            if (list.Count == 0 || index >= list.Count) return defaultValue;
            return list[index];
        }

        #endregion


        #region List Extensions
        /// <summary>
        /// Checks whether the list.AddRange(source) source is null or the collection is 0 count.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="collection"></param>
        public static void SmartAddRange<T>(this List<T> target, IList<T> collection)
        {
            if (target == null) target = new List<T>();
            if (collection == null || collection.Count == 0) return;

            target.AddRange(collection);
        }

        /// <summary>
        /// List.Add but check item and target is null or not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="item"></param>
        public static void SmartAdd<T>(this List<T> target, T item)
        {
            if (target == null) target = new List<T>();
            if (item == null ) return;
            target.Add(item);
        }


        /// <summary>
        /// bannedPreElement and bannedPostElement means A:{a,b,b,a} and B:{c,b,b,a}, is the bannedPreelement is a and the sublist is {b,b}, then list A wont satisfy the condition since shouldnt have a in front of the sublist {b,b}.
        /// </summary>
        public static List<int> ContainsSubList(this List<string> masterList, List<string> subList, string bannedPreElement, string bannedPostElement, string masterElement = "")
        {
            List<int> subListHeader = new List<int>();
            for (int i = 0; i <= masterList.Count - subList.Count; i++)
            {

                bool same = true;

                // check for banned cards, if index is out of range, then automatically wont trigger banned card, so wont check
                if (bannedPreElement.Length > 0 && masterList.IndexWithinRange(i - 1))
                    if (masterList[i - 1] == bannedPreElement) same = false;

                if (bannedPostElement.Length > 0 && masterList.IndexWithinRange(i + subList.Count))
                    if (masterList[i + subList.Count] == bannedPostElement) same = false;

                if (same)
                {
                    for (int j = 0; j < subList.Count; j++)
                    {
                        if (subList[j] == masterElement) continue;    // If it is any card, then we dun need to check.
                        else if (masterList[i + j] != subList[j])   // Check if a elemtn is not the same, and just exit.
                        {
                            same = false;
                            break;
                        }
                    }
                }

                if (same)
                {
                    subListHeader.Add(i);
                    i += subList.Count - 1;
                }
            }
            return subListHeader;
        }


        /// <summary>
        /// Checks if a list contains a sub list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="masterList"></param>
        /// <param name="subList"></param>
        /// <returns></returns>
        public static int ContainsSubList<T>(this List<T> masterList, List<T> subList)
        {
            int totalCombinations = 0;
            for (int i = 0; i <= masterList.Count - subList.Count; i++)
            {
                if (masterList.Skip(i).Take(subList.Count).SequenceEqual(subList))
                {
                    totalCombinations++;
                }
            }
            return totalCombinations;
        }


        /// <summary>
        /// Checks if list index is within range
        /// </summary>
        /// <param name="list"></param>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public static bool IndexWithinRange(this IList list, int targetIndex)
        {
            return targetIndex >= 0 && targetIndex < list.Count;
        }


        /// <summary>
        /// Checks if list index is within range
        /// </summary>
        /// <param name="list"></param>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public static bool IndexWithinRange(this Array array, int targetIndex)
        {
            return targetIndex >= 0 && targetIndex < array.Length;
        }


        /// <summary>
        /// This is useful for just picking one element without the need to check unique or anything
        /// </summary>
        public static T SimplePickRandom<T>(this List<T> source)
        {
            if (source.Count == 0) return default;   // returns null normally
            return source[UnityEngine.Random.Range(0, source.Count)];
        }

        /// <summary>
        /// Pick Random element from list/arrat
        /// </summary>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        /// <summary>
        /// Pick Random element from list/array. Will not pick duplicate elements
        /// </summary>
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        /// <summary>
        /// Shuffles a list
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
        #endregion






























        #region Text Extensions

        public enum TimeSpanToStringType { Default }
        /// <summary>
        /// Turn Timespan into a more readable string version. E.g. 4h00m, 39m20s...
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToReadableString(this TimeSpan timeSpan, TimeSpanToStringType type = TimeSpanToStringType.Default)
        {
            string msg = "";
            // if more than an hour
            if (timeSpan.TotalHours >= 1)
            {
                // Show xx h yy m
                msg = $"{timeSpan.Hours}h{timeSpan.Minutes}m";
            }
            else
            {
                // show xx m yy s
                msg = $"{timeSpan.Minutes}m{timeSpan.Seconds}s";
            }
            return msg;
        }


        /// <summary>
        /// Return 1 to 1st, 2 to 2nd, 3 to 3rd, 4 to 4th ....
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetIndicatedNumber(int number)
        {
            return number.ToString() + GetOrindalIndicator(number);
        }

        /// <summary>
        /// Return st from 1, nd from 2, rd from 3 ....
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetOrindalIndicator(int number)
        {
            switch (number)
            {
                case (11):
                case (12):
                case (13):
                    return "th";
            }

            int remainder = number % 10;
            string msg = "th";
            switch (remainder)
            {
                case (1):
                    msg = "st";
                    break;
                case (2):
                    msg = "nd";
                    break;
                case (3):
                    msg = "rd";
                    break;
            }

            return msg;
        }

        /// <summary>
        /// return 10 as +10, -10 as -10, 21 as +21
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToStringWithSign(this float value)
        {
            return (value >= 0 ? "+" : "") + value.ToString();
        }

        public static string ReplaceWord(this string input, string wordToReplace, string replacementWord)
        {
            var escapedWordToReplace = Regex.Escape(wordToReplace);
            var pattern = $"(?<!\\w){escapedWordToReplace}(?!\\w)";
            return Regex.Replace(input, pattern, replacementWord, RegexOptions.IgnoreCase);
        }

        #endregion
















































        #region DOTween Extensions
#if DOTWEEN
        /// <summary>
        /// Move the target by first parenting the object to a target
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="target"></param>
        /// <param name="targetLocalEndPosition"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveInTargetLocalSpace(this Transform transform, Transform target, Vector3 targetLocalEndPosition, float duration)
        {
            var t = DOTween.To(
                () => transform.position - target.transform.position, // Value getter
                x => transform.position = x + target.transform.position, // Value setter
                targetLocalEndPosition,
                duration);
            t.SetTarget(transform);
            return t;
        }
#endif
        #endregion






































        #region GameObject / Component Extensions

        public static T GetComponentOrInParent<T>(this Component mono, bool inparent)
        {
            if (inparent) return mono.GetComponentInParent<T>();
            else return mono.GetComponent<T>();
        }

        public static T SmartGetComponent<T>(this GameObject source) where T : Component
        {
            if (source.GetComponent<T>() == null) source.AddComponent<T>();
            return source.GetComponent<T>();
        }

        /// <summary>
        /// Check if a layer is within the layerMask
        /// </summary>
        public static bool Contains(this LayerMask layerMask, int layer)
        {
            return layerMask == (layerMask | (1 << layer));
        }

        /// <summary>
        /// Find interfaces in the current active scene
        /// </summary>
        public static List<T> FindInterfacesInActiveScene<T>()
        {
            return FindInterfaces<T>(SceneManager.GetActiveScene());
        }

        /// <summary>
        /// Find interfaces in the provided scene
        /// </summary>
        public static List<T> FindInterfaces<T>(Scene scene)
        {
            List<T> interfaces = new List<T>();
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            foreach (var rootGameObject in rootGameObjects)
            {
                T[] childrenInterfaces = rootGameObject.GetComponentsInChildren<T>(true);
                foreach (var childInterface in childrenInterfaces)
                {
                    interfaces.Add(childInterface);
                }
            }
            return interfaces;
        }

        /// <summary>
        /// Usually used because need to find objects in the dontDestoyrONLoad layer. This is very expensive, if really dont have the usage, dont use this.
        /// </summary>
        public static List<T> FindInterfacesInAllScenes<T>()
        {
            List<T> interfaces = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                interfaces.AddRange(FindInterfaces<T>(SceneManager.GetSceneAt(i)));
            }

            interfaces.AddRange(FindInterfaces<T>(GetDontDestroyOnLoadScene()));

            return interfaces;
        }

        #endregion









































        #region UI Extensions



        /// <summary>
        /// Return true if the pointer is hovering over a ui element. Used to check ui blocking something.
        /// </summary>
        /// <returns></returns>
        public static bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(GetEventSystemRaycastResults());
        }

        ///Returns 'true' if we touched or hovering on Unity UI element.
        public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
        {
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];

                if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all event systen raycast results of current mouse or touch position.
        /// </summary>
        /// <returns></returns>
        static List<RaycastResult> GetEventSystemRaycastResults()
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);

            return raysastResults;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="canvasGroup"></param>
        /// <param name="value"></param>
        public static void EnableCanvasGroup(this CanvasGroup canvasGroup, bool value)
        {
            canvasGroup.alpha = value ? 1f : 0f;
            canvasGroup.blocksRaycasts = value;
        }








        #endregion




























































        #region Number (int/float) Extensions
        /// <summary>
        /// A = 1, B = 2, C = 3 ...
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public static int LetterToNumber(char letter)
        {
            int index = char.ToUpper(letter) - 64;
            return index;
        }

        public static int RandomPositiveNegativeSign()
        {
            return UnityEngine.Random.Range(0, 2) == 0 ? -1 : 1;
        }


        public static int Wrap(this int x, int x_min, int x_max)
        {
            return (((x - x_min) % (x_max - x_min)) + (x_max - x_min)) % (x_max - x_min) + x_min;
        }





        #endregion

        /// <summary>
        /// This wil get a random enum.
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TEnum GetRandomEnum<TEnum>() where TEnum : System.Enum
        {
            Array values = Enum.GetValues(typeof(TEnum));
            System.Random random = new System.Random();
            TEnum randomBar = (TEnum)values.GetValue(random.Next(values.Length));
            return randomBar;
        }




        /// <summary>
        /// Return a new GUID. Used for id.
        /// </summary>
        /// <returns></returns>
        public static string GetUniqueID()
        {
            return Guid.NewGuid().ToString();
        }



        public enum CalculationMethod { Add, Subtract, Multiply, Divide, Power }


        /// <summary>
        /// Calcuulate
        /// </summary>
        /// <param name="method"></param>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <returns></returns>
        public static float Calculate(CalculationMethod method, float valueA, float valueB)
        {
            switch (method)
            {
                case (CalculationMethod.Add):
                    return valueA + valueB;
                case (CalculationMethod.Subtract):
                    return valueA - valueB;
                case (CalculationMethod.Multiply):
                    return valueA * valueB;
                case (CalculationMethod.Divide):
                    return valueA / valueB;
                case (CalculationMethod.Power):
                    return Mathf.Pow(valueA, valueB);
                default:
                    Debug.LogError($"Method {method} not implemented.");
                    return 0;
            }
        }



        /// <summary>
        /// Select a point on the boundaries of a rectangle by giving the lefttop point and rightbottom point for the rect
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static Vector2 PointOnBounds(float x1, float y1, float x2, float y2)
        {
            // Generate a random number between 0 and 3 to select a side of the rectangle
            int side = UnityEngine.Random.Range(0, 4);

            // Depending on the side selected, generate a random position along the corresponding edge
            switch (side)
            {
                case 0: // Top side
                    return new Vector2(UnityEngine.Random.Range(x1, x2), y1);
                case 1: // Right side
                    return new Vector2(x2, UnityEngine.Random.Range(y1, y2));
                case 2: // Bottom side
                    return new Vector2(UnityEngine.Random.Range(x1, x2), y2);
                case 3: // Left side
                    return new Vector2(x1, UnityEngine.Random.Range(y1, y2));
                default:
                    return Vector2.zero;
            }
        }




#if UNITY_EDITOR

        /// <summary>
        /// Adds newly (if not already in the list) found assets.
        /// Returns how many found (not how many added)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="assetsFound">Adds to this list if it is not already there</param>
        /// <returns></returns>
        public static int TryGetUnityObjectsOfTypeFromPath<T>(string path, List<T> assetsFound) where T : UnityEngine.Object
        {
            string[] filePaths = System.IO.Directory.GetFiles(path);

            int countFound = 0;

            Debug.Log(filePaths.Length);

            if (filePaths != null && filePaths.Length > 0)
            {
                for (int i = 0; i < filePaths.Length; i++)
                {
                    UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(filePaths[i], typeof(T));
                    if (obj is T asset)
                    {
                        countFound++;
                        if (!assetsFound.Contains(asset))
                        {
                            assetsFound.Add(asset);
                        }
                    }
                }
            }

            return countFound;
        }

#endif





        public static Vector3 GetRandomPointInsideCollider(this BoxCollider boxCollider)
        {
            Vector3 extents = boxCollider.size / 2f;
            Vector3 point = new Vector3(
                UnityEngine.Random.Range(-extents.x, extents.x),
                UnityEngine.Random.Range(-extents.y, extents.y),
                UnityEngine.Random.Range(-extents.z, extents.z)
            ) + boxCollider.center;

            return boxCollider.transform.TransformPoint(point);
        }

        public static Vector3 GetRandomPointInsideCollider2D(this BoxCollider2D boxCollider)
        {
            Vector2 extents = boxCollider.size / 2f;
            Vector2 point = new Vector2(
                UnityEngine.Random.Range(-extents.x, extents.x),
                UnityEngine.Random.Range(-extents.y, extents.y)
            ) + boxCollider.offset;

            return boxCollider.transform.TransformPoint(point);
        }

        public static Scene GetDontDestroyOnLoadScene()
        {
            GameObject temp = null;
            try
            {
                temp = new GameObject();
                UnityEngine.Object.DontDestroyOnLoad(temp);
                UnityEngine.SceneManagement.Scene dontDestroyOnLoad = temp.scene;
                UnityEngine.Object.DestroyImmediate(temp);
                temp = null;

                return dontDestroyOnLoad;
            }
            finally
            {
                if (temp != null)
                    UnityEngine.Object.DestroyImmediate(temp);
            }
        }

        public static Vector3 GetRandomPointInCircle(Vector3 origin, float radius)
        {
            return (Vector3)UnityEngine.Random.insideUnitCircle * radius + origin;
        }

        public static Vector3 GetRandomPointOnCircle(Vector3 origin, float radius)
        {
            return (Vector3)UnityEngine.Random.insideUnitCircle.normalized * radius + origin;
        }

        public static GameObject[] GetDontDestroyOnLoadObjects()
        {
            return GetDontDestroyOnLoadScene().GetRootGameObjects();
        }


        public enum ComparasonType
        {
            LessThan,
            LessThanOrEqualTo,
            EqualTo,
            MoreThanOrEqualTo,
            MoreThan,
        }

        public static bool Compare(ComparasonType type, float leftValue, float rightValue)
        {
            switch (type)
            {
                case (ComparasonType.LessThan): return leftValue < rightValue;
                case (ComparasonType.LessThanOrEqualTo): return leftValue <= rightValue;
                case (ComparasonType.EqualTo): return leftValue == rightValue;
                case (ComparasonType.MoreThanOrEqualTo): return leftValue >= rightValue;
                case (ComparasonType.MoreThan): return leftValue > rightValue;
            }

            return false;
        }









        public class ScriptableObjectIdAttribute : PropertyAttribute { }

#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ScriptableObjectIdAttribute))]
        public class ScriptableObjectIdDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                // GUI.enabled = false;
                if (string.IsNullOrEmpty(property.stringValue))
                {
                    property.stringValue = Guid.NewGuid().ToString();
                }
                EditorGUI.PropertyField(position, property, label, true);
                // GUI.enabled = true;
            }
        }
#endif
    }
}
