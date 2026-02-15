using UnityEngine;

public class SupportoMicrofono : MonoBehaviour
{    
    private string micMontatoQui = "";

    [Header("Impostazioni Asta")]
    [Tooltip("Scrivi 'Boom' o 'Ambisonic' per forzare quest'asta ad accettare SOLO quel microfono. Lascia vuoto per accettarli tutti.")]
    public string tipoMicrofonoAccettato = "";

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

    public void PiazzaMicrofono() 
    {
        GameManager gm = GameManager.instance;
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>(); 

        if (gm == null || inventario == null) return;
        if (gm.taskAttuale != GameManager.Reparto.Fonico && gm.taskAttuale != GameManager.Reparto.Regia) return;

        bool hoMicInMano = (inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono);
        string nomeMicInMano = hoMicInMano ? inventario.oggettoInMano : "";

        // --- NOVITÀ: CONTROLLO ASTA CORRETTA ---
        if (hoMicInMano && !string.IsNullOrEmpty(tipoMicrofonoAccettato))
        {
            if (!IsNameMatch(nomeMicInMano, tipoMicrofonoAccettato))
            {
                Debug.Log($"<color=orange>Quest'asta accetta solo {tipoMicrofonoAccettato}! Tu hai {nomeMicInMano}.</color>");
                return; // Ferma tutto, non te lo fa montare!
            }
        }

        // --- CASO 1: MICROFONO GIA' PRESENTE ---
        if (microfonoPiazzato)
        {
            if (hoMicInMano && nomeMicInMano != micMontatoQui)
            {
                gm.RestituisciOggettoAlTavolo(micMontatoQui); 
                ResettaSupporto(); 
            }
            else
            {
                return; // Evita loop se premi E con lo stesso mic in mano
            }
        }
        else if (!hoMicInMano)
        {
            Debug.Log("Non hai un microfono in mano!");
            return;
        }

        // --- CASO 2: MONTAGGIO ---
        string nomeMic = inventario.oggettoInMano;
        micMontatoQui = nomeMic; 
        gm.micDaInstallare = nomeMic; 
        
        GameObject micAttivato = null;
        string titoloOlogramma = "";
        string descOlogramma = "";

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
            gm.supportoPiazzato = true; 
            
            MontaggioMicrofonoCinematica cinematica = GetComponent<MontaggioMicrofonoCinematica>();
            if (cinematica != null)
            {
                cinematica.AvviaCinematicaMontaggio(micAttivato, titoloOlogramma, descOlogramma);
            }
            else
            {
                if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);
            }

            // Pulizia sicura inventario e chiusura task
            inventario.RimuoviOggetto();
            gm.ApplicaEffettoMicrofono(nomeMic);

            if (gm.taskAttuale == GameManager.Reparto.Fonico)
            {
                Debug.Log($"<color=green>Microfono piazzato! Passiamo al prossimo reparto.</color>");
                gm.CompletaTask(GameManager.Reparto.Fonico); 
            }
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