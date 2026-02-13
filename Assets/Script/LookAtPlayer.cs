using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Transform cameraPrincipale;

    void Start()
    {
        // Trova la camera principale all'inizio
        if (Camera.main != null)
        {
            cameraPrincipale = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cameraPrincipale != null)
        {
            // Fa ruotare l'oggetto verso la camera
            transform.LookAt(transform.position + cameraPrincipale.forward);
        }
    }
}