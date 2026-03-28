using UnityEngine;

public class EnvSpawner : MonoBehaviour
{
    [SerializeField] private Unit factoryUnit;
    [SerializeField] private int factoryPrice;
    [SerializeField] private Unit gunUnit;
    [SerializeField] private int gunPrice;

    public int FactoryPrice => factoryPrice;
    public int GunPrice => gunPrice;
    
    private GameObject previewFactory;
    private GameObject previewGun;
    private Camera mainCamera;
    
    private bool isPlacingFactory = false;
    private bool canPlaceFactory = false;

    private bool isPlacingGun = false;
    private bool canPlaceGun = false;
    
    private void Awake()
    {
        mainCamera = Camera.main;
    }
    
    public void StartSpawnFactory()
    {
        if (isPlacingFactory || isPlacingGun) return;
        if(CurrencyManager.instance.CurrentRunMoney < factoryPrice) return;
        
        // Создаем превью фабрики
        if (factoryUnit != null)
        {
            previewFactory = Instantiate(factoryUnit.gameObject);
            previewFactory.GetComponent<Collider2D>().enabled = false; // Отключаем коллайдер для превью
            previewFactory.GetComponent<Factory>().SetActive(false);
            isPlacingFactory = true;
            canPlaceFactory = false;
        }
    }

    public void StartSpawnGun()
    {
        if (isPlacingFactory || isPlacingGun) return;
        if(CurrencyManager.instance.CurrentRunMoney < gunPrice) return;
        
        // Создаем превью фабрики
        if (gunUnit != null)
        {
            previewGun = Instantiate(gunUnit.gameObject);
            previewGun.GetComponent<Collider2D>().enabled = false; // Отключаем коллайдер для превью
            previewGun.GetComponent<GunController>().ChangeActive(false);
            isPlacingGun = true;
            canPlaceGun = false;
        }
    }

    public void CancelAll()
    {
        CancelPlacement();
        CancelPlacementGun();
    }
    
    private void Update()
    {
        if (isPlacingFactory)
        {
            CheckSpawnFactory();
            HandlePlacementInput();
        }

        if(isPlacingGun)
        {
            CheckSpawnGun();
            HandlePlacementInputGun();
        }
    }
    
    private void CheckSpawnFactory()
    {
        if (previewFactory == null) return;
        
        // Получаем позицию мыши в мире
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        previewFactory.transform.position = mouseWorldPosition;
        
        // Проверяем, можно ли разместить фабрику в текущей позиции
        canPlaceFactory = IsValidPlacementPosition(mouseWorldPosition);
        
        // Изменяем цвет превью в зависимости от возможности размещения
        SpriteRenderer renderer = previewFactory.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = canPlaceFactory ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        }
    }
    
    private void HandlePlacementInput()
    {
        // Фиксируем позицию по клику мыши
        if (Input.GetMouseButtonDown(0))
        {
            if (canPlaceFactory)
            {
                PlaceFactory();
            }
        }
        
        // Отмена размещения по правой кнопке мыши или ESC
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    private void CheckSpawnGun()
    {
        if (previewGun == null) return;
        
        // Получаем позицию мыши в мире
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        previewGun.transform.position = mouseWorldPosition;
        
        // Проверяем, можно ли разместить фабрику в текущей позиции
        canPlaceGun = IsValidPlacementPositionGun(mouseWorldPosition);
        
        // Изменяем цвет превью в зависимости от возможности размещения
        SpriteRenderer renderer = previewGun.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = canPlaceGun ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        }
    }
    
    private void HandlePlacementInputGun()
    {
        // Фиксируем позицию по клику мыши
        if (Input.GetMouseButtonDown(0))
        {
            if (canPlaceGun)
            {
                PlaceGun();
            }
        }
        
        // Отмена размещения по правой кнопке мыши или ESC
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacementGun();
        }
    }
    
    private void PlaceFactory()
    {
        if (previewFactory == null) return;
        
        // Включаем коллайдер для размещенной фабрики
        Collider2D collider = previewFactory.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        // Возвращаем нормальный цвет
        SpriteRenderer renderer = previewFactory.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.white;
        }
        
        previewFactory.GetComponent<Factory>().SetActive(true);
        
        // Убираем флаг размещения и очищаем превью
        isPlacingFactory = false;
        previewFactory = null;
        canPlaceFactory = false;

        CurrencyManager.instance.CurrentRunMoney -= factoryPrice;

        //Уменьшаем количество объектов на карте
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(mouseWorldPosition);
        foreach (var colliderr in hitColliders)
        {
            var minearea = colliderr.GetComponent<MineArea>();
            if (minearea != null)
            {
                minearea.currentCount--;
                break;
            }
        }
    }

    private void PlaceGun()
    {
        if (previewGun == null) return;

        // Включаем коллайдер для размещенной фабрики
        Collider2D collider = previewGun.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        // Возвращаем нормальный цвет
        SpriteRenderer renderer = previewGun.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.white;
        }

        previewGun.GetComponent<GunController>().ChangeActive(true);

        CurrencyManager.instance.CurrentRunMoney -= gunPrice;
        
        // Убираем флаг размещения и очищаем превью
        isPlacingGun = false;
        previewGun = null;
        canPlaceGun = false;
    }
    
    private void CancelPlacement()
    {
        if (previewFactory != null)
        {
            Destroy(previewFactory);
        }
        
        isPlacingFactory = false;
        previewFactory = null;
        canPlaceFactory = false;
    }
    
    private void CancelPlacementGun()
    {
        if (previewGun != null)
        {
            Destroy(previewGun);
        }
        
        isPlacingGun = false;
        previewGun = null;
        canPlaceGun = false;
    }
    
    private bool IsValidPlacementPosition(Vector3 position)
    {
        // Проверяем, есть ли объект с MineArea в позиции мыши
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(position);
        
        foreach (var collider in hitColliders)
        {
            if (collider.GetComponent<MineArea>() != null)
            {
                // Проверяем, не занято ли место другой фабрикой
                if (IsPositionOccupied(position))
                {
                    return false;
                }
                return true;
            }
        }
        
        return false;
    }

    private bool IsValidPlacementPositionGun(Vector3 position)
    {
        // Get all GameObjects at the position using raycast
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        
        foreach (var hit in hits)
        {
            if(hit.collider.gameObject == previewGun) continue;

            if (hit.collider.gameObject.tag != "Ground")
            {
                return false;
            }
        }

        return true;
    }
    
    private bool IsPositionOccupied(Vector3 position)
    {
        // Проверяем, есть ли другие объекты с Unit (фабрики) в этой позиции
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(position);
        
        foreach (var collider in hitColliders)
        {
            Unit unit = collider.GetComponent<Unit>();
            if (unit != null && unit.gameObject != previewFactory)
            {
                return true; // Место занято другой фабрикой
            }
        }
        
        return false;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }
    
    // Публичный метод для проверки, идет ли размещение в данный момент
    public bool IsPlacingFactory()
    {
        return isPlacingFactory;
    }
    
    // Метод для принудительной отмены размещения (например, при смене сцены)
    public void ForceCancelPlacement()
    {
        CancelPlacement();
    }
}