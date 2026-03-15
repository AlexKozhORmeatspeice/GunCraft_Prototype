using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private BulletData data;
    private Vector3 moveDir;

    private float startTime = -100.0f;

    private int hitCount = 1;

    public void SetConfig(BulletData bulletData)
    {
        data = bulletData;
        hitCount = data.shootThroughCount;

        moveDir = data.dir.normalized;
        moveDir.z = 0.0f;

        startTime = Time.time;
    }

    public void Update()
    {
        CheckLifetime();
        UpdatePosition();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Unit unit = collision.GetComponentInChildren<Unit>();
        if(unit == null)
        {
            Destroy(gameObject);
            return;
        }

        if(unit.Type != data.enemyType)
        {
            return;
        }

        if(unit.Type == data.enemyType)
        {
            hitCount--;
            ProcessHit(unit);

            if(hitCount <= 0)
            {
                Destroy(gameObject);
            }

            return;
        }
    }

    private void ProcessHit(Unit unit)
    {
        float randVal = Random.Range(0.0f, 1.0f);
        if(randVal < data.critChance)
        {
            unit.ChangeHP(-data.damage * data.critModificator);
        }
        else
        {
            unit.ChangeHP(-data.damage);
        }

        List<BulletHitInfo> hitInfos = new List<BulletHitInfo>();

        // Сначала проверяем возможные реакции между элементами в одной пуле
        // 1. Проверяем реакцию Огонь + Молния
        int fire = (int)data.fire;
        int electro = (int)data.electro;
        int water = (int)data.water;

        int fireLightningReactions = Mathf.Min(fire, electro);
        if (fireLightningReactions > 0)
        {
            hitInfos.Add(new BulletHitInfo
            {
                element = ElementType.Explosion,
                stacks = fireLightningReactions,
                hitPoint = transform.position,
                target = unit,
                source = data.source
            });
            
            // Уменьшаем использованные стаки
            fire -= fireLightningReactions;
            electro -= fireLightningReactions;
        }
        
        // 2. Проверяем реакцию Огонь + Вода
        int fireWaterReactions = Mathf.Min(fire, water);
        if (fireWaterReactions > 0)
        {
            hitInfos.Add(new BulletHitInfo
            {
                element = ElementType.Steam,
                stacks = fireWaterReactions,
                hitPoint = transform.position,
                target = unit,
                source = data.source
            });
            
            // Уменьшаем использованные стаки
            fire -= fireWaterReactions;
            water -= fireWaterReactions;
        }
        
        // 3. Проверяем реакцию Молния + Вода
        int electroWaterReactions = Mathf.Min(electro, water);
        if (electroWaterReactions > 0)
        {
            hitInfos.Add(new BulletHitInfo
            {
                element = ElementType.StunArea,
                stacks = electroWaterReactions,
                hitPoint = transform.position,
                target = unit,
                source = data.source
            });
            
            // Уменьшаем использованные стаки
            electro -= electroWaterReactions;
            water -= electroWaterReactions;
        }
        
        // Добавляем оставшиеся базовые элементы
        if (fire > 0)
        {
            hitInfos.Add(new BulletHitInfo
            {
                element = ElementType.Fire,
                stacks = fire,
                hitPoint = transform.position,
                target = unit,
                source = data.source
            });
        }
        
        if (electro > 0)
        {
            hitInfos.Add(new BulletHitInfo
            {
                element = ElementType.Lightning,
                stacks = electro,
                hitPoint = transform.position,
                target = unit,
                source = data.source
            });
        }
        
        if (water > 0)
        {
            hitInfos.Add(new BulletHitInfo
            {
                element = ElementType.Water,
                stacks = water,
                hitPoint = transform.position,
                target = unit,
                source = data.source
            });
        }

        foreach(var hit in hitInfos)
        {
            unit.ProcessBulletHit(hit);
        }
    }

    private void UpdatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, transform.position + moveDir, Time.deltaTime * data.speed);

        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg + 90;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
    }

    private void CheckLifetime()
    {
        if(Time.time - startTime > data.lifetime)
        {
            Destroy(gameObject);
        }
    }
}
