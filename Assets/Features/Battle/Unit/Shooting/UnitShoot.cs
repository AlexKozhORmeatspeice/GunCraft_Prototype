using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class BulletData
{
    public float lifetime;

    public float speed;
    public float damage;
    public float fire;
    public float water;
    public float electro;

    public float critChance; 
    public float critModificator; 

    public int shootThroughCount; 

    public int maxAmmo;
    public float shootRateBulPerMin;
    public float reloadTime;
    
    public UnitType enemyType;
    public Vector2 dir;
    public Unit source;

    public int currentAmmo;
    public bool needReload = false;
    public float lastShootTime = -999.0f;
    public float lastReloadTime = -999.0f;

    public BulletData()
    {
        speed = 15.0f;

        damage = 5;
        fire = 0;
        water = 0;
        electro = 0;
        lifetime = 1f;

        critChance = 0.0f;
        critModificator = 1.0f;

        shootThroughCount = 1;

        shootRateBulPerMin = 30.0f;
        maxAmmo = 10;
        reloadTime = 3.0f;

        currentAmmo = maxAmmo;
    }

    public BulletData(BulletData bulletData)
    {
        speed = bulletData.speed;
        damage = bulletData.damage;
        fire = bulletData.fire;
        water = bulletData.water;
        electro = bulletData.electro;
        lifetime = bulletData.lifetime;
        shootRateBulPerMin = bulletData.shootRateBulPerMin;
        maxAmmo = bulletData.maxAmmo;
        reloadTime = bulletData.reloadTime;

        critChance = bulletData.critChance;
        critModificator = bulletData.critModificator;

        shootThroughCount = bulletData.shootThroughCount; 

        currentAmmo = maxAmmo;
    }

    public string GetDataStr()
    {
        string str = "";

        str += "\n";
        str += "Speed: " + speed + "\n";
        str += "Dmg: " + damage + "\n";
        str += "Fire: " + fire + "\n";
        str += "Electro: " + electro + "\n";
        str += "Water: " + water + "\n";
        str += "ShootRate: " + shootRateBulPerMin + "\n";
        str += "MaxAmmo: " + maxAmmo + "\n";
        str += "ReloadTime: " + reloadTime + "\n";

        return str;
    }
}

public class WeaponTreeNode
{
    public WeaponTreeNode parent;
    public List<WeaponTreeNode> childs = new();
    public Item item;

    public bool isCreatedByItem = false;
}

public class UnitShoot : MonoBehaviour
{
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Inventory inventory;
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private UnitType enemyType;
    
    [Header("Settings")]
    [SerializeField] private int baseMaxAmmo = 10;
    [SerializeField] private float baseReloadTime = 3.0f;

    public WeaponTreeNode treeRoot;
    private List<BulletData> bullets = new();

    public HashSet<Item> treeItems = new();

    private float lastReloadTime = -999.0f;
    
    private int maxAmmo = 10;
    private float nowAmmo = 0;
    private bool needReload = false;

    private float reloadTime = 0;

    void Awake()
    {
        needReload = false;
        nowAmmo = baseMaxAmmo;

        maxAmmo = baseMaxAmmo;
        reloadTime = baseReloadTime;

        CreateBaseTree();
    }

    public void Shoot(Vector3 dir)
    {
        var bullets = GetBullets();

        float step = 0.5f;

        for (int i = 0; i < bullets.Count; i++)
        {
            var bullet = bullets[i];

            //Проверка на перезарядку
            if(bullet.needReload)
            {
                if(Time.time - bullet.lastReloadTime > bullet.reloadTime)
                {
                    bullet.currentAmmo = bullet.maxAmmo;
                    bullet.needReload = false;
                }
                else
                {
                    continue;
                }
            }

            if(bullet.currentAmmo <= 0 && !bullet.needReload)
            {
                bullet.lastReloadTime = Time.time;
                bullet.needReload = true;
                continue;
            }

            //Проверки на то можем ли мы стрелять этой пулей
            if(Time.time - bullet.lastShootTime < 60.0f / bullet.shootRateBulPerMin)
            {
                continue;
            }

            bullet.lastShootTime = Time.time;
            bullet.dir = dir;
            bullet.currentAmmo--;

            CreateBullet(bullet, step * (i - bullets.Count / 2));
        }
    }

    private Bullet CreateBullet(BulletData data, float offsetX)
    {
        Bullet bullet = GameObject.Instantiate(bulletPrefab, transform.position + unitMovement.GetRightDir() * offsetX, Quaternion.identity);

        bullet.SetConfig(data);

        return bullet;
    }

    private List<BulletData> GetBullets()
    {
        foreach (var bulletData in bullets)
        {
            bulletData.enemyType = enemyType;
            bulletData.source = GetComponent<Unit>();
        }

        return bullets;
    }

    public void UpdateTree()
    {
        treeItems.Clear();
        ProcessItemTree(treeRoot);

        bullets.Clear();
        BulletData bulletData = new();
        SetBullets(treeRoot, ref bulletData);
    }

