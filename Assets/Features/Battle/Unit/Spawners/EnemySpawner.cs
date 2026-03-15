using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class EnemySetting
{
    public string enemyName;
    public Unit unitPrefab;
    public float weight;
    public List<string> modules;
    public float hp;
    public float speed;
    public float killPrice;
    public AnimationCurve spawnWeightOverTime = AnimationCurve.Constant(0, 1, 1);
}

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [SerializeField] private List<EnemySetting> enemyTypes;
    
    [Header("Spawn Settings")]
    [SerializeField] private int initialMaxEnemies = 5;
    [SerializeField] private int maxEnemiesLimit = 30;
    [SerializeField] private int killsToReachMax = 100; // Сколько убийств нужно для достижения максимума
    [SerializeField] private float spawnCheckInterval = 0.5f;
    [SerializeField] private bool spawnEnabled = true;
    
    [Header("Distance Settings")]
    [SerializeField] private float minSpawnDistanceFromCamera = 20f;
    [SerializeField] private float maxSpawnDistanceFromCamera = 50f;
    [SerializeField] private float minDistanceFromPlayer = 10f;
    [SerializeField] private float minDistanceBetweenEnemies = 5f;
    
    [Header("NavMesh Settings")]
    [SerializeField] private float navMeshSampleDistance = 5f;
    [SerializeField] private int maxSpawnAttemptsPerEnemy = 30;
    [SerializeField] private bool showDebugVisuals = true;
    
    private Camera mainCamera;
    private Transform playerTransform;
    private int totalKills = 0;
    private int currentMaxEnemies;
    private List<Unit> activeEnemies = new List<Unit>();

    public Action<Unit> onCreateNewEnemy;
    public Action<Unit> onEnemyDied;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }
        
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (playerTransform == null)
        {
            Debug.LogWarning("Player not found! Distance checks will be skipped.");
        }
        
        currentMaxEnemies = initialMaxEnemies;
        
        // Проверяем наличие префабов
        ValidateEnemyTypes();
        
        StartCoroutine(SpawnManager());
    }

    void ValidateEnemyTypes()
    {
        if (enemyTypes == null || enemyTypes.Count == 0)
        {
            Debug.LogError("No enemy types defined in EnemySpawner!");
            return;
        }
        
        foreach (var enemyType in enemyTypes)
        {
            if (enemyType.unitPrefab == null)
            {
                Debug.LogError($"Enemy type {enemyType.enemyName} has no unit prefab assigned!");
            }
        }
    }

    void UpdateMaxEnemiesOnKill()
    {
        totalKills++;
        
        // Рассчитываем новое максимальное количество на основе убийств
        float t = Mathf.Clamp01((float)totalKills / killsToReachMax);
        currentMaxEnemies = Mathf.RoundToInt(Mathf.Lerp(initialMaxEnemies, maxEnemiesLimit, t));
        
        if (showDebugVisuals)
        {
            Debug.Log($"Total kills: {totalKills}, Max enemies increased to: {currentMaxEnemies}");
        }
    }

    IEnumerator SpawnManager()
    {
        // Небольшая задержка перед началом спауна
        yield return new WaitForSeconds(1f);
        
        while (true)
        {
            if (!spawnEnabled)
            {
                yield return new WaitForSeconds(spawnCheckInterval);
                continue;
            }
            
            // Очищаем список от уничтоженных врагов
            activeEnemies.RemoveAll(enemy => enemy == null);
            
            // Если врагов меньше максимума, спауним недостающих
            if (activeEnemies.Count < currentMaxEnemies)
            {
                int enemiesToSpawn = currentMaxEnemies - activeEnemies.Count;
                
                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    bool spawned = false;
                    
                    for (int attempt = 0; attempt < maxSpawnAttemptsPerEnemy; attempt++)
                    {
                        if (TrySpawnEnemy())
                        {
                            spawned = true;
                            break;
                        }
                        
                        yield return new WaitForSeconds(0.1f);
                    }
                    
                    if (!spawned && showDebugVisuals)
                    {
                        Debug.LogWarning($"Failed to spawn enemy after {maxSpawnAttemptsPerEnemy} attempts");
                    }
                    
                    yield return new WaitForSeconds(0.2f);
                }
            }
            
            yield return new WaitForSeconds(spawnCheckInterval);
        }
    }

    bool TrySpawnEnemy()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        
        // Проверяем, находится ли позиция на NavMesh
        if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            // Проверка расстояния до игрока
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(hit.position, playerTransform.position);
                if (distanceToPlayer < minDistanceFromPlayer)
                {
                    return false;
                }
            }
            
            // Проверка расстояния до камеры
            float distanceFromCamera = Vector3.Distance(hit.position, mainCamera.transform.position);
            if (distanceFromCamera < minSpawnDistanceFromCamera || distanceFromCamera > maxSpawnDistanceFromCamera)
            {
                return false;
            }
            
            // Проверка расстояния до других врагов
            foreach (var enemy in activeEnemies)
            {
                if (enemy != null && Vector3.Distance(hit.position, enemy.transform.position) < minDistanceBetweenEnemies)
                {
                    return false;
                }
            }
            
            // Выбираем случайного врага с учетом веса
            EnemySetting selectedEnemyType = SelectRandomEnemyType();
            
            if (selectedEnemyType != null && selectedEnemyType.unitPrefab != null)
            {
                SpawnEnemy(selectedEnemyType, hit.position);
                return true;
            }
        }
        
        return false;
    }

    EnemySetting SelectRandomEnemyType()
    {
        if (enemyTypes == null || enemyTypes.Count == 0) return null;
        
        // Корректируем веса на основе количества убийств
        float adjustedTotalWeight = 0f;
        Dictionary<EnemySetting, float> adjustedWeights = new Dictionary<EnemySetting, float>();
        
        foreach (var enemyType in enemyTypes)
        {
            if (enemyType.unitPrefab == null) continue;
            
            float t = Mathf.Clamp01((float)totalKills / killsToReachMax);
            float timeMultiplier = enemyType.spawnWeightOverTime.Evaluate(t);
            float adjustedWeight = enemyType.weight * timeMultiplier;
            adjustedWeights[enemyType] = adjustedWeight;
            adjustedTotalWeight += adjustedWeight;
        }
        
        if (adjustedTotalWeight <= 0)
        {
            return enemyTypes.FirstOrDefault(e => e.unitPrefab != null);
        }
        
        // Выбираем случайный тип
        float randomValue = Random.Range(0f, adjustedTotalWeight);
        float cumulative = 0f;
        
        foreach (var enemyType in enemyTypes)
        {
            if (enemyType.unitPrefab == null) continue;
            
            cumulative += adjustedWeights[enemyType];
            if (randomValue <= cumulative)
            {
                return enemyType;
            }
        }
        
        return enemyTypes.FirstOrDefault(e => e.unitPrefab != null);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        
        // Генерируем случайное направление
        float angle = Random.Range(0f, 360f);
        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        
        // Случайное расстояние от камеры
        float distance = Random.Range(minSpawnDistanceFromCamera, maxSpawnDistanceFromCamera);
        
        return cameraPos + direction * distance;
    }

    void SpawnEnemy(EnemySetting enemySetting, Vector3 position)
    {
        // Спауним врага
        Unit enemy = Instantiate(enemySetting.unitPrefab, position, Quaternion.identity);
        enemy.gameObject.name = enemySetting.enemyName;

        enemy.enemySetting = enemySetting;
        
        // Применяем настройки
        enemy.maxHP = enemySetting.hp;
        enemy.hp = enemySetting.hp;
        
        var movement = enemy.GetComponent<UnitMovement>();
        if (movement != null)
        {
            movement.moveSpeed = enemySetting.speed;
        }

        // Добавляем модули
        UnitShoot unitShoot = enemy.GetComponent<UnitShoot>();
        Inventory inventory = enemy.GetComponent<Inventory>();

        if (unitShoot != null && inventory != null && enemySetting.modules != null)
        {
            WeaponTreeNode nowNode = unitShoot.treeRoot;
            foreach(var module in enemySetting.modules)
            {
                nowNode.item = inventory.GetItem(module);

                WeaponTreeNode newNode = new();
                newNode.parent = nowNode;

                nowNode.childs.Add(newNode);
                nowNode = newNode;
            }
            unitShoot.UpdateTree();
        }
        
        // Добавляем врага в список активных
        activeEnemies.Add(enemy);
        
        // Подписываемся на событие смерти
        enemy.OnDied += () => OnEnemyDied(enemy);
        
        onCreateNewEnemy?.Invoke(enemy);
        
        if (showDebugVisuals)
        {
            Debug.Log($"Спавн врага {enemySetting.enemyName} на позиции {position}. Всего врагов: {activeEnemies.Count}/{currentMaxEnemies}");
        }
    }

    void OnEnemyDied(Unit enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            
            // Увеличиваем максимальное количество врагов при убийстве
            UpdateMaxEnemiesOnKill();
            
            onEnemyDied?.Invoke(enemy);
            
            if (showDebugVisuals)
            {
                Debug.Log($"Враг уничтожен. Всего убийств: {totalKills}, Макс. врагов: {currentMaxEnemies}, Текущих: {activeEnemies.Count}");
            }
        }
    }

    public void ClearAllEnemy()
    {
        // Создаем копию списка, так как будем его модифицировать
        var enemiesToRemove = new List<Unit>(activeEnemies);
        
        foreach (var enemy in enemiesToRemove)
        {
            if (enemy != null)
            {
                enemy.OnDied -= () => OnEnemyDied(enemy);
                Destroy(enemy.gameObject);
            }
        }
        
        activeEnemies.Clear();
        totalKills = 0;
        currentMaxEnemies = initialMaxEnemies;
        
        Debug.Log("All enemies cleared");
    }

    public void RegisterEnemyDeath(Unit enemy)
    {
        OnEnemyDied(enemy);
    }

    public void AddKill()
    {
        totalKills++;
        UpdateMaxEnemiesOnKill();
    }

    public int GetCurrentMaxEnemies()
    {
        return currentMaxEnemies;
    }

    public int GetCurrentEnemyCount()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
        return activeEnemies.Count;
    }

    public int GetTotalKills()
    {
        return totalKills;
    }

    public void SetSpawnEnabled(bool enabled)
    {
        spawnEnabled = enabled;
    }

    // Визуализация для отладки
    private void OnDrawGizmosSelected()
    {
        if (showDebugVisuals && mainCamera != null)
        {
            // Рисуем зоны спауна
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(mainCamera.transform.position, minSpawnDistanceFromCamera);
            
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(mainCamera.transform.position, maxSpawnDistanceFromCamera);
            
            // Рисуем запретную зону вокруг игрока
            if (playerTransform != null)
            {
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawWireSphere(playerTransform.position, minDistanceFromPlayer);
            }
        }
    }
}