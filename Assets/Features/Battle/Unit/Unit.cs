using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum UnitType
{
    Player, Enemy
}

// Единый тип для всех элементов и реакций
public enum ElementType
{
    // Базовые элементы
    Fire,       // Огонь - периодический урон
    Lightning,  // Молния - цепная молния
    Water,      // Вода - блокировка стрельбы
    
    // Реакции (комбинации)
    Explosion,  // Огонь + Молния - взрыв
    Steam,      // Огонь + Вода - пар с уроном и туманом
    StunArea    // Молния + Вода - область стана
}

// Класс для хранения активного эффекта
public class ActiveEffect
{
    public ElementType element;
    public float duration;
    public float timeRemaining;
    public int stacks; // Количество стаков элемента
    public GameObject visualPrefab;
    private GameObject visualInstance;

    public void ApplyVisual(Transform parent)
    {
        if (visualPrefab != null && visualInstance == null)
        {
            Debug.Log("Added visual");
            visualInstance = GameObject.Instantiate(visualPrefab, parent);
            visualInstance.transform.SetAsLastSibling();
        }
    }

    public void RemoveVisual()
    {
        if (visualInstance != null)
        {
            GameObject.Destroy(visualInstance);
        }
    }
}

// Класс для попадания пули
public class BulletHitInfo
{
    public ElementType element;
    public int stacks;
    public Vector3 hitPoint;
    public Unit target;
    public Unit source;
}

public class Unit : MonoBehaviour
{
    [SerializeField] private UnitType unitType;
    [SerializeField] public float maxHP;
    [SerializeField] private PlayerUI playerUI;
    
    [Header("Effect Settings")]
    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject steamPrefab;
    [SerializeField] private GameObject stunAreaPrefab;
    [SerializeField] private GameObject fireVisualPrefab;
    [SerializeField] private GameObject waterVisualPrefab;
    [SerializeField] private GameObject lightningVisualPrefab;

    [Header("Settings")]
    [SerializeField] private float fireDmg = 10;
    [SerializeField] private float fireTickRate = 1f;
    [SerializeField] private float fireLenghtS = 4.0f;
    [SerializeField] private List<float> electroDamages = new();

    [SerializeField] private float blastDamage = 100.0f;
    [SerializeField] private float blastBaseRadius = 6.0f;

    [SerializeField] private float steamDamage = 10.0f;
    [SerializeField] private float steamTime = 2.0f;
    [SerializeField] private float steamRadius = 6.0f;

    [SerializeField] private float stunDamage = 25.0f;
    [SerializeField] private float stunTime = 1.0f;
    [SerializeField] private float stunRadius = 4.0f;
    [SerializeField] private bool isMain = false;

    private const float timeBetweenDamage = 0.1f;
    private float lastTimeDamage = -999f;


    public EnemySetting enemySetting;

    // События
    public event Action<Unit> onDeath;
    public event Action OnDied;
    public event Action<float> onHPChanged;
    public event Action<Unit> onShotBlocked; // Для эффекта воды (блок стрельбы)

    public float hp;
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
    private Coroutine effectProcessor;
    
    private bool canShoot = true; // Для эффекта воды
    private bool isStunned = false; // Для эффекта стана
    private bool noEye = false;

    // Публичные свойства
    public float HP => hp;
    public float NormHP => hp / maxHP;
    public UnitType Type => unitType;
    public bool CanShoot => canShoot && !isStunned;
    public bool IsStunned => isStunned;
    public bool NoEye => noEye;

    private NetworkObject networkObject;

    void Start()
    {
        networkObject = GetComponent<NetworkObject>();
        hp = maxHP;
        effectProcessor = StartCoroutine(ProcessEffects());
    }

    void OnDestroy()
    {
        if (effectProcessor != null)
            StopCoroutine(effectProcessor);
    }

    void Update()
    {
        // Обновляем статус canShoot на основе эффектов воды
        canShoot = !HasEffect(ElementType.Water);
    }

    public void ChangeHP(float changeHP)
    {
        if(Type == UnitType.Player && changeHP < 0 && Time.time - lastTimeDamage < timeBetweenDamage)
        {
            return;
        }

        lastTimeDamage = Time.time;

        float newHP = hp + changeHP;

        if (newHP > maxHP)
        {
            hp = maxHP;
            onHPChanged?.Invoke(hp);
            return;
        }

        if (newHP <= 0)
        {
            hp = 0;
            onHPChanged?.Invoke(hp);
            Die();
            return;
        }

        hp = newHP;
        onHPChanged?.Invoke(hp);
    }

    // Метод для обработки попадания пули
    public void ProcessBulletHit(BulletHitInfo hitInfo)
    {
        // Проверяем, является ли попадание реакцией
        if (IsReaction(hitInfo.element))
        {
            // Обрабатываем реакцию сразу
            ProcessReaction(hitInfo);
        }
        else
        {
            // Добавляем базовый элемент
            AddElementStacks(hitInfo.element, hitInfo.stacks, hitInfo);
            
            // Проверяем возможные реакции с существующими эффектами
            //CheckForReactions(hitInfo);
        }
    }

