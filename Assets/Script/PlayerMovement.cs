using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Parametri")]
    public float velocita = 10f;
    public float gravita = -9.81f;
    
    public CharacterController controller;

    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * velocita * Time.deltaTime);

        velocity.y += gravita * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}