using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image aimIcon;
    [SerializeField] private ShopScreen shop;

    void Start()
    {
        OpenShop();
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0.0f;
        
        aimIcon.transform.position = mousePos;
    }

    public void OpenShop()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        shop.SetVisible(true);
    }
}
