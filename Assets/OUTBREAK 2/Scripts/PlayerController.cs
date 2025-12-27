using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float maxHeath = 100f;
    public float currentHealth;
    public float attackDame = 25f;
    private Rigidbody2D rb;
    public Slider healthSlider;
    private Animator animator;
    
    [HideInInspector] public Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHeath;
    }

    void Update()
    {
        // Lấy Input di chuyển
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        HealthUpdate();

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        // Di chuyển nhân vật
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void HealthUpdate()
    {
        healthSlider.maxValue = maxHeath;
        healthSlider.value = currentHealth;
    }

    public void Hit(float dame)
    {
        currentHealth -= dame;
    }
}