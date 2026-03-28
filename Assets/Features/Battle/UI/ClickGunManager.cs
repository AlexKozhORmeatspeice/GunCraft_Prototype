using UnityEngine;

public class ClickGunManager : MonoBehaviour
{
    [SerializeField] private WeaponScreen weaponScreen;
    [SerializeField] private PlayerUI playerUI;

    private Camera mainCamera;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            CheckMouseGun();
        }
    }

    private void CheckMouseGun()
    {
        Vector3 mouseWP = GetMouseWorldPosition();
        RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWP, Vector2.zero);
        
        foreach (var hit in hits)
        {
            GunController gunController = hit.collider.GetComponent<GunController>();

            if (gunController && gunController.IsActive)
            {
                UnitShoot unitShoot = hit.collider.GetComponent<UnitShoot>();
                if(unitShoot)
                {
                    SetGun(unitShoot);
                    break;
                }
            }
        }
    }

    private void SetGun(UnitShoot unitShoot)
    {
        weaponScreen.SetShootComp(unitShoot);
        playerUI.OpenShop();
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z);
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }
}