    public void ClearUpdates()
    {
        CleareUpdates(treeRoot);
    }
    
    private void CreateBaseTree()
    {
        treeRoot = new();
        treeRoot.item = null;

        WeaponTreeNode node2 = new();
        treeRoot.childs.Add(node2);

        WeaponTreeNode node3 = new();
        node2.childs.Add(node3);

        WeaponTreeNode node4 = new();
        node3.childs.Add(node4);

        treeItems.Clear();
        ProcessItemTree(treeRoot);

        bullets.Clear();
        BulletData bulletData = new();
        SetBullets(treeRoot, ref bulletData);
    }
    
    private void ProcessItemTree(WeaponTreeNode weaponTreeNode)
    {
        /// Обрабатываем предметы
        // Обработка доп пули
        if(weaponTreeNode.item != null && weaponTreeNode.item.id == ItemAPI.Item_AdditionalBullet)
        {
            int childCount = weaponTreeNode.childs.Count;
            int diff = Mathf.Max(0, Mathf.Abs(weaponTreeNode.item.additionBulletsCount - childCount));
            for(int i = 0; i < diff; i++)
            {
                WeaponTreeNode newNode = new WeaponTreeNode();
                newNode.parent = weaponTreeNode;
                newNode.isCreatedByItem = true;

                weaponTreeNode.childs.Add(newNode);
            }
        }

        if(weaponTreeNode.childs.Count == 0) return;
        foreach(var child in weaponTreeNode.childs)
        {
            ProcessItemTree(child);
        }
    }

    private void CleareUpdates(WeaponTreeNode weaponTreeNode)
    {
        if(weaponTreeNode.item != null)
        {
            weaponTreeNode.item.SetBaseLevel();
        }

        if(weaponTreeNode.childs.Count == 0) return;
        foreach(var child in weaponTreeNode.childs)
        {
            ProcessItemTree(child);
        }
    }

    private void SetBullets(WeaponTreeNode weaponTreeNode, ref BulletData bullet)
    {
        //Обновляем данные пули для предмета
        if(weaponTreeNode.item != null)
        {
            Item item = weaponTreeNode.item;
            treeItems.Add(item);

            AddItemToBullet(ref bullet, item);
        }

        //Если у нас лист - добавляем данные в пули и обнуляем эффекты предмета
        if(weaponTreeNode.childs.Count == 0) 
        {
            bullets.Add(new BulletData(bullet));

            if(weaponTreeNode.item != null)
            {
                Item item = weaponTreeNode.item;
                RemoveItemFromBullet(ref bullet, item);
            }
            return;
        }
        
        //Проходим детей node
        foreach(var child in weaponTreeNode.childs)
        {
            SetBullets(child, ref bullet);
        }

        //После прохода снимаем эффекты с пули
        if(weaponTreeNode.item != null)
        {
            Item item = weaponTreeNode.item;

            RemoveItemFromBullet(ref bullet, item);
        }
    }

    private void AddItemToBullet(ref BulletData bulletData, Item itemData)
    {
        bulletData.electro += itemData.buffElectroDmg;
        bulletData.fire += itemData.buffFireDmg;
        bulletData.water += itemData.buffWaterDmg;

        bulletData.damage += itemData.buffDmg;
        bulletData.damage *= itemData.buffDmgKoef;

        bulletData.critChance += itemData.CritChance;
        bulletData.critModificator += itemData.CritModifiactor;

        bulletData.shootThroughCount += itemData.ShootThroughCount;

        bulletData.shootRateBulPerMin += Mathf.Max(1, itemData.ShootRate);

        bulletData.maxAmmo = Mathf.Max(1, itemData.AmmoAmount + bulletData.maxAmmo);

        bulletData.reloadTime = Mathf.Max(0.01f, itemData.ReloadTime + bulletData.reloadTime);

        bulletData.speed *= itemData.buffSpeedKoef;
    }

    private void RemoveItemFromBullet(ref BulletData bulletData, Item itemData)
    {
        bulletData.electro -= itemData.buffElectroDmg;
        bulletData.fire -= itemData.buffFireDmg;
        bulletData.water -= itemData.buffWaterDmg;

        bulletData.damage /= itemData.buffDmgKoef;
        bulletData.damage -= itemData.buffDmg;

        bulletData.critChance -= itemData.CritChance;
        bulletData.critModificator -= itemData.CritModifiactor;

        bulletData.shootThroughCount -= itemData.ShootThroughCount;

        bulletData.shootRateBulPerMin -= itemData.ShootRate;

        bulletData.maxAmmo = Mathf.Max(1, itemData.AmmoAmount - bulletData.maxAmmo);

        bulletData.reloadTime = Mathf.Max(0.01f, itemData.ReloadTime - bulletData.reloadTime);

        bulletData.speed /= itemData.buffSpeedKoef;
    }
}
