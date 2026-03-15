using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
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

    void Start()
    {
        
    }

    private void InitBaseInventory()
    {
        currentItems.Clear();

        currentItems.Add(GetItem(ItemAPI.Item_BaseDmg));
        currentItems.Add(GetItem(ItemAPI.Item_BaseDmg));

        currentItems.Add(GetItem(ItemAPI.Item_Distance));
        currentItems.Add(GetItem(ItemAPI.Item_Distance));

        currentItems.Add(GetItem(ItemAPI.Item_ShootRate));
        currentItems.Add(GetItem(ItemAPI.Item_ShootRate));
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
