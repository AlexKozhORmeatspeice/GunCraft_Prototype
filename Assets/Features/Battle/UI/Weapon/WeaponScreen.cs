using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class WeaponScreen : MonoBehaviour
{
    [SerializeField] private UnitShoot shootComp;
    [SerializeField] private BuyNewSlotButton newSlotButtonPrefab;
    [SerializeField] private Transform vertContainer; // Префаб горизонтального контейнера
    [SerializeField] private Transform horrContainer; // Префаб вертикального контейнера
    [SerializeField] private ItemSlot itemPrefab;
    [SerializeField] private Slot slotPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private MouseMoveArea mouseMoveArea;
    [SerializeField] private float additionalSlotPrice = 100;

    private List<BuyNewSlotButton> buyNewSlots = new();

    private Dictionary<WeaponTreeNode, GameObject> nodeToContainerMap = new();
    private Dictionary<WeaponTreeNode, Slot> nodeToSlotMap = new();
    private Dictionary<Slot, WeaponTreeNode> slotToNodeMap = new();

    public void Show()
    {
        gameObject.SetActive(true);
        CreateTree();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ClearTree();
    }

    private void CreateTree()
    {
        if (shootComp == null || shootComp.treeRoot == null) return;

        ClearTree();
        
        // Создаем корневой вертикальный контейнер для всего дерева
        Transform rootHorrContainer = CreateContainer(horrContainer, container);
        rootHorrContainer.name = "Root_Horizontal";

        mouseMoveArea.content = rootHorrContainer.GetComponent<RectTransform>();
        
        // Начинаем построение дерева с корневого узла
        CreateNode(shootComp.treeRoot, rootHorrContainer);
    }

    private void CreateNode(WeaponTreeNode node, Transform parentVerticalContainer)
    {
        if (node == null) return;

        // Создаем горизонтальный контейнер для текущего узла (строка с предметом и детьми)
        Transform vertContainer = CreateContainer(this.vertContainer, parentVerticalContainer);
        vertContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.5f, 0);
        vertContainer.name = $"Row_{GetNodeName(node)}_Vert";
        vertContainer.transform.SetAsFirstSibling();
        
        // Создаем слот для предмета
        Slot slot = CreateSlot(vertContainer);
        nodeToSlotMap[node] = slot;
        slotToNodeMap[slot] = node;
        
        // Если есть предмет, создаем его визуальное представление
        if (node.item != null)
        {
            ItemSlot itemSlot = CreateItem(node.item, slot.transform);
            itemSlot.data = node.item;

            slot.PlaceItem(itemSlot);
        }

        slot.OnItemPlaced += OnAddItem;
        slot.OnItemRemoved += OnRemoveItem;

        // Если есть дочерние узлы
        if (node.childs != null && node.childs.Count > 0)
        {
            // Создаем вертикальный контейнер для дочерних узлов
            // Он будет расположен справа от слота в том же горизонтальном контейнере
            Transform childrenHorrContainer = CreateContainer(horrContainer, vertContainer);
            childrenHorrContainer.name = $"Children_{GetNodeName(node)}_Horizontal";
            childrenHorrContainer.transform.SetAsFirstSibling();
            
            // Для каждого дочернего узла создаем отдельный горизонтальный контейнер
            foreach (WeaponTreeNode child in node.childs)
            {
                if (child != null)
                {
                    CreateNode(child, childrenHorrContainer);
                }
            }
        }

        // Если нет дочерних узлов создаем слот для покупки
        if (node.childs == null || node.childs.Count == 0)
        {
            // Создаем вертикальный контейнер для дочерних узлов
            // Он будет расположен справа от слота в том же горизонтальном контейнере
            Transform childrenHorrContainer = CreateContainer(horrContainer, vertContainer);
            childrenHorrContainer.name = $"Children_{GetNodeName(node)}_Horizontal";
            childrenHorrContainer.transform.SetAsFirstSibling();

            CreateBuySlot(childrenHorrContainer, node);
        }

        nodeToContainerMap[node] = vertContainer.gameObject;
    }

    private Transform CreateContainer(Transform prefab, Transform parent)
    {
        if (prefab == null || parent == null) return null;

        Transform newContainer = Instantiate(prefab, parent);
        newContainer.gameObject.SetActive(true);
        
        return newContainer;
    }

    private Slot CreateSlot(Transform parent)
    {
        if (slotPrefab == null || parent == null) return null;

        Slot newSlot = Instantiate(slotPrefab, parent);
        newSlot.slotType = SlotType.Weapon;
        newSlot.name = "Slot";

        return newSlot;
    }

    private void OnAddItem(Slot slot, ItemSlot item)
    {
        if(slotToNodeMap.TryGetValue(slot, out var node))
        if(node == null) return;
        
        node.item = item.data;

        slot.OnItemPlaced -= OnAddItem;
        slot.OnItemRemoved -= OnRemoveItem;

        shootComp.UpdateTree();

        CreateTree();
    }

    private void OnRemoveItem(Slot slot, ItemSlot item)
    {
        if(slotToNodeMap.TryGetValue(slot, out var node))
        if(node == null) return;
        
        node.item = null;

        slot.OnItemPlaced -= OnAddItem;
        slot.OnItemRemoved -= OnRemoveItem;

        shootComp.UpdateTree();
        
        CreateTree();
    }

    private BuyNewSlotButton CreateBuySlot(Transform parent, WeaponTreeNode node)
    {
        if (slotPrefab == null || parent == null) return null;

        BuyNewSlotButton newSlot = Instantiate(newSlotButtonPrefab, parent);
        newSlot.node = node;
        buyNewSlots.Add(newSlot);

        newSlot.onClickBuy += OnBuySlot;

        return newSlot;
    }

    private void OnBuySlot(BuyNewSlotButton buyBtn, WeaponTreeNode node)
    {
        if(CurrencyManager.instance.CurrentMoney < additionalSlotPrice)
        {
            Debug.Log("Not enough money to buy new slot!");
            return;
        }

        CurrencyManager.instance.CurrentMoney -= (int)additionalSlotPrice;

        buyBtn.onClickBuy -= OnBuySlot;

        WeaponTreeNode newNode = new();
        newNode.item = null;
        newNode.parent = node;
        newNode.item = null;
        newNode.isCreatedByItem = false;

        node.childs.Add(newNode);

        shootComp.UpdateTree();
        CreateTree();
    }

    private ItemSlot CreateItem(Item itemData, Transform parent)
    {
        if (itemPrefab == null || itemData == null) return null;

        ItemSlot newItem = Instantiate(itemPrefab, parent);
        newItem.name = $"Item_{itemData.name}";
        
        // Настраиваем предмет
        newItem.itemName = itemData.name;
        newItem.itemIcon = itemData.icon;
        newItem.itemCount = 1;
        newItem.draggable = true;
        
        // Устанавливаем иконку
        Image itemImage = newItem.GetComponent<Image>();
        if (itemImage != null && itemData.icon != null)
        {
            itemImage.sprite = itemData.icon;
        }

        return newItem;
    }

    private void ClearTree()
    {
        // Очищаем все дочерние объекты в container
        foreach (Transform child in container)
        {
            if (child != null && child != horrContainer && child != vertContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Очищаем словари
        foreach(var slot in slotToNodeMap.Keys)
        {
            slot.OnItemPlaced -= OnAddItem;
            slot.OnItemRemoved -= OnRemoveItem;
        }

        foreach (var slot in buyNewSlots)
        {
            slot.onClickBuy -= OnBuySlot;
        }

        buyNewSlots.Clear();
        nodeToContainerMap.Clear();
        nodeToSlotMap.Clear();
        slotToNodeMap.Clear();
    }

    private string GetNodeName(WeaponTreeNode node)
    {
        if (node == null) return "Null";
        if (node.item != null && !string.IsNullOrEmpty(node.item.name))
        {
            return node.item.name;
        }
        return "Empty";
    }

    // Метод для обновления конкретного узла
    public void UpdateNode(WeaponTreeNode node)
    {
        if (node == null || !nodeToSlotMap.ContainsKey(node)) return;

        Slot slot = nodeToSlotMap[node];
        
        // Очищаем слот
        if (slot.currentItem != null)
        {
            Destroy(slot.currentItem.gameObject);
            slot.RemoveItem();
        }

        // Создаем новый предмет если есть данные
        if (node.item != null)
        {
            ItemSlot newItem = CreateItem(node.item, slot.transform);
            slot.PlaceItem(newItem);
        }
    }

    // Метод для выделения пути к узлу
    public void HighlightPathToNode(WeaponTreeNode targetNode)
    {
        WeaponTreeNode current = targetNode;
        while (current != null)
        {
            if (nodeToContainerMap.ContainsKey(current))
            {
                // Подсвечиваем контейнер
                Image containerImage = nodeToContainerMap[current].GetComponent<Image>();
                if (containerImage != null)
                {
                    containerImage.color = Color.yellow;
                }
            }
            current = current.parent;
        }
    }

    // Метод для сброса подсветки
    public void ResetHighlight()
    {
        foreach (var kvp in nodeToContainerMap)
        {
            Image containerImage = kvp.Value.GetComponent<Image>();
            if (containerImage != null)
            {
                containerImage.color = Color.white;
            }
        }
    }
}