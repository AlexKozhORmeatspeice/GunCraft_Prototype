using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ChooseShopScreen : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int spawnCount = 2;
    [SerializeField] private int rerrolCost = 100;

    [Header("Objs")]
    [SerializeField] private ChooseSlot chooseSlotPrefab;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventoryScreen inventoryScreen;
    [SerializeField] private Inventory inventory;
    [SerializeField] private TMP_Text rerollText;

    private List<ChooseSlot> slots = new();

    private bool isInit;

    void Awake()
    {
        isInit = false;
    }

    void Update()
    {
        if(rerollText != null)
        {
            rerollText.text = "Реролл (" + rerrolCost + ")";
        }
    }

    public void Show()
    {
        if(!isInit)
        {
            isInit = true;
            Reroll();
        }
    }

    public void Hide()
    {
        
    }

    public void OnReroll()
    {
        if(CurrencyManager.instance.CurrentRunMoney < rerrolCost) return;
        CurrencyManager.instance.CurrentRunMoney -= rerrolCost;

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

        if(inventory == null) return;

        var items = inventory.AllItems;
        var randItems = items.OrderBy(x => Random.Range(0.0f, 1.0f)).ToList();

        for(int i = 0; i < spawnCount; i++)
        {
            int ind = i % randItems.Count;
            Item item = randItems[ind];

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
                if(CurrencyManager.instance.CurrentRunMoney < slot.itemData.price)
                {
                    Debug.Log("Недостаточно валюты!");
                    return;
                }

                CurrencyManager.instance.CurrentRunMoney -= slot.itemData.price;
                
                inventory.CurrentItems.Add(slot.itemData);
                inventoryScreen.RefreshInventory();

                Reroll();
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
