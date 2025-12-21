using UnityEngine;

[CreateAssetMenu(fileName = "New Gun", menuName = "Shooter/Gun Data")]
public class GunData : ScriptableObject
{
    public string gunName;
    public Sprite gunSprite;
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    public Vector2 bulletSpawnOffset;
}