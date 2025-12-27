using UnityEngine;

public class MeleeMonster : MonsterAI
{
    public float attackRange = 1.5f;
    public float damage = 25f;

    protected override void HandleChasing(float distToPlayer)
    {
        if (player == null) return;

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
        animator.SetTrigger("Attack");

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            p.GetComponent<PlayerController>()?.Hit(damage);
        }
    }
}
