using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    
    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public AnimationCurve dashSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("References")]
    public Transform weaponPivot; // Дочерний объект для оружия/прицела
    
    [Header("Rotation Settings")]
    public bool rotatePlayerInstead = false; // Поворачивать всего персонажа
    public float rotationSpeed = 360f;
    
    private Vector2 currentVelocity;
    private Vector2 targetVelocity;
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    // Dash variables
    private bool isDashing = false;
    private float dashTime = 0f;
    private float lastDashTime = -999f;
    private Vector2 dashDirection;
    
    private Unit unit;

    public void SetMoveInput(Vector2 dir)
    {
        if (!isDashing) // Не меняем targetVelocity во время рывка
        {
            targetVelocity = dir.normalized;
        }
    }

    public void SetRotation(float angle)
    {
        weaponPivot.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    public Vector3 GetForwardDir()
    {
        return weaponPivot.up;
    }

    public Vector3 GetRightDir()
    {
        return weaponPivot.right;
    }
    
    public bool CanDash()
    {
        return Time.time >= lastDashTime + dashCooldown && !isDashing;
    }
    
    public void Dash(Vector3 dir)
    {
        // Проверяем можно ли сделать рывок
        if (!CanDash()) return;
        
        // Начинаем рывок
        isDashing = true;
        dashTime = 0f;
        lastDashTime = Time.time;
        
        // Сохраняем направление рывка
        dashDirection = dir.normalized;
        
        // Если направление не задано, используем текущее направление движения
        if (dashDirection == Vector2.zero)
        {
            dashDirection = targetVelocity;
            
            // Если нет направления движения, используем направление взгляда
            if (dashDirection == Vector2.zero)
            {
                dashDirection = GetForwardDir();
            }
        }
    }
    
    private void UpdateDash()
    {
        if (!isDashing) return;
        
        // Обновляем время рывка
        dashTime += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(dashTime / dashDuration);
        
        // Получаем текущую скорость из кривой анимации
        float curveValue = dashSpeedCurve.Evaluate(t);
        float currentDashSpeed = dashSpeed * curveValue;
        
        // Применяем скорость рывка
        rb.velocity = dashDirection * currentDashSpeed;
        
        // Завершаем рывок
        if (t >= 1f)
        {
            isDashing = false;
        }
    }

    void Start()
    {
        unit = GetComponent<Unit>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if(unit.IsStunned) return;
        
        if (isDashing)
        {
            UpdateDash();
        }
        else
        {
            // Плавное движение
            rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity * moveSpeed, 
                ref currentVelocity, 
                targetVelocity.magnitude > 0 ? 1f/acceleration : 1f/deceleration);
        }
    }
}