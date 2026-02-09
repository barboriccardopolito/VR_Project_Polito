using UnityEngine;

public class MacchinettaCaffe : MonoBehaviour
{
    private AudioSource audioSourceLocale;
    
    // Riferimento all'anello luminoso
    private Evidenziatore evidenziatore;

    void Start()
    {
        audioSourceLocale = GetComponent<AudioSource>();
        
        // 1. Cerca l'evidenziatore (se l'hai messo come figlio o sull'oggetto stesso)
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        // 2. Sincronizza l'audio iniziale
        if (GameManager.instance.rumoreCaffeAttivo)
        {
            if (audioSourceLocale != null && !audioSourceLocale.isPlaying) audioSourceLocale.Play();
        }
        else
        {
            if (audioSourceLocale != null) audioSourceLocale.Stop();
        }
    }

    void Update()
    {
        // --- LOGICA LUCE DI AVVERTIMENTO ---
        if (evidenziatore != null)
        {
            // La macchinetta si illumina se:
            // A. È il turno del Fonico (deve bonificare l'audio) OPPURE siamo in Revisione (Regia)
            bool faseAudio = (GameManager.instance.taskAttuale == GameManager.Reparto.Fonico);
            bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);

            // B. E la macchinetta è effettivamente ACCESA (se è spenta, non è un problema)
            bool isAccesa = GameManager.instance.rumoreCaffeAttivo;

            if ((faseAudio || faseRevisione) && isAccesa)
            {
                evidenziatore.Accendi();
            }
            else
            {
                evidenziatore.Spegni();
            }
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
            
            // Nota: L'Update al prossimo frame vedrà che 'rumoreCaffeAttivo' è false 
            // e spegnerà automaticamente l'anello.
        }
        else
        {
            Debug.Log("La macchinetta è già spenta.");
        }
    }
}