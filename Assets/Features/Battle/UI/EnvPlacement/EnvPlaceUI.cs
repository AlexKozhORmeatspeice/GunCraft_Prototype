using System;
using TMPro;
using UnityEngine;

public class EnvPlaceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text factoryText;
    [SerializeField] private TMP_Text gunText;
    [SerializeField] private EnvSpawner envSpawner;

    void Update()
    {
        factoryText.text = "Купить фабрику (" + envSpawner.FactoryPrice + ")"; 
        gunText.text = "Купить пушку (" + envSpawner.FactoryPrice + ")"; 
    }
}
