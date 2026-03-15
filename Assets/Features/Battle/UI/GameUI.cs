using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text gameCurrenyTxt;

    void Update()
    {
        currencyText.text = "Валюта мета прогрессии: " + CurrencyManager.instance.CurrentMoney.ToString();
        gameCurrenyTxt.text = "Валюта в забеге: " + CurrencyManager.instance.CurrentRunMoney.ToString();
    }
}
