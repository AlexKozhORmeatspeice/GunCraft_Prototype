using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

using Random = UnityEngine.Random;

public class Chest : MonoBehaviour
{
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private ChestItem itemPrefab;

    public Action<Chest> OnChestOpened;

    private const string itemsPath = "Run/Upgrades";
    private List<Upgrade> upgrades = new List<Upgrade>();

    public int price;

    void Awake()
    {
        upgrades = Resources.LoadAll<Upgrade>(itemsPath).ToList();
    }

    void Update()
    {
        priceText.text = "Цена: " + price.ToString();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("got trigger");
        if(CurrencyManager.instance.CurrentRunMoney < price)
        {
            return;
        }
        Debug.Log("got money");

        Unit unit = collision.GetComponentInChildren<Unit>();

        if(unit && unit.Type == UnitType.Player)
        {
            CurrencyManager.instance.CurrentRunMoney -= price;

            SpawnItem();
            OnChestOpened?.Invoke(this);
            Destroy(gameObject);
        }
    }

    private void SpawnItem()
    {
        float angle = Random.Range(0.0f, 360.0f);

        float dist = 2.0f;
        Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f);

        ChestItem chestItem = GameObject.Instantiate(itemPrefab, transform.position + dir * dist, Quaternion.identity);

        chestItem.SetUpgrade(GetAcceptUpgrade().OrderBy(x => Random.Range(0.0f, 1.0f)).FirstOrDefault()); // Устанавливаем случайный апгрейд
    }

    private List<Upgrade> GetAcceptUpgrade()
    {
        List<Upgrade> nowUpgrades = new();
        UnitShoot unitShoot = UnitManager.instance.player.GetComponent<UnitShoot>();

        foreach(var upgrade in upgrades)
        {
            if(upgrade.isItemUpgarde)
            {
                if(unitShoot.treeItems.Contains(upgrade.linkToItem))
                {
                    nowUpgrades.Add(upgrade);
                }
            }
            else
            {
                nowUpgrades.Add(upgrade);
            }
        }


        return nowUpgrades;
    }
}
