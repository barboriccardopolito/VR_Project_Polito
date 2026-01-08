using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float velocita = 10f;

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * moveVertical + transform.right * moveHorizontal;
        transform.Translate(move * velocita * Time.deltaTime, Space.World);
    }
}