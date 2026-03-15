using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance;

    [SerializeField] public Unit player;
    public Action<Unit> onUnitDeath;

    private HashSet<Unit> units = new HashSet<Unit>();

    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        foreach (var unit in units)
        {
            unit.onDeath += OnUnitDeath;
        }
    }

    private void OnUnitDeath(Unit unit)
    {
        units.Remove(unit);
        onUnitDeath?.Invoke(unit);
    }
}
