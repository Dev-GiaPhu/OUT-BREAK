using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private GameObject player => GameObject.FindWithTag("Player");
    void Update()
    {
        if(player != null)
        {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
        }
    }
}
