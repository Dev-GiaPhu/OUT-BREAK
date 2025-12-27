using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour
{
    public float speed = 10f;
    public float playerDame;
    public GameObject particle;
    private Rigidbody2D rb;
    public GameObject player;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        StartCoroutine(DestroyAfterTime(5f));
        playerDame = player.GetComponent<PlayerController>().attackDame;
    }

    void Update()
    {
        if(rb.linearVelocity.magnitude < 0.5f)
        {
            OnTriggerEnter2D(null);
        }
    }
    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    public void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger == null) return;

        if( trigger.gameObject.CompareTag("Enemy"))
        {
            trigger.GetComponent<MonsterAI>().TakeDamage(playerDame);
        }
        GameObject parti = Instantiate(
            particle,
            transform.position,
            Quaternion.identity
        );

        // ================= XOAY PARTICLE VỀ PLAYER =================
        Vector2 dirToPlayer =
            player.transform.position - parti.transform.position;

        float angle =
            (Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg)-90;

        parti.transform.rotation =
            Quaternion.Euler(0, 0, angle);
        // ============================================================

        // ================= SORTING ORDER =================
        SpriteRenderer triggerSR =
            trigger.GetComponent<SpriteRenderer>();

        if (triggerSR != null)
        {
            if (dirToPlayer.y > 0)
                parti.GetComponent<Renderer>().sortingOrder =
                    triggerSR.sortingOrder - 1;
            else
                parti.GetComponent<Renderer>().sortingOrder =
                    triggerSR.sortingOrder + 1;
        }
        // =================================================

        Destroy(gameObject);
    }

}
    