using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mineant;
using Mineant.Inventory;
using UnityEditor;
using UnityEngine;

public class InventoryItemDBConverter : ItemDBConverter<InventoryItem>
{
    public override string GetExcelName()
    {
        return "InventoryItem";
    }

    protected override InventoryItem CreateNewItem(string[,] database, int index)
    {
        InventoryItem item = ScriptableObject.CreateInstance<InventoryItem>();
        AssetDatabase.CreateAsset(item, $"{GetExportPath()}/{database[1, index]}.asset");
        return item;
    }

    protected override void EditItem(InventoryItem item, string[,] database, int index)
    {
        item.ItemID = database[0, index];
        item.Name = database[1, index];
        item.Description = database[2, index];
    }

    protected override bool FindExistingItem(string[,] database, int index, out InventoryItem item)
    {
        List<InventoryItem> items = new();
        Helpers.TryGetUnityObjectsOfTypeFromPath<InventoryItem>(GetExportPath(), items);
        item = items.FirstOrDefault(item => item.ItemID == database[0, index]);
        return item != null;
    }

    protected override string GetExportPath()
    {
        return "Assets/Resources/DB Test";
    }

    protected override void PostProcessItems(List<InventoryItem> items)
    {
        foreach (InventoryItem item in items)
        {
            Debug.Log(item.Name);
        }
    }
}