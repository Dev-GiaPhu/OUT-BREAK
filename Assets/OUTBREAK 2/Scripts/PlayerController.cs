using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public GameObject player;
    public float speed = 5f;
    private Vector2 movement;

    private Rigidbody2D rb;
    private Animator ani;
    private Vector2 mousePos;
    private Camera cam;
    private bool isShooting = false;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        cam = Camera.main;
    }
    void FixedUpdate()
    {
        ani.SetFloat("TempX", ani.GetFloat("X"));
        ani.SetFloat("TempY", ani.GetFloat("Y"));
    }

    void Update()
    {

        // Logic di chuyển và bắn súng
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        player.GetComponent<Rigidbody2D>().linearVelocity = movement * speed;
        // if (Input.GetKeyDown(KeyCode.K)) isAutoFire = !isAutoFire;
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition); 
        RotateToMouse();
    }

    void RotateToMouse()
    {
        ani.SetFloat("X", movement.x);
        ani.SetFloat("Y", movement.y);
        if( movement.x == 0 && movement.y == 0)
        {
            ani.SetBool("Idle", true);
        }
        else
        {
            ani.SetBool("Idle", false);
        }
    }
}