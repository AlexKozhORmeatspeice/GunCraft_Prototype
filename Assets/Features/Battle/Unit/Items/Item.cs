using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Battle/ItemData")]
public class Item : ScriptableObject
{
    [Header("Info")]
    [SerializeField] public string id;
    [SerializeField] public string name;
    [SerializeField] public string desc;
    
    [Header("Stats")]
    [SerializeField] public List<float> BuffDmg;
    [SerializeField] public List<float> BuffDmgKoef;

    [SerializeField] public List<float> BuffFireDmg;
    [SerializeField] public List<float> BuffElectroDmg;
    [SerializeField] public List<float> BuffWaterDmg;

    [SerializeField] public List<float> BuffSpeedKoef;
    
    [SerializeField] public List<int> AdditionBulletsCount;

    [SerializeField] public List<float> critChance;
    [SerializeField] public List<float> critModifiactor;

    [SerializeField] public List<int> shootThroughCount;
    [SerializeField] public List<float> shootRate;
    [SerializeField] public List<int> ammoAmount;
    [SerializeField] public List<float> reloadTime;

    [Header("Magazine")]
    [SerializeField] public int price;
    [SerializeField] public bool canTakeWhenOnWeapon = true;
    [SerializeField] public bool canUpgrade = true;

    [Header("UI")]
    [SerializeField] public Sprite icon;

    public float buffDmg
    {
        get 
        {
            if(CurrentLevel > BuffDmg.Count - 1)
            {
                return BuffDmg[BuffDmg.Count - 1];
            }

            return BuffDmg[CurrentLevel];
        }
    }
    
    public float buffDmgKoef
    {
        get 
        {
            if(CurrentLevel > BuffDmgKoef.Count - 1)
            {
                return BuffDmgKoef[BuffDmgKoef.Count - 1];
            }

            return BuffDmgKoef[CurrentLevel];
        }
    }

    public float buffFireDmg
    {
        get 
        {
            if(CurrentLevel > BuffFireDmg.Count - 1)
            {
                return BuffFireDmg[BuffFireDmg.Count - 1];
            }

            return BuffFireDmg[CurrentLevel];
        }
    }

    public float buffElectroDmg
    {
        get 
        {
            if(CurrentLevel > BuffElectroDmg.Count - 1)
            {
                return BuffElectroDmg[BuffElectroDmg.Count - 1];
            }

            return BuffElectroDmg[CurrentLevel];
        }
    }

    public float buffWaterDmg
    {
        get 
        {
            if(CurrentLevel > BuffWaterDmg.Count - 1)
            {
                return BuffWaterDmg[BuffWaterDmg.Count - 1];
            }

            return BuffWaterDmg[CurrentLevel];
        }
    }

    public float buffSpeedKoef
    {
        get 
        {
            if(CurrentLevel > BuffSpeedKoef.Count - 1)
            {
                return BuffSpeedKoef[BuffSpeedKoef.Count - 1];
            }

            return BuffSpeedKoef[CurrentLevel];
        }
    }

    public int additionBulletsCount
    {
        get 
        {
            if(CurrentLevel > AdditionBulletsCount.Count - 1)
            {
                return AdditionBulletsCount[AdditionBulletsCount.Count - 1];
            }

            return AdditionBulletsCount[CurrentLevel];
        }
    }

    public float CritChance
    {
        get 
        {
            if(CurrentLevel > critChance.Count - 1)
            {
                return critChance[critChance.Count - 1];
            }

            return critChance[CurrentLevel];
        }
    }
    
    public float CritModifiactor
    {
        get 
        {
            if(CurrentLevel > critModifiactor.Count - 1)
            {
                return critModifiactor[critModifiactor.Count - 1];
            }

            return critModifiactor[CurrentLevel];
        }
    }

    public int ShootThroughCount
    {
        get 
        {
            if(CurrentLevel > shootThroughCount.Count - 1)
            {
                return shootThroughCount[shootThroughCount.Count - 1];
            }

            return shootThroughCount[CurrentLevel];
        }
    }

