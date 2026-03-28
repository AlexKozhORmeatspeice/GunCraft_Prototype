using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ShopScreen : MonoBehaviour
{
    [SerializeField] private InventoryScreen inventoryScreen;
    [SerializeField] private WeaponScreen weaponScreen;
    [SerializeField] private ChooseShopScreen chooseShopScreen;
    [SerializeField] private TMP_Text currentMoneyText;
    private Unit playerUnit;

    private void Awake()
    {
        UnitManager.onSetLocalPlayer += SetPlayer;
        SetVisible(false);
    }

    private void OnDestroy()
    {
        UnitManager.onSetLocalPlayer -= SetPlayer;
    }

    private void SetPlayer(Unit unit)
    {
        playerUnit = unit;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);

        if(visible)
        {
            inventoryScreen.Show();
            weaponScreen.Show();
            chooseShopScreen.Show();
        }
        else
        {
            inventoryScreen.Hide();
            weaponScreen.Hide();
            chooseShopScreen.Hide();
        }
    }

    void Update()
    {
        currentMoneyText.text = "Текущая валюта: " + CurrencyManager.instance.CurrentMoney.ToString();
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }
}
