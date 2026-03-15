using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Weapon
}

public class Slot : MonoBehaviour, IDropHandler
{
    [Header("Slot Settings")]
    public bool isEmpty = true;
    public ItemSlot currentItem;
    public Inventory inventory;

    public SlotType slotType = SlotType.Inventory;

    // Событие при помещении предмета в слот
    public System.Action<Slot, ItemSlot> OnItemPlaced;
    // Событие при удалении предмета из слота
    public System.Action<Slot, ItemSlot> OnItemRemoved;
    
    protected virtual void Start()
    {
        // При старте проверяем, есть ли уже предмет в слоте
        if (currentItem != null)
        {
            isEmpty = false;
            currentItem.SetParentSlot(this);
        }
    }
    
    // Вызывается когда предмет бросают на этот слот
    public virtual void OnDrop(PointerEventData eventData)
    {
        // Проверяем, есть ли перетаскиваемый предмет
        if (ItemSlot.draggedItem == null) return;
        
        ItemSlot draggedItem = ItemSlot.draggedItem;
        
        // Позиционируем предмет в слоте
        if(inventory != null && slotType == SlotType.Inventory)
        {
            if(draggedItem.data != null)
                inventory.CurrentItems.Add(draggedItem.data);
        }

        // Если слот пустой
        if (isEmpty)
        {
            PlaceItem(draggedItem);
        }
    }
    
    // Поместить предмет в слот
    public virtual void PlaceItem(ItemSlot item)
    {
        if (item == null) return;
        
        // Убираем предмет из предыдущего слота
        if (item.currentSlot != null)
        {
            item.currentSlot.RemoveItem();
        }
        
        // Помещаем предмет в этот слот
        item.SetParentSlot(this);
        currentItem = item;
        isEmpty = false;
        

        item.transform.SetParent(transform);
        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        if(item.data != null && item.data.id == ItemAPI.Item_AdditionalBullet && slotType == SlotType.Weapon)
        {
            item.draggable = false;
        }
        
        OnItemPlaced?.Invoke(this, item);
    }
    
    // Удалить предмет из слота
    public virtual void RemoveItem()
    {
        if (currentItem != null)
        {
            ItemSlot item = currentItem;
            currentItem = null;
            isEmpty = true;
            
            if(inventory != null && slotType == SlotType.Inventory)
            {
                if(item.data != null)
                    inventory.CurrentItems.Remove(item.data);
            }

            OnItemRemoved?.Invoke(this, item);
        }
    }
    
    // Поменять предметы местами
    protected virtual void SwapItems(ItemSlot draggedItem)
    {
        if (draggedItem == null || currentItem == null) return;
        
        Slot draggedSlot = draggedItem.currentSlot;
        ItemSlot currentSlotItem = currentItem;
        
        // Убираем текущий предмет из этого слота
        RemoveItem();
        
        // Помещаем перетаскиваемый предмет в этот слот
        PlaceItem(draggedItem);
        
        // Помещаем предыдущий предмет в слот, откуда пришел перетаскиваемый
        if (draggedSlot != null)
        {
            draggedSlot.PlaceItem(currentSlotItem);
        }
    }
}