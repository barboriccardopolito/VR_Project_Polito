using UnityEngine;

public class SupportoMicrofono : MonoBehaviour
{
    [Header("Visuali Microfoni")]
    public GameObject modelloBoom;      // Il modello 3D del Boom sul supporto
    public GameObject modelloAmbisonic; // Il modello 3D dell'Ambisonic sul supporto

    [Header("Audio")]
    public AudioClip suonoPiazzamento; // TRASCINA QUI IL TUO SFX (Click/Avvitamento)
    private AudioSource audioSource;

    private bool giaPiazzato = false;

    void Start()
    {
        // Setup Componente Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; // Audio 3D localizzato

        // Assicuriamoci che i microfoni siano spenti all'inizio
        NascondiTutto();
    }

    public void PiazzaMicrofono()
    {
        // Se c'è già qualcosa, fermati
        if (giaPiazzato) return;

        // Recuperiamo cosa ha in mano il giocatore dal GameManager
        string micInMano = GameManager.instance.micScelto;

        if (string.IsNullOrEmpty(micInMano))
        {
            Debug.Log("Non hai selezionato nessun microfono da piazzare!");
            return;
        }

        bool successo = false;

        // LOGICA DI PIAZZAMENTO
        // Usiamo Contains per essere sicuri (es. "Boom" trova "Microfono Boom")
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

        // SE ABBIAMO PIAZZATO CORRETTAMENTE...
        if (successo)
        {
            giaPiazzato = true;

            // --- SUONO (La parte nuova) ---
            if (suonoPiazzamento != null)
            {
                audioSource.PlayOneShot(suonoPiazzamento);
            }

            Debug.Log($"<color=green>Piazzato {micInMano} con successo!</color>");

            // Completa la Task del Fonico
            GameManager.instance.CompletaTask(GameManager.Reparto.Fonico);
        }
        else
        {
            Debug.LogWarning("Questo supporto non è adatto al microfono che hai in mano (" + micInMano + ").");
        }
    }

    // Funzione di utilità
    void NascondiTutto()
    {
        if (modelloBoom) modelloBoom.SetActive(false);
        if (modelloAmbisonic) modelloAmbisonic.SetActive(false);
    }
    
    // Chiamata se devi resettare la scena
    public void ResettaSupporto()
    {
        NascondiTutto();
        giaPiazzato = false;
    }
}