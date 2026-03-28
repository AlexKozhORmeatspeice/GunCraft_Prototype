using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPValue : MonoBehaviour
{
    [SerializeField] private Unit unit;
    [SerializeField] private Slider slider;

    void Awake()
    {
        UnitManager.onSetLocalPlayer += SetPlayer;
    }

    void OnDestroy()
    {
        UnitManager.onSetLocalPlayer -= SetPlayer;
    }

    private void SetPlayer(Unit _unit)
    {
        unit = _unit;
    }

    void Update()
    {
        if(unit != null)
            slider.value = unit.NormHP;
    }
}