    private bool IsReaction(ElementType element)
    {
        return element == ElementType.Explosion || 
               element == ElementType.Steam || 
               element == ElementType.StunArea;
    }

    private void AddElementStacks(ElementType element, int stacks, BulletHitInfo hitInfo)
    {
        // Только базовые элементы могут накапливаться
        if (element != ElementType.Fire && element != ElementType.Lightning && element != ElementType.Water)
            return;

        // Ищем существующий эффект этого элемента
        ActiveEffect existingEffect = activeEffects.Find(e => e.element == element);

        if (existingEffect != null)
        {
            // Обновляем существующий эффект
            existingEffect.stacks += stacks;
            existingEffect.timeRemaining = GetEffectDuration(element);
            Debug.Log($"Добавлено {stacks} стаков {element}. Всего: {existingEffect.stacks}");
        }
        else
        {
            // Создаем новый эффект
            ActiveEffect newEffect = new ActiveEffect
            {
                element = element,
                stacks = stacks,
                duration = GetEffectDuration(element),
                timeRemaining = GetEffectDuration(element),
                visualPrefab = GetVisualPrefab(element)
            };

            activeEffects.Add(newEffect);
            newEffect.ApplyVisual(transform);
            Debug.Log($"Новый эффект {element} с {stacks} стаками");
        }

        // Если это молния, сразу применяем эффект
        if (element == ElementType.Lightning)
        {
            StartCoroutine(ApplyChainLightning(hitInfo));
        }
    }

