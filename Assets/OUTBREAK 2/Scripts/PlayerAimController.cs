using UnityEngine;

public class PlayerAimAndWeapon : MonoBehaviour
{
    [Header("Components")]
    private Animator animator;
    private PlayerMovement movement;
    public Camera mainCam;

    [Header("Weapon Setup")]
    public Transform gunPivot;          // Object cha của súng (Empty Object)
    public SpriteRenderer weaponSprite;  // SpriteRenderer của cây súng
    
    [Header("Settings")]
    [Range(0, 1)] public float sidePriority = 0.5f; // Ngưỡng 0.5 = 45 độ

    private float currentFinalY;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        // 1. Lấy vị trí chuột và tính hướng từ Player
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = (Vector2)mousePos - (Vector2)transform.position;

        // 2. Cập nhật hướng nhìn nhân vật (Gọi trước để có currentFinalY)
        if (lookDir.sqrMagnitude > 0.1f)
        {
            UpdateCharacterDirection(lookDir.normalized);
        }

        // 3. Điều khiển Súng (Xoay, Flip, và Sorting Layer)
        HandleWeapon(mousePos, lookDir);

        // 4. Cập nhật Speed để Animator chuyển giữa Idle và Walk
        animator.SetFloat("Speed", movement.moveInput.sqrMagnitude);
    }

    void UpdateCharacterDirection(Vector2 dir)
    {
        float finalX = 0;
        float finalY = 0;

        // Logic ưu tiên hướng ngang (Side View) khi ở góc chéo 45 độ
        if (Mathf.Abs(dir.x) > sidePriority)
        {
            finalX = (dir.x > 0) ? 1 : -1;
            finalY = 0;
        }
        else
        {
            finalX = 0;
            finalY = (dir.y > 0) ? 1 : -1;
        }

        // Lưu hướng Y để đồng bộ súng
        currentFinalY = finalY;

        // Gửi tham số vào Animator (Đảm bảo Animator có param "X" và "Y")
        animator.SetFloat("X", finalX);
        animator.SetFloat("Y", finalY);
    }

    void HandleWeapon(Vector3 mousePos, Vector2 lookDir)
    {
        // Xoay súng theo hướng chuột
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.Euler(0, 0, angle);

        // Lật súng (Flip Y) khi quay sang trái để súng không bị ngược đầu
        Vector3 gunScale = Vector3.one;
        if (angle > 90 || angle < -90)
            gunScale.y = -1f;
        else
            gunScale.y = 1f;
        gunPivot.localScale = gunScale;

        // ĐỒNG BỘ LAYER: Nếu nhân vật xoay lên (finalY = 1), súng nằm sau lưng
        if (currentFinalY > 0)
            weaponSprite.sortingOrder = 1; // Sau lưng Player
        else
            weaponSprite.sortingOrder = 3;  // Trước mặt Player
    }
}