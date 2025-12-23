using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GunData currentGunData; // File dữ liệu súng hiện tại
    public SpriteRenderer weaponSprite;
    public Transform firePoint;    // Điểm xuất hiện đạn
    public GameObject Gun;

    void Start()
    {
        LoadGun(currentGunData);
    }
    void Update()
    {
        if(Input.GetButtonDown("Fire1") && currentGunData != null)
        {
            Shoot();
        }
    }
    // Hàm này dùng để thay đổi súng khi nhặt được đồ mới
    public void LoadGun(GunData newData)
    {
        currentGunData = newData;
        weaponSprite.sprite = newData.gunSprite;
        Debug.Log("Weapon loaded: "+ newData.gunName);
        
        // Cập nhật vị trí đầu nòng súng (Fire Point)
        firePoint.localPosition = newData.bulletSpawnOffset;
    }

    public void Shoot()
    {
        if( currentGunData != null)
        {
            Instantiate(currentGunData.bulletPrefab, firePoint.position, Gun.transform.rotation);
        }
        else
        {
            Debug.Log("Khong co sung");
        }
    }
}