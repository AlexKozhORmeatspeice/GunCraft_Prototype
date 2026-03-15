using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Battle/UpgradeData")]
public class Upgrade : ScriptableObject
{
    [Header("Info")]
    [SerializeField] public string id;
    [SerializeField] public string name;

    [SerializeField] public bool isItemUpgarde;
    [SerializeField] public Item linkToItem;

    [Header("Stats")]
    [SerializeField] public float priceDown;
    [SerializeField] public float boostHP;
    [SerializeField] public float addHP;
    [SerializeField] public float boostGold;

    [Header("Graphic")]
    [SerializeField] public Sprite sprite; 
}
