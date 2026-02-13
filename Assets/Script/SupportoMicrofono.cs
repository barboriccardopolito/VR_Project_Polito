using UnityEngine;

public class SupportoMicrofono : MonoBehaviour
{
    [Header("Visuali Microfoni")]
    public GameObject modelloBoom;
    public GameObject modelloAmbisonic;

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
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>(); // Riferimento inventario
        string micInMano = GameManager.instance.micScelto;

        if (string.IsNullOrEmpty(micInMano))
        {
            Debug.Log("Non hai selezionato nessun microfono!");
            return;
        }

        GameManager.instance.ResettaVisualeSupportiMicrofoni();

        bool successo = false;

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
            GameManager.instance.supportoPiazzato = true;

            if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);

            // --- NUOVA LOGICA: PULISCI MANO ---
            if (inventario != null)
            {
                inventario.RimuoviOggetto(); // Nasconde l'asta/treppiede dalla mano
            }

            Debug.Log($"<color=green>Piazzato {micInMano} con successo!</color>");
            GameManager.instance.CompletaTask(GameManager.Reparto.Fonico); // Chiude la task
        }
    }

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