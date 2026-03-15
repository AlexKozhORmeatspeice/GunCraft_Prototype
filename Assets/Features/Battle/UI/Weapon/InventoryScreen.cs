using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScreen : MonoBehaviour
{
    [SerializeField] private ItemSlot itemPrefab;
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private int slotCount;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private Inventory inventory;
    
    private List<Slot> slots = new List<Slot>();
    private bool isInitialized = false;

    public void Show()
    {
        gameObject.SetActive(true);
        
        // Инициализируем экран при первом показе
        if (!isInitialized)
        {
            InitializeSlots();
        }
        
        // Обновляем содержимое при каждом показе
        RefreshInventory();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void InitializeSlots()
    {
        if (slotContainer == null || slotPrefab == null) return;

        // Очищаем существующие слоты
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        // Создаем новые слоты
        for (int i = 0; i < slotCount; i++)
        {
            Slot newSlot = Instantiate(slotPrefab, slotContainer);
            newSlot.slotType = SlotType.Inventory;
            newSlot.inventory = inventory;
            
            newSlot.name = $"Slot_{i}";
            
            slots.Add(newSlot);
        }

        isInitialized = true;
    }

    public void RefreshInventory()
    {
        if (inventory == null || inventory.CurrentItems == null) return;

        // Сначала очищаем все слоты
        ClearAllSlots();

        // Заполняем слоты предметами из инвентаря
        for (int i = 0; i < inventory.CurrentItems.Count && i < slots.Count; i++)
        {
            Item itemData = inventory.CurrentItems[i];
            
            if (itemData != null && slots[i] != null)
            {
                // Создаем визуальный предмет
                ItemSlot newItem = CreateItemVisual(itemData, slots[i]);
                
                // Помещаем предмет в слот
                slots[i].PlaceItem(newItem);
            }
        }
    }

    private ItemSlot CreateItemVisual(Item itemData, Slot targetSlot)
    {
        if (itemPrefab == null) return null;

        // Создаем предмет
        ItemSlot newItem = Instantiate(itemPrefab, targetSlot.transform);
        
        // Настраиваем предмет
        newItem.data = itemData;
        newItem.itemName = itemData.name;
        newItem.itemIcon = itemData.icon;
        newItem.draggable = true;
        
        // Устанавливаем иконку если есть Image компонент
        var itemImage = newItem.GetComponent<UnityEngine.UI.Image>();
        if (itemImage != null && itemData.icon != null)
        {
            itemImage.sprite = itemData.icon;
        }
        
        return newItem;
    }

    private void ClearAllSlots()
    {
        foreach (Slot slot in slots)
        {
            if (slot != null && slot.currentItem != null)
            {
                // Уничтожаем визуальный предмет
                Destroy(slot.currentItem.gameObject);
                slot.currentItem = null;
            }
            slot.isEmpty = true;
        }
    }

    // Обработчики событий слотов
    private void OnItemPlacedInSlot(Slot slot, ItemSlot item)
    {
        UpdateInventoryData();
    }

    private void OnItemRemovedFromSlot(Slot slot, ItemSlot item)
    {
        UpdateInventoryData();
    }

    private void UpdateInventoryData()
    {
        if (inventory == null || inventory.CurrentItems == null) return;
    }

    // Метод для получения конкретного слота
    public Slot GetSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
        {
            return slots[index];
        }
        return null;
    }

    // Метод для проверки, пуст ли инвентарь
    public bool IsInventoryEmpty()
    {
        foreach (Slot slot in slots)
        {
            if (!slot.isEmpty)
            {
                return false;
            }
        }
        return true;
    }

    // Метод для поиска предмета по имени
    public ItemSlot FindItemByName(string itemName)
    {
        foreach (Slot slot in slots)
        {
            if (!slot.isEmpty && slot.currentItem != null && slot.currentItem.itemName == itemName)
            {
                return slot.currentItem;
            }
        }
        return null;
    }
}