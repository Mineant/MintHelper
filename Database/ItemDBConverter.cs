using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mineant;
using UnityEditor;
using UnityEngine;

public abstract class ItemDBConverter : MonoBehaviour
{
    public abstract void GenerateDatabase(List<TextAsset> databases);
}

public abstract class ItemDBConverter<T> : ItemDBConverter
{
    public override void GenerateDatabase(List<TextAsset> databases)
    {
        GenerateDatabase(CSVReader.SplitCsvGrid(databases.FirstOrDefault(t => t.name == $"{GetExcelName()}").text));
    }

    protected virtual List<T> GenerateDatabase(string[,] database)
    {
        // Find current creatures
        List<T> items = new();

        // Start from 1, ignore first line
        for (int i = 1; i < database.GetLength(1); i++)
        {
            if (IsDatabaseItemNull(database, i)) break;

            T item = default;

            if (FindExistingItem(database, i, out T foundItem))
            {
                item = foundItem;
            }
            else
            {
                item = CreateNewItem(database, i);
            }

            EditItem(item, database, i);
            if (item is UnityEngine.Object obj) EditorUtility.SetDirty(obj);

            items.Add(item);
        }

        // Set global resources
        PostProcessItems(items);

        // Save to asset
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return items;
    }

    public abstract string GetExcelName();
    protected abstract string GetExportPath();

    protected abstract bool FindExistingItem(string[,] database, int index, out T item);

    protected abstract T CreateNewItem(string[,] database, int index);

    protected abstract void EditItem(T item, string[,] database, int index);

    protected abstract void PostProcessItems(List<T> items);

    /// <summary>
    /// By default this checks if the first item (the id) is empty or null, if empty, then will stop processing database and not process the remaining items.
    /// </summary>
    /// <param name="database"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    protected virtual bool IsDatabaseItemNull(string[,] database, int index)
    {
        return String.IsNullOrWhiteSpace(database[0, index]);
    }

}

