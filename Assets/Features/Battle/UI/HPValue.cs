using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPValue : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private Slider slider;

    void Update()
    {
        slider.value = unit.NormHP;
    }
}
