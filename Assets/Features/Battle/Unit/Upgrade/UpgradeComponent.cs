using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeComponent : MonoBehaviour
{
    private UnitShoot unitShoot;
    private Unit unit;

    void Awake()
    {
        unitShoot = GetComponent<UnitShoot>();
        unit = GetComponent<Unit>();
    }

    public void AddUpgrade(Upgrade upgrade)
    {
        if(upgrade.isItemUpgarde)
        {
            upgrade.linkToItem.Upgrade(1);
        }

        unit.maxHP += upgrade.boostHP;
        unit.hp += upgrade.addHP;

        CurrencyManager.instance.getMoneyAdditionalForKillModificator *= upgrade.boostGold;
        ChestSpawner.instance.discountModificator *= upgrade.priceDown;

        unitShoot.UpdateTree();
    }

    public void ClearAllUpgrades()
    {
        CurrencyManager.instance.SetBase();
        ChestSpawner.instance.SetBase();

        unit.maxHP = 100;
        unit.hp = unit.maxHP;

        unitShoot.ClearUpdates();
        unitShoot.UpdateTree();
    }
}
