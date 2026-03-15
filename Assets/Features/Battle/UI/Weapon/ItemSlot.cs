using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item Settings")]
    public string itemName;

    [SerializeField] private Image icon;
    public Sprite itemIcon
    {
        get { return icon.sprite; }
        set => icon.sprite = value;
    }
    
    public int itemCount = 1;
    
    [Header("Drag Settings")]
    public bool draggable = true;
    public float dragAlpha = 0.6f;
    
    // Статическая ссылка на перетаскиваемый предмет
    public static ItemSlot draggedItem;
    
    // Родительский слот
    [HideInInspector] public Slot currentSlot;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Canvas mainCanvas;

    public Item data;
    
    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Добавляем CanvasGroup если его нет
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    protected virtual void Start()
    {
        // Находим главный Canvas
        mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
        }
    }
    
    // Установить родительский слот
    public void SetParentSlot(Slot slot)
    {
        currentSlot = slot;
        originalParent = slot.transform;
        transform.SetParent(slot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    // Начало перетаскивания
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        
        // Сохраняем исходные данные
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // Устанавливаем статическую ссылку
        draggedItem = this;
        
        // Визуальные изменения
        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;
        
        // Перемещаем предмет на верхний слой (в корень Canvas)
        transform.SetParent(mainCanvas.transform);
        
        OnBeginDragEvent(eventData);
    }
    
    // Процесс перетаскивания
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!draggable || draggedItem == null) return;
        
        // Перемещаем предмет за мышью
        rectTransform.anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
        
        OnDragEvent(eventData);
    }
    
    // Конец перетаскивания
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!draggable) return;
        
        // Возвращаем визуальные настройки
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        // Проверяем, был ли предмет брошен на слот
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        
        if (dropTarget != null)
        {
            Slot targetSlot = dropTarget.GetComponent<Slot>();
            
            // Если бросили на слот, OnDrop вызовется автоматически
            if (targetSlot != null && (targetSlot.currentItem == null))
            {
                
            }
            else
            {
                // Если бросили не на слот, возвращаемся в исходный слот
                ReturnToOriginalSlot();
            }
        }
        else
        {
            // Если бросили в пустоту, возвращаемся
            ReturnToOriginalSlot();
        }
        
        // Сбрасываем статическую ссылку
        draggedItem = null;
        
        OnEndDragEvent(eventData);
    }
    
    // Вернуться в исходный слот
    protected virtual void ReturnToOriginalSlot()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    // Виртуальные методы для переопределения в наследниках
    protected virtual void OnBeginDragEvent(PointerEventData eventData) { }
    protected virtual void OnDragEvent(PointerEventData eventData) { }
    protected virtual void OnEndDragEvent(PointerEventData eventData) { }
}