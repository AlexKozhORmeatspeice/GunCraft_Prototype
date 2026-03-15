using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChooseSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text descText;

    public Item itemData;

    public string Desc
    {
        set => descText.text = value;
    }

    
}
