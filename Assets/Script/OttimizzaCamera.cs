using UnityEngine;

public class OttimizzaCamera : MonoBehaviour
{
    [Header("Impostazioni Performance")]
    [Range(1, 60)]
    public int fpsDesiderati = 15; // 15 FPS sono perfetti per una telecamera di sicurezza

    private Camera cam;
    private float timer;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.enabled = false; 
    }

    void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            timer += Time.deltaTime;
            
            if (timer > 1.0f / fpsDesiderati)
            {
                cam.Render();
                timer = 0;
            }
        }
    }
}