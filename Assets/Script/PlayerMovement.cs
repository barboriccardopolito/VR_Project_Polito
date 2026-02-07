using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Parametri")]
    public float velocita = 10f;
    public float gravita = -9.81f;
    
    // Riferimento al CharacterController (trascinalo qui o lo trova da solo)
    public CharacterController controller;

    private Vector3 velocity; // Serve per calcolare la caduta (gravità)
    private bool isGrounded;  // Tocchiamo terra?

    void Start()
    {
        // Se non l'hai collegato nell'Inspector, lo cerchiamo noi
        if (controller == null) controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. Controllo se tocchiamo terra (Reset gravità)
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Valore piccolo per tenerci incollati al suolo
        }

        // 2. Input Movimento (WASD)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calcolo direzione relativa a dove guarda il player
        Vector3 move = transform.right * x + transform.forward * z;

        // 3. Muovi il Player (Qui avvengono le collisioni coi muri!)
        controller.Move(move * velocita * Time.deltaTime);

        // 4. Applica Gravità
        velocity.y += gravita * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}