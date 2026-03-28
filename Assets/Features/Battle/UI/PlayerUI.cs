using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private ShopScreen shop;
    [SerializeField] private EnvSpawner envSpawner;

    private bool isOpenedShop;

    void Start()
    {
        isOpenedShop = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            isOpenedShop = !isOpenedShop;

            if(isOpenedShop)
            {
                envSpawner.CancelAll();
                shop.SetVisible(true);
            }
            else
            {
                shop.SetVisible(false);
            }
        }
    }

    public void OpenShop()
    {
        isOpenedShop = true;
        shop.SetVisible(true);
        envSpawner.CancelAll();
    }

    public void CloseShop()
    {
        isOpenedShop = false;
        shop.SetVisible(false);
    }
}
