using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private List<string> baseItems = new();

    private const string itemsPath = "Inventory/Items";
    private List<Item> items;

    public List<Item> AllItems => items;

    private List<Item> currentItems = new();
    public List<Item> CurrentItems => currentItems;

    public void Awake()
    {
        items = Resources.LoadAll<Item>(itemsPath).ToList();
        InitBaseInventory();
    }

    private void InitBaseInventory()
    {
        currentItems.Clear();

        foreach (var item in baseItems)
        {
            currentItems.Add(GetItem(item));
        }
    }

    public Item GetItem(string id)
    {
        if(items == null)
        {
            items = Resources.LoadAll<Item>(itemsPath).ToList();
        }
        
        foreach (var item in items)
        {
            if (item.id == id)
            {
                return item;
            }
        }

        return null;
    }
}
