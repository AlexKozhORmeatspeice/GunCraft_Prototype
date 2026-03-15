using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChooseShopScreen : MonoBehaviour
{
    [SerializeField] private ChooseSlot chooseSlotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private Inventory inventory;
    [SerializeField] private InventoryScreen inventoryScreen;
    [SerializeField] private int spawnCount = 2;

    private List<ChooseSlot> slots = new();

    public void Show()
    {
        Reroll();
    }

    public void Hide()
    {
        foreach(var slot in slots)
        {
            Destroy(slot.gameObject);
        }

        slots.Clear();
    }

    public void OnReroll()
    {
        Reroll();
    }

    public void OnChoose()
    {
        Choose();
        Reroll();
    }

    private void Reroll()
    {
        foreach(var slot in slots)
        {
            Destroy(slot.gameObject);
        }

        slots.Clear();

        var items = inventory.AllItems;
        var randItems = items.OrderBy(x => x.price).ToList();

        for(int i = 0; i < items.Count; i++)
        {
            int ind = i % randItems.Count;
            Item item = randItems[i];

            var card = CreateCard(item);
            slots.Add(card);
        }
    }

    private void Choose()
    {
        foreach(var slot in slots)
        {
            if(slot.GetComponentInChildren<Toggle>().isOn == true)
            {
                if(CurrencyManager.instance.CurrentMoney < slot.itemData.price)
                {
                    Debug.Log("Недостаточно валюты!");
                    return;
                }

                CurrencyManager.instance.CurrentMoney -= slot.itemData.price;
                
                inventory.CurrentItems.Add(slot.itemData);
                inventoryScreen.RefreshInventory();
                break;
            }
        }
    }

    private ChooseSlot CreateCard(Item item)
    {
        var card = Instantiate(chooseSlotPrefab, slotContainer);
        card.gameObject.SetActive(true);

        card.Desc = item.GetDesc();
        card.itemData = item;

        return card;
    }
}
