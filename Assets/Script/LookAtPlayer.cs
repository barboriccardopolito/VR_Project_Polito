using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private Transform cameraPrincipale;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraPrincipale = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cameraPrincipale != null)
        {
            transform.LookAt(transform.position + cameraPrincipale.forward);
        }
    }
}