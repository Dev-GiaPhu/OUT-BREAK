using UnityEngine;
using UnityEngine.InputSystem;

public class target : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.position = Mouse.current.position.ReadValue();
    }
}
