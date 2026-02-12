using UnityEngine;

public class SupportoMicrofono : MonoBehaviour
{
    [Header("Visuali Microfoni")]
    public GameObject modelloBoom;      // Il modello 3D del Boom sul supporto
    public GameObject modelloAmbisonic; // Il modello 3D dell'Ambisonic sul supporto

    [Header("Audio")]
    public AudioClip suonoPiazzamento; 
    private AudioSource audioSource;

    private bool giaPiazzato = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 

        NascondiTutto();
    }

    public void PiazzaMicrofono()
    {
        // Se c'è già qualcosa su QUESTO supporto specifico, fermati
        // (Opzionale: se vuoi poter cambiare mic sullo stesso supporto, togli questa riga)
        // if (giaPiazzato) return; 

        string micInMano = GameManager.instance.micScelto;

        if (string.IsNullOrEmpty(micInMano))
        {
            Debug.Log("Non hai selezionato nessun microfono!");
            return;
        }

        // --- FIX: PULIZIA GLOBALE ---
        // Prima di piazzare questo, diciamo al GameManager di spegnere TUTTI gli altri microfoni
        // in scena (così se avevi messo il Boom e ora metti l'Ambisonic, il Boom sparisce).
        GameManager.instance.ResettaVisualeSupportiMicrofoni();

        bool successo = false;

        // LOGICA DI PIAZZAMENTO
        if (micInMano.Contains("Boom"))
        {
            if (modelloBoom != null) 
            { 
                modelloBoom.SetActive(true); 
                successo = true; 
            }
        }
        else if (micInMano.Contains("Ambisonic"))
        {
            if (modelloAmbisonic != null) 
            { 
                modelloAmbisonic.SetActive(true); 
                successo = true; 
            }
        }

        if (successo)
        {
            giaPiazzato = true;
            GameManager.instance.supportoPiazzato = true; // Aggiorna stato globale

            if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);

            Debug.Log($"<color=green>Piazzato {micInMano} con successo!</color>");
            GameManager.instance.CompletaTask(GameManager.Reparto.Fonico);
        }
    }

    // Questa funzione ora viene chiamata anche dal GameManager per resettare tutto
    public void ResettaSupporto()
    {
        NascondiTutto();
        giaPiazzato = false;
    }

    void NascondiTutto()
    {
        if (modelloBoom) modelloBoom.SetActive(false);
        if (modelloAmbisonic) modelloAmbisonic.SetActive(false);
    }
}