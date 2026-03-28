using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text gameCurrenyTxt;

    void Update()
    {
        gameCurrenyTxt.text = "Золота добыто: " + CurrencyManager.instance.CurrentRunMoney.ToString();
    }
}
