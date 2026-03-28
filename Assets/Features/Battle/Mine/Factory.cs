using UnityEngine;
using System.Collections;

public class Factory : MonoBehaviour
{
    [Header("Production Settings")]
    [SerializeField] private float productionInterval = 5f;
    [SerializeField] private int productionAmount = 10;
    [SerializeField] private bool autoStartProduction = true;
    
    private Coroutine productionCoroutine;
    private bool isActive = true;
    
    private void Start()
    {
        if (autoStartProduction)
        {
            StartProduction();
        }
    }
    
    public void StartProduction()
    {
        if (productionCoroutine != null)
        {
            StopCoroutine(productionCoroutine);
        }
        
        productionCoroutine = StartCoroutine(ProductionRoutine());
    }
    
    public void StopProduction()
    {
        if (productionCoroutine != null)
        {
            StopCoroutine(productionCoroutine);
            productionCoroutine = null;
        }
    }
    
    private IEnumerator ProductionRoutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(productionInterval);
            
            if (CurrencyManager.instance != null)
            {
                CurrencyManager.instance.CurrentRunMoney += productionAmount;
            }
        }
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (!isActive)
        {
            StopProduction();
        }
        else if (productionCoroutine == null && autoStartProduction)
        {
            StartProduction();
        }
    }
    
    public void SetProductionInterval(float interval)
    {
        if (interval > 0)
        {
            productionInterval = interval;
            
            if (productionCoroutine != null)
            {
                StartProduction();
            }
        }
    }
    
    public void SetProductionAmount(int amount)
    {
        productionAmount = amount;
    }
    
    private void OnDestroy()
    {
        StopProduction();
    }
}