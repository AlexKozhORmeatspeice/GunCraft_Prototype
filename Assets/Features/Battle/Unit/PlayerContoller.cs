using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerContoller : MonoBehaviour
{
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitShoot unitShoot;

    private Vector3 moveDir;
    private float lastTimeShoot = -999f;

    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovementInput();
        RotatePlayerToMouse();

        HandleDash();

        HandleShoot();
    }

    private void HandleShoot()
    {
        if(Input.GetMouseButton(0))
        {
            lastTimeShoot = Time.time;
            unitShoot.Shoot(unitMovement.GetForwardDir());
        }
    }

    private void HandleDash()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            unitMovement.Dash(moveDir);
        }
    }

    private void HandleMovementInput()
    {
        moveDir = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        unitMovement.SetMoveInput(moveDir);
    }

    private void RotatePlayerToMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        
        Vector2 direction = mousePosition - transform.position;
        
        if (direction.magnitude > 0.01f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Плавный поворот
            unitMovement.SetRotation(angle);
        }
    }
}
