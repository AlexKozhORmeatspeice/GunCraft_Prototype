using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuyNewSlotButton : MonoBehaviour
{
    [SerializeField] private Button button;

    public WeaponTreeNode node;

    public Action<BuyNewSlotButton, WeaponTreeNode> onClickBuy;

    void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    private void OnClick()
    {
        onClickBuy?.Invoke(this, node);
    }
}
