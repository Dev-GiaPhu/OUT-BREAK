using UnityEngine;
using System.Collections;

public class bullet : MonoBehaviour
{
    public float speed = 10f;
    public GameObject particle;
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * speed;
        StartCoroutine(DestroyAfterTime(5f));
    }
    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //bala
        }
        else if( !collision.gameObject.CompareTag("Player") )
        {
            Instantiate(particle, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
