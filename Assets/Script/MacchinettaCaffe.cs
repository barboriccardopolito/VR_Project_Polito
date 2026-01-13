using UnityEngine;

public class MacchinettaCaffe : MonoBehaviour
{
    private AudioSource audioSourceLocale;

    void Start()
    {
        audioSourceLocale = GetComponent<AudioSource>();
        
        // Sincronizza lo stato iniziale
        if (GameManager.instance.rumoreCaffeAttivo)
        {
            if (audioSourceLocale != null && !audioSourceLocale.isPlaying) audioSourceLocale.Play();
        }
        else
        {
            if (audioSourceLocale != null) audioSourceLocale.Stop();
        }
    }

    public void SpegniMacchinetta()
    {
        // Se è accesa, la spegniamo
        if (GameManager.instance.rumoreCaffeAttivo)
        {
            GameManager.instance.rumoreCaffeAttivo = false;
            
            if (audioSourceLocale != null) audioSourceLocale.Stop();
            
            Debug.Log("<color=cyan>[Ambiente]:</color> Click. Hai spento la macchinetta del caffè.");
        }
        else
        {
            // Se vuoi riaccenderla
            Debug.Log("La macchinetta è già spenta.");
        }
    }
}