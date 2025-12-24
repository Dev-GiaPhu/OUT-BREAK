using UnityEngine;

public class MeleeMonster : MonsterAI
{
    public float attackRange = 1.5f;

    protected override void HandleChasing(float distToPlayer)
    {
        if (distToPlayer <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackInterval;
            }
        }
        else
        {
            MoveTowards(player.position);
        }
    }

    public override void Attack()
    {
        Debug.Log(gameObject.name + " vung kiếm chém!");
        // Thêm code trừ máu player ở đây
    }
}