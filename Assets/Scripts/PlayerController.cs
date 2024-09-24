using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed at which the player moves

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        // Move forward
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.forward;
        }

        // Move backward
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.back;
        }

        // Move left
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left;
        }

        // Move right
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right;
        }

        // Apply movement
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