    private float GetEffectDuration(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return 4f;      // Огонь: 4 секунды
            case ElementType.Water: return 1f;     // Вода: 1 секунда
            case ElementType.Lightning: return 0f; // Молния: моментально
            case ElementType.Explosion: return 0f; // Взрыв: моментально
            case ElementType.Steam: return 2f;     // Пар: 2 секунды
            case ElementType.StunArea: return 1f;  // Область стана: 1 секунда
            default: return 0f;
        }
    }

    private GameObject GetVisualPrefab(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: 
                Debug.Log("get prefab fire");
                return fireVisualPrefab;
            case ElementType.Water: return waterVisualPrefab;
            case ElementType.Lightning: return lightningVisualPrefab;
            default: return null;
        }
    }

    private void ProcessReaction(BulletHitInfo reactionInfo)
    {
        switch (reactionInfo.element)
        {
            case ElementType.Explosion:
                TriggerExplosionReaction(reactionInfo.hitPoint, reactionInfo.stacks, reactionInfo.source);
                break;
                
            case ElementType.Steam:
                TriggerSteamReaction(reactionInfo.hitPoint, reactionInfo.stacks);
                break;
                
            case ElementType.StunArea:
                TriggerStunAreaReaction(reactionInfo.hitPoint, reactionInfo.stacks, reactionInfo.source);
                break;
        }
    }

    private void TriggerExplosionReaction(Vector3 position, int reactionCount, Unit source)
    {
        Debug.Log($"Взрыв! Сила: {reactionCount}, Урон: {100 * reactionCount}");
        
        position.z = -6.0f;
        float area = blastBaseRadius;

        // Спавним визуальный эффект
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            Destroy(explosion, 2f);
        }

        // Находим всех врагов в радиусе
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, area);
        
        foreach (var hitCollider in hitColliders)
        {
            Unit enemy = hitCollider.GetComponent<Unit>();
            if (enemy != null && enemy != source)
            {
                // Урон: 100 за каждую реакцию
                enemy.ChangeHP(-blastDamage * reactionCount);
            }
        }
    }

    private void TriggerSteamReaction(Vector3 position, int reactionCount)
    {
        float area = 4.0f * reactionCount;

        position.z = steamRadius;
        // Спавним визуальный эффект пара
        if (steamPrefab != null)
        {
            GameObject steam = Instantiate(steamPrefab, position, Quaternion.identity);
            steam.transform.localScale = Vector3.one * (area);
            Destroy(steam, 2f);
        }

        // Находим всех врагов в радиусе
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, area);
        
        foreach (var hitCollider in hitColliders)
        {
            Unit enemy = hitCollider.GetComponent<Unit>();
            if (enemy != null)
            {
                // Пар наносит 10 урона за стак
                enemy.ChangeHP(-steamDamage * reactionCount);
                
                // TODO: Здесь нужно реализовать логику тумана (блокировка видимости)
                enemy.SetNoEye(steamTime);
            }
        }
    }

    private void TriggerStunAreaReaction(Vector3 position, int reactionCount, Unit source)
    {
        float area = stunRadius * reactionCount;

        // Спавним визуальный эффект
        if (stunAreaPrefab != null)
        {
            position.z = -6;
            GameObject stunArea = Instantiate(stunAreaPrefab, position, Quaternion.identity);
            stunArea.transform.localScale = Vector3.one * (area);
            Destroy(stunArea, 1f);
        }

        // Находим всех врагов в радиусе
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll((Vector2)position, area);
        
        foreach (var hitCollider in hitColliders)
        {
            Unit enemy = hitCollider.GetComponent<Unit>();
            if (enemy != null && enemy != source)
            {
                enemy.ChangeHP(stunDamage * reactionCount);
                
                // Накладываем стан
                enemy.Stun(stunTime);
            }
        }
    }

    private IEnumerator ApplyChainLightning(BulletHitInfo hitInfo)
    {
        Unit currentTarget = hitInfo.target;
        int remainingStacks = hitInfo.stacks;
        float currentDamage = electroDamages[0]; // Начальный урон

        for (int bounce = 0; bounce < 3 && remainingStacks > 0; bounce++)
        {
            // Наносим урон текущей цели
            if (currentTarget != null)
            {
                currentTarget.ChangeHP(-currentDamage);
                Debug.Log($"Цепная молния: урон {currentDamage}");
            }

            // Ищем следующую цель
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(currentTarget.transform.position, 10f);
            Unit nextTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                Unit potentialTarget = hitCollider.GetComponent<Unit>();
                if (potentialTarget != null && potentialTarget != currentTarget && potentialTarget != hitInfo.source)
                {
                    float distance = Vector3.Distance(currentTarget.transform.position, potentialTarget.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        nextTarget = potentialTarget;
                    }
                }
            }

            if (nextTarget == null)
                break;

            // Уменьшаем урон для следующего отскока
            currentTarget = nextTarget;
            currentDamage = (bounce == 0) ? electroDamages[1] : electroDamages[2]; // 20 -> 10 -> 5
            
            yield return new WaitForSeconds(0.1f); // Небольшая задержка между отскоками
        }

        // Уменьшаем стаки молнии
        ActiveEffect lightningEffect = activeEffects.Find(e => e.element == ElementType.Lightning);
        if (lightningEffect != null)
        {
            lightningEffect.stacks -= hitInfo.stacks;
            if (lightningEffect.stacks <= 0)
                RemoveEffect(lightningEffect);
        }
    }

    public void Stun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        Debug.Log($"{gameObject.name} оглушен на {duration} секунд");
        
        yield return new WaitForSeconds(duration);
        
        isStunned = false;
        Debug.Log($"{gameObject.name} больше не оглушен");
    }

    public void SetNoEye(float duration)
    {
        StartCoroutine(NoEyeCoroutine(duration));
    }

    private IEnumerator NoEyeCoroutine(float duration)
    {
        noEye = true;
        Debug.Log($"{gameObject.name} оглушен на {duration} секунд");
        
        yield return new WaitForSeconds(duration);
        
        noEye = false;
        Debug.Log($"{gameObject.name} больше не оглушен");
    }

    public void RemoveEffect(ActiveEffect effect)
    {
        if (activeEffects.Contains(effect))
        {
            effect.RemoveVisual();
            activeEffects.Remove(effect);
            Debug.Log($"Эффект {effect.element} удален");
        }
    }

    public bool HasEffect(ElementType element)
    {
        return activeEffects.Any(e => e.element == element);
    }

    public int GetEffectStacks(ElementType element)
    {
        ActiveEffect effect = activeEffects.Find(e => e.element == element);
        return effect?.stacks ?? 0;
    }

    private IEnumerator ProcessEffects()
    {
        WaitForSeconds wait = new WaitForSeconds(1f / fireTickRate);

        while (true)
        {
            yield return wait;

            // Обрабатываем эффект огня
            ActiveEffect fireEffect = activeEffects.Find(e => e.element == ElementType.Fire);
            if (fireEffect != null)
            {
                // Огонь наносит 10 урона в секунду (40 всего за 4 секунды)
                float damagePerTick = fireDmg / fireTickRate;
                ChangeHP(-damagePerTick * fireEffect.stacks);
                
                Debug.Log($"Огонь наносит {damagePerTick * fireEffect.stacks} урона");
            }

            // Обновляем время всех эффектов
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveEffect effect = activeEffects[i];
                
                // Молния и реакции не обновляем по времени (они или моментальные или обрабатываются отдельно)
                if (effect.element != ElementType.Lightning && 
                    effect.element != ElementType.Explosion && 
                    effect.element != ElementType.StunArea)
                {
                    effect.timeRemaining -= 1f / fireTickRate;

                    if (effect.timeRemaining <= 0)
                    {
                        RemoveEffect(effect);
                    }
                }
            }
        }
    }

    private void Die()
    {
        onDeath?.Invoke(this);
        OnDied?.Invoke();

        
        // Очищаем эффекты
        foreach (var effect in activeEffects.ToList())
        {
            RemoveEffect(effect);
        }

        if(isMain)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        Destroy(gameObject);
    }

    public NetworkObject GetNetworkObject()
    {
        return networkObject;
    }
}