    public float ShootRate
    {
        get 
        {
            if(CurrentLevel > shootRate.Count - 1)
            {
                return shootRate[shootRate.Count - 1];
            }

            return shootRate[CurrentLevel];
        }
    }

    public int AmmoAmount
    {
        get 
        {
            if(CurrentLevel > ammoAmount.Count - 1)
            {
                return ammoAmount[ammoAmount.Count - 1];
            }

            return ammoAmount[CurrentLevel];
        }
    }

    public float ReloadTime
    {
        get 
        {
            if(CurrentLevel > reloadTime.Count - 1)
            {
                return reloadTime[reloadTime.Count - 1];
            }

            return reloadTime[CurrentLevel];
        }
    }
    
    public int CurrentLevel = 0;

    public void Upgrade(int levels)
    {
        CurrentLevel += levels;
    }

    public void SetBaseLevel()
    {
        CurrentLevel = 0;
    }

    public string GetDesc()
    {
        string str = "";

        str += "<b>" + name + "</b>";
        str += "\n\n";
        
        if(BuffDmg[0] != 0)
        {
            str += "Базовый урон: ";
            foreach(var dmg in BuffDmg)
            {
                str += " " + dmg + " | ";
            }
        }

        if(BuffDmgKoef[0] != 0)
        {
            str += "\n";
            str += "Множитель урона: ";
            foreach(var dmg in BuffDmgKoef)
            {
                str += " " + dmg + " | ";
            }
        }

        if(BuffFireDmg[0] != 0)
        {
            str += "\n";
            str += "Огонь: ";
            foreach(var dmg in BuffFireDmg)
            {
                str += " " + dmg + " | ";
            }
        }

        if(BuffWaterDmg[0] != 0)
        {
            str += "\n";
            str += "Вода: ";
            foreach(var dmg in BuffWaterDmg)
            {
                str += " " + dmg + " | ";
            }
        }
        
        if(BuffElectroDmg[0] != 0)
        {
            str += "\n";
            str += "электричество: ";
            foreach(var dmg in BuffElectroDmg)
            {
                str += " " + dmg + " | ";
            }
        }

        if(BuffSpeedKoef[0] != 0)
        {
            str += "\n";
            str += "Скорость пули: ";
            foreach(var dmg in BuffSpeedKoef)
            {
                str += " " + dmg + " | ";
            }
        }

        if(AdditionBulletsCount[0] != 0)
        {
            str += "\n";
            str += "Кол-во дополнительны пуль: ";
            foreach(var dmg in AdditionBulletsCount)
            {
                str += " " + dmg + " | ";
            }
        }

        if(critChance[0] != 0)
        {
            str += "\n";
            str += "Шанс нанести крит урон: ";
            foreach(var dmg in critChance)
            {
                str += " " + dmg + " | ";
            }
        }

        if(critModifiactor[0] != 0)
        {
            str += "\n";
            str += "Модификатор крит урона: ";
            foreach(var dmg in critModifiactor)
            {
                str += " " + dmg + " | ";
            }
        }

        if(shootThroughCount[0] != 0)
        {
            str += "\n";
            str += "Количество пробитий: ";
            foreach(var dmg in shootThroughCount)
            {
                str += " " + dmg + " | ";
            }
        }

        if(shootRate[0] != 0)
        {
            str += "\n";
            str += "Скорость стрельбы: ";
            foreach(var dmg in shootRate)
            {
                str += " " + dmg + " | ";
            }
        }

        if(ammoAmount[0] != 0)
        {
            str += "\n";
            str += "Количество патронов: ";
            foreach(var dmg in ammoAmount)
            {
                str += " " + dmg + " | ";
            }
        }

        if(reloadTime[0] != 0)
        {
            str += "\n";
            str += "Время перезарядки: ";
            foreach(var dmg in reloadTime)
            {
                str += " " + dmg + " | ";
            }
        }

        str += "\n";
        str += "\n";
        str += "<b>Цена</b> : " + price;

        return str;
    }
}
