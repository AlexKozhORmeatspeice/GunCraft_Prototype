using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float canTakeDelay = 0.5f;

    private Upgrade upgrade;
    private float spawnTime;

    void Awake()
    {
        spawnTime = Time.time;
    }

    public void SetUpgrade(Upgrade upgrade)
    {
        this.upgrade = upgrade;
        spriteRenderer.sprite = upgrade.sprite;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(Time.time - spawnTime < canTakeDelay)
        {
            return;
        }
        
        UpgradeComponent upgradeComponent = collision.GetComponent<UpgradeComponent>();

        if(upgradeComponent)
        {
            if(upgrade != null)
            {
                upgradeComponent.AddUpgrade(upgrade);
            }

            Destroy(gameObject);
        }
    }
}
