using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class MonsterAI : MonoBehaviour
{
    public enum State { Roaming, Chasing, Returning }
    protected State currentState = State.Roaming;

    [Header("Base Settings")]
    public float moveSpeed = 3f;
    public float maxHealth = 100f;
    protected float currentHealth;

    [Header("State")]
    private bool deading = false;
    public bool Die = false; // 🔑 CHỈ 1 BIẾN CHẾT DUY NHẤT

    [Header("Detection")]
    public float detectionRange = 10f;
    public float loseTargetRange = 15f;
    public float chunkMaxDistance = 20f;

    [Header("Combat")]
    public float attackInterval = 2f;
    protected float nextAttackTime;

    // ================= COMPONENTS =================
    protected Rigidbody2D rb;
    protected Animator animator;
    protected Transform player;

    // ================= MOVEMENT =================
    protected Vector3 spawnPoint;
    protected Vector3 targetMovePoint;
    protected Vector2 moveDir;
    protected Vector2 lastMoveDir = Vector2.down;

    // ================= INIT =================
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        animator = GetComponent<Animator>();

        currentHealth = maxHealth;
        spawnPoint = transform.position;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        SetRandomRoamingPoint();
    }

    // ================= UPDATE =================
    protected virtual void Update()
    {
        if (deading) return; // ⛔ CHẾT → DỪNG AI

        float distToPlayer =
            player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        float distToSpawn = Vector2.Distance(transform.position, spawnPoint);

        if (distToSpawn > chunkMaxDistance || distToPlayer > loseTargetRange)
        {
            currentState = State.Returning;
        }

        switch (currentState)
        {
            case State.Roaming:
                HandleRoaming(distToPlayer);
                break;

            case State.Chasing:
                HandleChasing(distToPlayer);
                break;

            case State.Returning:
                HandleReturning();
                break;
        }
        UpdateAnimator();
    }

    // ================= FIXED UPDATE =================
    protected virtual void FixedUpdate()
    {
        if(deading)
        {
            rb.linearVelocity = Vector2.zero;
        }
        if (Die)
        {
            Destroy(gameObject);
        }
        if (Die == false && deading == false )
        {
            rb.linearVelocity = moveDir * moveSpeed;
        }
    }

    // ================= STATES =================
    void HandleRoaming(float distToPlayer)
    {
        if (distToPlayer <= detectionRange)
        {
            currentState = State.Chasing;
            return;
        }

        MoveTowards(targetMovePoint);

        if (Vector2.Distance(transform.position, targetMovePoint) < 0.5f)
            SetRandomRoamingPoint();
    }

    protected abstract void HandleChasing(float distToPlayer);

    void HandleReturning()
    {
        MoveTowards(spawnPoint);

        if (currentHealth < maxHealth)
            currentHealth += Time.deltaTime * 5f;

        if (Vector2.Distance(transform.position, spawnPoint) < 0.5f)
        {
            currentState = State.Roaming;
            SetRandomRoamingPoint();
        }
    }

    // ================= MOVEMENT =================
    protected void MoveTowards(Vector3 pos)
    {
        moveDir = ((Vector2)pos - rb.position).normalized;
    }

    protected void StopMove()
    {
        moveDir = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }

    protected void SetRandomRoamingPoint()
    {
        targetMovePoint = spawnPoint + new Vector3(
            Random.Range(-5f, 5f),
            Random.Range(-5f, 5f),
            0
        );
    }

    // ================= ANIMATOR =================
    void UpdateAnimator()
    {
        if(Die)
        {
            animator.SetBool("Die", true);
        }
        float speed = rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);

        if (speed > 0.01f)
        {
            Vector2 dir = rb.linearVelocity.normalized;
            lastMoveDir = dir;
            animator.SetFloat("X", dir.x);
            animator.SetFloat("Y", dir.y);
        }
        else
        {
            animator.SetFloat("X", lastMoveDir.x);
            animator.SetFloat("Y", lastMoveDir.y);
        }
    }

    // ================= COMBAT =================
    public abstract void Attack();

    public void TakeDamage(float damage)
    {
        if (Die) return;

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            Die_();
        }
    }
    public void Die_()
    {
        deading = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Die",true);
    }
}
