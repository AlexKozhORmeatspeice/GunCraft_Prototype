using UnityEngine;
using System.Linq;

public class GunController : MonoBehaviour
{
    [SerializeField] private UnitShoot unitShoot;
    
    private bool isActive = false;
    private float lastShootTime = 0f;
    
    public bool IsActive => isActive;

    private void Update()
    {
        if (!isActive) return;
        
        // Ищем ближайшего врага
        Unit nearestEnemy = FindNearestEnemy();
        if(nearestEnemy == null) return;
        
        // Вычисляем направление на врага
        Vector3 direction = (nearestEnemy.transform.position - transform.position).normalized;
        
        // Стреляем в сторону врага
        unitShoot.Shoot(direction);
    }
    
    private Unit FindNearestEnemy()
    {
        // Находим все объекты с компонентом Unit
        Unit[] allUnits = FindObjectsOfType<Unit>();
        
        // Фильтруем только врагов
        var enemies = allUnits.Where(unit => unit.Type == UnitType.Enemy);
        
        Unit nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Unit enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            
            // Проверяем радиус поиска
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        
        return nearestEnemy;
    }
    
    public void ChangeActive(bool _isActive)
    {
        isActive = _isActive;
    }
}