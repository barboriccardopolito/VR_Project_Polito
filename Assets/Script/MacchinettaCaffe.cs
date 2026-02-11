using UnityEngine;

public class MacchinettaCaffe : MonoBehaviour
{
    private AudioSource audioSourceLocale;
    
    // Riferimento all'anello luminoso
    private Evidenziatore evidenziatore;

    // --- VARIABILI PER GESTIONE DISTANZA ---
    private float distanzaOriginale; // Memorizza il valore dell'Inspector (es. 2)
    public float distanzaPerBoom = 30f; // Raggio esteso per il Boom (copre tutto l'ufficio)

    void Start()
    {
        audioSourceLocale = GetComponent<AudioSource>();
        
        // 1. Memorizziamo la distanza originale impostata nell'Inspector
        if (audioSourceLocale != null)
        {
            distanzaOriginale = audioSourceLocale.maxDistance;
        }

        // 2. Cerca l'evidenziatore
        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        // 3. Sincronizza l'audio iniziale
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
        // Gestione Luce
        GestisciLuce();

        // Gestione Raggio Audio (NUOVO)
        GestisciRaggioAudio();
    }

    void GestisciRaggioAudio()
    {
        if (audioSourceLocale == null) return;

        // Recupera il microfono attualmente scelto dal GameManager
        string micAttuale = GameManager.instance.micScelto;

        // Se abbiamo scelto il BOOM, aumentiamo il raggio per coprire il set
        if (!string.IsNullOrEmpty(micAttuale) && micAttuale.Contains("Boom"))
        {
            // Espandi il raggio (es. 30 metri)
            audioSourceLocale.maxDistance = distanzaPerBoom;
        }
        else
        {
            // Per Lavalier, Ambisonic o Nessuno, torna al valore originale (es. 2 metri)
            audioSourceLocale.maxDistance = distanzaOriginale;
        }
    }

    void GestisciLuce()
    {
        if (evidenziatore != null)
        {
            bool faseAudio = (GameManager.instance.taskAttuale == GameManager.Reparto.Fonico);
            bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);
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
        if (GameManager.instance.rumoreCaffeAttivo)
        {
            GameManager.instance.rumoreCaffeAttivo = false;
            
            if (audioSourceLocale != null) audioSourceLocale.Stop();
            
            Debug.Log("<color=cyan>[Ambiente]:</color> Click. Hai spento la macchinetta del caffè.");
        }
        else
        {
            Debug.Log("La macchinetta è già spenta.");
        }
    }
}