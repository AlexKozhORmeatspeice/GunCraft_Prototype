using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private int moneyMetaForKill;
    [SerializeField] private EnemySpawner enemySpawner;

    public float getMoneyAdditionalForKill = 0;
    public float getMoneyAdditionalForKillModificator = 1;

    public static CurrencyManager instance;

    private int currentMoney;
    private int currentRunMoney;
    

    public void SetBase()
    {
        getMoneyAdditionalForKill = 0;
        getMoneyAdditionalForKillModificator = 1;
    }
    
    public int CurrentMoney 
    {
        get => currentMoney;
        set
        {
            if (value < 0)
            {
                currentMoney = 0;
                return;
            }

            currentMoney = value;
        }
    }

    public int CurrentRunMoney 
    {
        get => currentRunMoney;
        set
        {
            if (value < 0)
            {
                currentRunMoney = 0;
                return;
            }

            currentRunMoney = value;
        }
    }
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        enemySpawner.onCreateNewEnemy += OnCreateNewEnemy;

        currentMoney = 0;
        currentRunMoney = 0;
    }

    private void OnCreateNewEnemy(Unit enemy)
    {
        enemy.onDeath += OnEnemyDeath;
    }

    private void OnEnemyDeath(Unit enemy)
    {
        enemy.onDeath -= OnEnemyDeath;

        currentMoney += moneyMetaForKill;
        currentRunMoney += (int)((enemy.enemySetting.killPrice + getMoneyAdditionalForKill) * getMoneyAdditionalForKillModificator);
    }
}

