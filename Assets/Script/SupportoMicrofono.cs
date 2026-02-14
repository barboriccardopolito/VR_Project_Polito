using UnityEngine;

public class SupportoMicrofono : MonoBehaviour
{
    [Header("Modelli 3D Figli")]
    public GameObject modelloBoom;
    public GameObject modelloAmbisonic;

    [Header("Audio")]
    public AudioClip suonoPiazzamento;
    private AudioSource audioSource;

    [HideInInspector] public bool microfonoPiazzato = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 

        NascondiTutto();
    }

    public void PiazzaMicrofono() // Chiamata quando premi E sullo stativo
    {
        GameManager gm = GameManager.instance;
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>(); 

        if (gm == null || inventario == null) return;

        if (gm.taskAttuale != GameManager.Reparto.Fonico) return;

        if (microfonoPiazzato)
        {
            Debug.Log("Questo supporto ha già un microfono.");
            return;
        }

        // --- CONTROLLO CORRETTO SULL'INVENTARIO ---
        if (!inventario.haUnOggetto || inventario.categoriaInMano != OggettoRaccolta.TipoOggetto.Microfono)
        {
            Debug.Log("Non hai un microfono in mano!");
            return;
        }

        string nomeMic = inventario.oggettoInMano;
        
        // Salviamo la scelta per quando torneremo dall'NPC
        gm.micDaInstallare = nomeMic; 

        // Variabili per la Cinematica
        GameObject micAttivato = null;
        string titoloOlogramma = "";
        string descOlogramma = "";

        // RICONOSCIMENTO MICROFONO E TESTI
        if (IsNameMatch(nomeMic, "Boom")) 
        { 
            if (modelloBoom) { modelloBoom.SetActive(true); micAttivato = modelloBoom; }
            titoloOlogramma = "Microfono Boom (Shotgun)";
            descOlogramma = "Pattern polare iper-cardioide. Altissima direzionalità per isolare i dialoghi dal rumore ambientale del set.";
        }
        else if (IsNameMatch(nomeMic, "Ambisonic")) 
        { 
            if (modelloAmbisonic) { modelloAmbisonic.SetActive(true); micAttivato = modelloAmbisonic; }
            titoloOlogramma = "Microfono VR Ambisonic";
            descOlogramma = "Capsula tetraedrica. Cattura il campo sonoro a 360 gradi (A-Format) per un audio spaziale totalmente immersivo.";
        }

        if (micAttivato != null)
        {
            microfonoPiazzato = true;
            gm.supportoPiazzato = true; // Diciamo al GM che il pezzo fisico è sul set
            
            // Svuotiamo la mano del giocatore
            inventario.RimuoviOggetto();

            // --- LANCIO DELLA CINEMATICA (Se presente) ---
            MontaggioMicrofonoCinematica cinematica = GetComponent<MontaggioMicrofonoCinematica>();
            if (cinematica != null)
            {
                cinematica.AvviaCinematicaMontaggio(micAttivato, titoloOlogramma, descOlogramma);
            }
            else
            {
                // Fallback sonoro se non hai ancora messo lo script della cinematica
                if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);
            }

            Debug.Log($"<color=green>Microfono piazzato! Torna dal Fonico per il Soundcheck.</color>");
            // NON COMPLETIAMO LA TASK QUI! Ci penserà l'NPC quando andremo a parlargli.
        }
    }

    public void ResettaSupporto()
    {
        microfonoPiazzato = false;
        NascondiTutto();
    }

    void NascondiTutto()
    {
        if (modelloBoom) modelloBoom.SetActive(false);
        if (modelloAmbisonic) modelloAmbisonic.SetActive(false);
    }

    private bool IsNameMatch(string input, string target)
    {
        return input.ToLower().Contains(target.ToLower());
    }
}