using UnityEngine;

public class MacchinettaCaffe : MonoBehaviour
{
    private AudioSource audioSourceLocale;
    
    private Evidenziatore evidenziatore;

    private float distanzaOriginale;
    public float distanzaPerBoom = 30f;

    void Start()
    {
        audioSourceLocale = GetComponent<AudioSource>();
        
        if (audioSourceLocale != null)
        {
            distanzaOriginale = audioSourceLocale.maxDistance;
        }

        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

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
        GestisciLuce();

        GestisciRaggioAudio();
    }

    void GestisciRaggioAudio()
    {
        if (audioSourceLocale == null) return;

        string micAttuale = GameManager.instance.micScelto;

        if (!string.IsNullOrEmpty(micAttuale) && micAttuale.Contains("Boom"))
        {
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