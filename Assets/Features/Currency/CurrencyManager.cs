using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    [SerializeField] private int moneyMetaForKill;
    [SerializeField] private int startMoney = 100;
    [SerializeField] private EnemySpawner enemySpawner;
    
    [Header("Passive Income")]
    [SerializeField] private bool enablePassiveIncome = true;
    [SerializeField] private float passiveIncomeInterval = 5f;
    [SerializeField] private int passiveIncomeAmount = 10;

    public float getMoneyAdditionalForKill = 0;
    public float getMoneyAdditionalForKillModificator = 1;

    public static CurrencyManager instance;

    private int currentMoney;
    private int currentRunMoney;
    private Coroutine passiveIncomeCoroutine;

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
        currentRunMoney = startMoney;
        
        if (enablePassiveIncome)
        {
            StartPassiveIncome();
        }
    }
    
    private void StartPassiveIncome()
    {
        if (passiveIncomeCoroutine != null)
        {
            StopCoroutine(passiveIncomeCoroutine);
        }
        
        passiveIncomeCoroutine = StartCoroutine(PassiveIncomeRoutine());
    }
    
    private IEnumerator PassiveIncomeRoutine()
    {
        while (enablePassiveIncome)
        {
            yield return new WaitForSeconds(passiveIncomeInterval);
            
            currentMoney += passiveIncomeAmount;
            currentRunMoney += passiveIncomeAmount;
        }
    }
    
    public void SetPassiveIncome(bool enable)
    {
        enablePassiveIncome = enable;
        
        if (enablePassiveIncome)
        {
            if (passiveIncomeCoroutine == null)
            {
                StartPassiveIncome();
            }
        }
        else
        {
            if (passiveIncomeCoroutine != null)
            {
                StopCoroutine(passiveIncomeCoroutine);
                passiveIncomeCoroutine = null;
            }
        }
    }
    
    public void SetPassiveIncomeInterval(float interval)
    {
        if (interval > 0)
        {
            passiveIncomeInterval = interval;
            
            if (enablePassiveIncome && passiveIncomeCoroutine != null)
            {
                StartPassiveIncome();
            }
        }
    }
    
    public void SetPassiveIncomeAmount(int amount)
    {
        passiveIncomeAmount = amount;
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
    
    private void OnDestroy()
    {
        if (passiveIncomeCoroutine != null)
        {
            StopCoroutine(passiveIncomeCoroutine);
        }
    }
}