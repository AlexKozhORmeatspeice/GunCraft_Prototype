using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance;

    public static Action<Unit> onSetLocalPlayer;

    public Unit LocalPlayer
    {
        get => localPlayer;
        set 
        {
            localPlayer = value;
            onSetLocalPlayer?.Invoke(localPlayer);
        }
    }

    private Unit localPlayer;
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

    public void Update()
    {
        
    }

    private void OnUnitDeath(Unit unit)
    {
        units.Remove(unit);
        onUnitDeath?.Invoke(unit);
    }
}
