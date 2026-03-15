using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitShoot unitShoot;
    [SerializeField] private Unit unit;
    [SerializeField] private Transform target; // Цель для преследования
    [SerializeField] private LayerMask obstacleLayer; // Слой препятствий
    
    [Header("NavMesh Settings")]
    [SerializeField] private float updatePathInterval = 0.5f; // Интервал обновления пути
    [SerializeField] private float pathEndThreshold = 0.5f; // Порог достижения точки пути
    
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f; // Дальность обнаружения цели
    [SerializeField] private float preferredDistance = 5f; // Предпочитаемая дистанция до цели
    [SerializeField] private float distanceTolerance = 1f; // Допуск по дистанции
    
    [Header("Combat Settings")]
    [SerializeField] private float accuracy = 0.9f; // Точность стрельбы (0-1)
    [SerializeField] private float rotationSpeed = 360f; // Скорость поворота оружия
    
    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 5f; // Радиус случайного блуждания
    [SerializeField] private float wanderInterval = 3f; // Интервал смены направления блуждания
    [SerializeField] private float wanderStopDuration = 2f; // Длительность остановки между блужданием
    
    // NavMesh компоненты
    private NavMeshAgent navMeshAgent;
    private bool isNavMeshAgentEnabled = false;

    private Vector3 lastTargetPosition;
    
    // Состояния AI
    private enum AIState
    {
        Wandering,      // Блуждание в поисках цели
        Approaching,    // Приближение к цели
        MaintainingDistance, // Удержание дистанции
        Searching       // Поиск цели после потери видимости
    }
    
    private AIState currentState = AIState.Wandering;
    
    // Таймеры
    private float shootTimer;
    private float pathUpdateTimer;
    private float wanderTimer;
    private float wanderStopTimer;
    private float searchTimer;
    
    // Путь
    private Vector3 targetPosition;
    private Vector3 lastKnownTargetPosition;
    private bool hasLineOfSight = false;
    private bool isMovingToPosition = false;
    
    // Параметры поиска
    [Header("Search Settings")]
    [SerializeField] private float searchDuration = 5f; // Сколько времени искать цель после потери видимости
    [SerializeField] private float searchRadius = 8f; // Радиус поиска вокруг последней известной позиции
    
    private void Start()
    {
        // Получаем компоненты, если не назначены
        if (unitMovement == null)
            unitMovement = GetComponent<UnitMovement>();
            
        if (unitShoot == null)
            unitShoot = GetComponent<UnitShoot>();
        
        // Инициализация NavMesh
        InitializeNavMesh();
        
        // Ищем цель если не назначена
        if (target == null)
            FindTarget();
            
        // Инициализация таймеров
        wanderTimer = wanderInterval;
        wanderStopTimer = 0f;
    }
    
    private void InitializeNavMesh()
    {
        // Пытаемся получить или добавить NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        // Настраиваем NavMeshAgent под нашу игру
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = unitMovement.moveSpeed;
            navMeshAgent.acceleration = unitMovement.acceleration;
            navMeshAgent.stoppingDistance = 0.1f; // Минимальная дистанция остановки
            navMeshAgent.autoBraking = true;
            navMeshAgent.radius = 0.5f;
            navMeshAgent.height = 1f;
            
            // Отключаем автоматическое обновление поворота
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;
            
            isNavMeshAgentEnabled = true;
        }
        else
        {
            Debug.LogError("NavMeshAgent не может быть создан!");
        }
    }
    
    private void Update()
    {
        if(!unit.NoEye && target != null)
        {
            lastTargetPosition = target.transform.position;
        }

        // Обновляем таймеры
        UpdateTimers();
        
        if(unit.IsStunned) return;
        
        // Проверяем наличие цели
        if (target == null)
        {
            FindTarget();
            if (target == null)
            {
                Wander();
                return;
            }
        }
        
        // Проверяем видимость цели
        CheckLineOfSight();
        
        // Обновляем состояние
        UpdateState();
        
        // Выполняем действия согласно состоянию
        PerformActions();
    }
    
    private void UpdateTimers()
    {
        shootTimer -= Time.deltaTime;
        pathUpdateTimer -= Time.deltaTime;
        wanderTimer -= Time.deltaTime;
        wanderStopTimer -= Time.deltaTime;
        searchTimer -= Time.deltaTime;
    }
    
    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            target = player.transform;
    }
    
    private void CheckLineOfSight()
    {
        if (target == null) return;
        
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        // Raycast для проверки препятствий
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleLayer);
        
        hasLineOfSight = hit.collider == null && distanceToTarget <= detectionRange;
        
        // Если есть прямая видимость, запоминаем позицию цели
        if (hasLineOfSight)
        {
            lastKnownTargetPosition = target.position;
            searchTimer = searchDuration; // Сбрасываем таймер поиска
        }
    }
    
    private void UpdateState()
    {
        if (target == null)
        {
            SetState(AIState.Wandering);
            return;
        }
        
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        
        if (hasLineOfSight)
        {
            // Цель видна - определяем, нужно ли двигаться
            float distanceDiff = distanceToTarget - preferredDistance;
            
            if (Mathf.Abs(distanceDiff) <= distanceTolerance)
            {
                // Мы на нужной дистанции - остаемся на месте
                SetState(AIState.MaintainingDistance);
            }
            else
            {
                // Нужно скорректировать дистанцию
                SetState(AIState.Approaching);
            }
        }
        else
        {
            // Цель не видна
            if (lastKnownTargetPosition != Vector3.zero && searchTimer > 0)
            {
                // Ищем цель в последней известной позиции
                SetState(AIState.Searching);
            }
            else
            {
                // Цель потеряна надолго - возвращаемся к блужданию
                SetState(AIState.Wandering);
                lastKnownTargetPosition = Vector3.zero;
            }
        }
    }
    
    private void SetState(AIState newState)
    {
        if (currentState != newState)
        {
            // Debug.Log($"AI State changed from {currentState} to {newState}");
            currentState = newState;
            
            // Сбрасываем флаг движения при смене состояния
            isMovingToPosition = false;
            
            // Останавливаем NavMeshAgent если новое состояние не требует движения
            if (newState == AIState.MaintainingDistance)
            {
                StopNavMeshAgent();
            }
        }
    }
    
    private void PerformActions()
    {
        switch (currentState)
        {
            case AIState.Wandering:
                Wander();
                break;
            case AIState.Approaching:
                Approach();
                break;
            case AIState.MaintainingDistance:
                MaintainDistance();
                break;
            case AIState.Searching:
                Search();
                break;
        }
        
        // Стреляем, если видим цель
        if (hasLineOfSight && target != null && unit.CanShoot)
        {
            TryShoot();
        }
        
        // Поворачиваем оружие в сторону цели
        RotateWeapon();
        
        // Обновляем скорость NavMeshAgent
        UpdateNavMeshAgentSpeed();
    }
    
    private void Wander()
    {
        // Если мы на остановке между перемещениями
        if (wanderStopTimer > 0f)
        {
            unitMovement.SetMoveInput(Vector2.zero);
            StopNavMeshAgent();
            return;
        }
        
        if (wanderTimer <= 0f || !isMovingToPosition)
        {
            // Генерируем новую случайную точку для блуждания
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
                SetNavMeshDestination(targetPosition);
                isMovingToPosition = true;
            }
            
            wanderTimer = wanderInterval;
        }
        
        // Проверяем, достигли ли мы цели
        if (isMovingToPosition && navMeshAgent != null && !navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= pathEndThreshold)
            {
                // Достигли точки - останавливаемся на некоторое время
                isMovingToPosition = false;
                wanderStopTimer = wanderStopDuration;
                StopNavMeshAgent();
                unitMovement.SetMoveInput(Vector2.zero);
            }
        }
    }
    
    private void Approach()
    {
        if (target == null) return;
        
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
        float distanceDiff = distanceToTarget - preferredDistance;
        
        // Рассчитываем желаемую позицию (на предпочитаемой дистанции от цели)
        Vector3 directionFromTarget = (transform.position - target.position).normalized;
        Vector3 desiredPosition;
        
        if (distanceDiff > 0)
        {
            // Мы слишком далеко - подходим ближе
            desiredPosition = target.position + directionFromTarget * preferredDistance;
        }
        else
        {
            // Мы слишком близко - отходим дальше
            desiredPosition = target.position - directionFromTarget * preferredDistance;
        }
        
        // Проверяем доступность позиции на NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredPosition, out hit, detectionRange, NavMesh.AllAreas))
        {
            SetNavMeshDestination(hit.position);
        }
        
        pathUpdateTimer = updatePathInterval;
    }
    
    private void MaintainDistance()
    {
        // Стоим на месте, но готовы стрелять
        unitMovement.SetMoveInput(Vector2.zero);
        StopNavMeshAgent();
        
        // Проверяем, не вышли ли мы за пределы допустимой дистанции
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            float distanceDiff = Mathf.Abs(distanceToTarget - preferredDistance);
            
            if (distanceDiff > distanceTolerance)
            {
                // Слишком далеко или близко - начинаем двигаться
                SetState(AIState.Approaching);
            }
        }
    }
    
    private void Search()
    {
        if (lastKnownTargetPosition == Vector3.zero) return;
        
        if (pathUpdateTimer <= 0f || !isMovingToPosition)
        {
            // Генерируем точку для поиска вокруг последней известной позиции цели
            Vector3 searchPoint = lastKnownTargetPosition + 
                                 (Vector3)Random.insideUnitCircle * searchRadius;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(searchPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                targetPosition = hit.position;
                SetNavMeshDestination(targetPosition);
                isMovingToPosition = true;
            }
            
            pathUpdateTimer = updatePathInterval * 2; // Обновляем путь реже при поиске
        }
        
        // Проверяем, достигли ли мы точки поиска
        if (isMovingToPosition && navMeshAgent != null && !navMeshAgent.pathPending)
        {
            if (navMeshAgent.remainingDistance <= pathEndThreshold)
            {
                isMovingToPosition = false;
            }
        }
    }
    
    private void TryShoot()
    {
        if (target == null) return;
        
        // Проверяем, видим ли мы цель
        if (!hasLineOfSight) return;
        
        // Стреляем с некоторой неточностью
        Vector2 directionToTarget = (target.position - transform.position).normalized;
        if(unit.NoEye)
        {
            directionToTarget = (lastTargetPosition - transform.position).normalized;
        }
        
        // Добавляем неточность
        if (accuracy < 1f)
        {
            float inaccuracy = (1f - accuracy) * 0.5f;
            Vector2 randomOffset = Random.insideUnitCircle * inaccuracy;
            directionToTarget = (directionToTarget + randomOffset).normalized;
        }
        
        unitShoot.Shoot(directionToTarget);
    }
    
    private void RotateWeapon()
    {
        if (target != null)
        {
            Vector2 directionToTarget = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            
            // Плавный поворот
            float currentAngle = unitMovement.weaponPivot.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, angle - 90, rotationSpeed * Time.deltaTime);
            
            unitMovement.SetRotation(newAngle + 90);
        }
    }
    
    #region NavMesh Helper Methods
    
    private void SetNavMeshDestination(Vector3 destination)
    {
        if (navMeshAgent != null && isNavMeshAgentEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(destination);
            
            // Конвертируем направление от NavMeshAgent в input для UnitMovement
            if (navMeshAgent.hasPath)
            {
                Vector3 directionToDestination = (navMeshAgent.steeringTarget - transform.position).normalized;
                unitMovement.SetMoveInput(new Vector2(directionToDestination.x, directionToDestination.y));
            }
        }
        else
        {
            // Fallback на старую систему если NavMesh недоступен
            Vector3 directionToDestination = (destination - transform.position).normalized;
            unitMovement.SetMoveInput(new Vector2(directionToDestination.x, directionToDestination.y));
        }
    }
    
    private void StopNavMeshAgent()
    {
        if (navMeshAgent != null && isNavMeshAgentEnabled)
        {
            navMeshAgent.ResetPath();
        }
    }
    
    private void UpdateNavMeshAgentSpeed()
    {
        if (navMeshAgent != null && isNavMeshAgentEnabled)
        {
            navMeshAgent.speed = unitMovement.moveSpeed;
            navMeshAgent.acceleration = unitMovement.acceleration;
        }
    }
    
    #endregion
    
    private void OnDisable()
    {
        if (navMeshAgent != null)
        {
            StopNavMeshAgent();
        }
    }
}