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
    [SerializeField] private Unit playerUnit;

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        
        if(visible)
        {
            CurrencyManager.instance.CurrentRunMoney = 0;

            if(EnemySpawner.Instance != null)
                EnemySpawner.Instance.ClearAllEnemy();
                
            playerUnit.ChangeHP(300);
            playerUnit.GetComponent<UpgradeComponent>().ClearAllUpgrades();

            Time.timeScale = 0.0f;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1.0f;
            Cursor.visible = false;
        }

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
