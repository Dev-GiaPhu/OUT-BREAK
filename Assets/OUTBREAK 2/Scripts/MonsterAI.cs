using UnityEngine;
using System.Collections;

public abstract class MonsterAI : MonoBehaviour
{
    public enum State { Roaming, Chasing, Returning }
    protected State currentState = State.Roaming;

    [Header("Base Settings")]
    public float moveSpeed = 3f;
    public float maxHealth = 100f;
    protected float currentHealth;

    [Header("Detection")]
    public float detectionRange = 10f;
    public float loseTargetRange = 15f; // Player chạy xa quá mức này quái sẽ bỏ
    public float chunkMaxDistance = 20f; // Khoảng cách tối đa so với tâm chunk

    [Header("Combat Settings")]
    public float attackInterval = 2f;
    protected float nextAttackTime;
    
    protected Transform player;
    protected Vector3 spawnPoint; // Tâm của chunk hoặc vị trí sinh ra
    protected Vector3 targetMovePoint;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        spawnPoint = transform.position;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        SetRandomRoamingPoint();
    }

    protected virtual void Update()
    {
        float distToPlayer = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;
        float distToSpawn = Vector3.Distance(transform.position, spawnPoint);

        // Kiểm tra nếu quái đi quá xa chunk hoặc player chạy quá xa
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
    }

    // --- Logic xử lý trạng thái ---

    void HandleRoaming(float distToPlayer)
    {
        if (distToPlayer <= detectionRange)
        {
            currentState = State.Chasing;
            return;
        }

        MoveTowards(targetMovePoint);
        if (Vector3.Distance(transform.position, targetMovePoint) < 0.5f)
            SetRandomRoamingPoint();
    }

    protected abstract void HandleChasing(float distToPlayer); // Lớp con sẽ tự định nghĩa cách đuổi

    void HandleReturning()
    {
        MoveTowards(targetMovePoint);
        
        // Hồi máu dần khi quay về
        if (currentHealth < maxHealth)
            currentHealth += Time.deltaTime * 5f; 

        if (Vector3.Distance(transform.position, targetMovePoint) < 0.5f)
        {
            currentState = State.Roaming;
            SetRandomRoamingPoint();
        }
    }

    // --- Các hàm hỗ trợ ---

    protected void MoveTowards(Vector3 position)
    {
        transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
    }

    protected void SetRandomRoamingPoint()
    {
        targetMovePoint = spawnPoint + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0);
    }

    public abstract void Attack(); // Mỗi quái tự tùy chỉnh cách đánh
}