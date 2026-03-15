using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChestSpawner : MonoBehaviour
{
    public static ChestSpawner instance;

    [SerializeField] private Chest chestPrefab;
    [SerializeField] private int basePrice = 160;
    [SerializeField] private int maxChest = 3;
    
    [Header("Price Increase Settings")]
    [SerializeField] private AnimationCurve priceIncreaseCurve = AnimationCurve.EaseInOut(0, 1, 1, 3);
    [SerializeField] private float priceMultiplier = 2f;
    [SerializeField] private bool useExponential = true;
    [SerializeField] private float exponentialBase = 1.5f;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int maxSpawnAttempts = 50;
    [SerializeField] private float minDistanceBetweenChests = 2f;
    [SerializeField] private bool showDebugVisuals = true;
    
    private List<Chest> activeChests = new List<Chest>();
    private int chestsSpawned = 0;
    private bool isSpawning = false;

    public float discountModificator = 1.0f;
    
    private void Awake()
    {
        discountModificator = 1.0f;
        instance = this;
    }

    public void SetBase()
    {
        discountModificator = 1.0f;
        chestsSpawned = 0;
        foreach(var chest in activeChests)
        {
            Destroy(chest.gameObject);
        }

        activeChests.Clear();
    }
    
    private void Update()
    {
        // Очищаем список от уничтоженных сундуков
        List<Chest> destroyedChests = new List<Chest>();
        foreach (var chest in activeChests)
        {
            if (chest == null)
            {
                destroyedChests.Add(chest);
            }
        }
        
        foreach (var chest in destroyedChests)
        {
            activeChests.Remove(chest);
        }
        
        // Спавним новый сундук если есть свободное место и не идет процесс спавна
        if (activeChests.Count < maxChest && !isSpawning)
        {
            StartCoroutine(SpawnChestWithDelay());
        }
    }
    
    private IEnumerator SpawnChestWithDelay()
    {
        isSpawning = true;
        
        // Небольшая задержка перед спавном для разнообразия
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
        
        SpawnChest();
        
        isSpawning = false;
    }
    
    private void SpawnChest()
    {
        if (chestPrefab == null) return;
        
        Vector3 spawnPosition = GetRandomNavMeshPosition();
        spawnPosition.z = -6.0f;
        
        // Создаем сундук
        Chest newChest = Instantiate(chestPrefab, spawnPosition, Quaternion.identity);
        
        // Рассчитываем цену для нового сундука
        int chestPrice = CalculatePrice();
        newChest.price = chestPrice;
        
        // Подписываемся на событие открытия сундука
        newChest.OnChestOpened += OnChestOpened;
        
        activeChests.Add(newChest);
        chestsSpawned++;
    }
    
    private int CalculatePrice()
    {
        float price = basePrice;
        
        if (useExponential)
        {
            // Экспоненциальный рост: basePrice * (exponentialBase ^ chestsSpawned)
            price = basePrice * Mathf.Pow(exponentialBase, chestsSpawned) * discountModificator;
        }
        else
        {
            // Рост по кривой: basePrice * (1 + multiplier * chestsSpawned)
            float curveValue = priceIncreaseCurve.Evaluate(chestsSpawned / (float)maxChest);
            price = basePrice * (1 + (priceMultiplier - 1) * curveValue * chestsSpawned) * discountModificator;
        }
        
        return Mathf.RoundToInt(price);
    }
    
    private Vector3 GetRandomNavMeshPosition()
    {
        List<Vector3> validPoints = new List<Vector3>();
        
        // Получаем границы NavMesh
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Выбираем случайный треугольник из NavMesh
            if (navMeshData.vertices.Length > 0)
            {
                int randomTriangle = Random.Range(0, navMeshData.indices.Length / 3);
                
                // Получаем вершины треугольника
                Vector3 v0 = navMeshData.vertices[navMeshData.indices[randomTriangle * 3]];
                Vector3 v1 = navMeshData.vertices[navMeshData.indices[randomTriangle * 3 + 1]];
                Vector3 v2 = navMeshData.vertices[navMeshData.indices[randomTriangle * 3 + 2]];
                
                // Генерируем случайную точку внутри треугольника
                float r1 = Mathf.Sqrt(Random.Range(0f, 1f));
                float r2 = Random.Range(0f, 1f);
                
                Vector3 randomPoint = (1 - r1) * v0 + r1 * (1 - r2) * v1 + r1 * r2 * v2;
                
                // Проверяем, что точка находится в радиусе спавна
                if (Vector3.Distance(randomPoint, transform.position) <= spawnRadius)
                {
                    // Проверяем, что точка не слишком близко к другим сундукам
                    bool tooClose = false;
                    foreach (var chest in activeChests)
                    {
                        if (chest != null && Vector3.Distance(randomPoint, chest.transform.position) < minDistanceBetweenChests)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    
                    if (!tooClose)
                    {
                        validPoints.Add(randomPoint);
                    }
                }
            }
        }
        
        // Если нашли точки, выбираем случайную
        if (validPoints.Count > 0)
        {
            return validPoints[Random.Range(0, validPoints.Count)];
        }
        
        // Если не нашли точек через треугольники, используем старый метод с SamplePosition
        return GetFallbackNavMeshPosition();
    }
    
    private Vector3 GetFallbackNavMeshPosition()
    {
        for (int i = 0; i < maxSpawnAttempts * 2; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection += transform.position;
            
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            {
                // Проверяем расстояние до других сундуков
                bool tooClose = false;
                foreach (var chest in activeChests)
                {
                    if (chest != null && Vector3.Distance(hit.position, chest.transform.position) < minDistanceBetweenChests)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    return hit.position;
                }
            }
        }
        
        // Если совсем не нашли позицию, возвращаем позицию спавнера
        Debug.LogWarning("Не удалось найти свободную позицию на NavMesh");
        return transform.position;
    }
    
    private void OnChestOpened(Chest chest)
    {
        chest.OnChestOpened -= OnChestOpened;
        
        if (showDebugVisuals)
        {
            Debug.Log("Сундук открыт, освобождается место для нового");
        }
        
        // Сундук будет удален из списка в Update при проверке на null
    }
    
    // Публичные методы для управления спавнером
    
    public void ResetSpawner()
    {
        // Останавливаем все корутины
        StopAllCoroutines();
        isSpawning = false;
        
        // Удаляем все активные сундуки
        foreach (var chest in activeChests)
        {
            if (chest != null)
            {
                chest.OnChestOpened -= OnChestOpened;
                Destroy(chest.gameObject);
            }
        }
        
        activeChests.Clear();
        chestsSpawned = 0;
        
        // Спавним первый сундук
        SpawnChest();
    }
    
    public void SetBasePrice(int newPrice)
    {
        basePrice = newPrice;
    }
    
    public void SetMaxChests(int newMax)
    {
        maxChest = newMax;
    }
}