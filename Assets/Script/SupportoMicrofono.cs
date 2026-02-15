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

    // --- AGGIUNTA EVIDENZIATORE ---
    private Evidenziatore evidenziatore;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1.0f; 

        evidenziatore = GetComponent<Evidenziatore>();
        if (evidenziatore == null) evidenziatore = GetComponentInChildren<Evidenziatore>();

        NascondiTutto();
    }

    void Update()
    {
        GestisciEvidenziatore();
    }

    void GestisciEvidenziatore()
    {
        if (evidenziatore == null || GameManager.instance == null) return;

        bool faseFonico = (GameManager.instance.taskAttuale == GameManager.Reparto.Fonico);
        bool faseRevisione = (GameManager.instance.taskAttuale == GameManager.Reparto.Regia);

        InventarioGiocatore inv = FindFirstObjectByType<InventarioGiocatore>();
        bool hoMicInMano = (inv != null && inv.haUnOggetto && inv.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono);
        string nomeMic = hoMicInMano ? inv.oggettoInMano : "";

        // Controlliamo che l'asta sia compatibile con il microfono che ho in mano!
        bool astaCorretta = true;
        if (hoMicInMano && !string.IsNullOrEmpty(tipoMicrofonoAccettato))
        {
            astaCorretta = nomeMic.ToLower().Contains(tipoMicrofonoAccettato.ToLower());
        }

        if (faseFonico)
        {
            // Si accende solo se hai un microfono in mano, l'asta è vuota, ED È L'ASTA GIUSTA!
            if (!microfonoPiazzato && hoMicInMano && astaCorretta) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
        else if (faseRevisione)
        {
            // In regia, si illumina se hai il mic corretto per cambiarlo
            if (hoMicInMano && astaCorretta && microfonoPiazzato && nomeMic != micMontatoQui) evidenziatore.Accendi();
            else evidenziatore.Spegni();
        }
        else
        {
            evidenziatore.Spegni();
        }
    }

    public void PiazzaMicrofono() 
    {
        GameManager gm = GameManager.instance;
        InventarioGiocatore inventario = FindFirstObjectByType<InventarioGiocatore>(); 

        if (gm == null || inventario == null) return;
        if (gm.taskAttuale != GameManager.Reparto.Fonico && gm.taskAttuale != GameManager.Reparto.Regia) return;

        bool hoMicInMano = (inventario.haUnOggetto && inventario.categoriaInMano == OggettoRaccolta.TipoOggetto.Microfono);
        string nomeMicInMano = hoMicInMano ? inventario.oggettoInMano : "";

        if (hoMicInMano && !string.IsNullOrEmpty(tipoMicrofonoAccettato))
        {
            if (!IsNameMatch(nomeMicInMano, tipoMicrofonoAccettato))
            {
                return; // Asta errata
            }
        }

        if (microfonoPiazzato)
        {
            if (hoMicInMano && nomeMicInMano != micMontatoQui)
            {
                gm.RestituisciOggettoAlTavolo(micMontatoQui); 
                ResettaSupporto(); 
            }
            else return; 
        }
        else if (!hoMicInMano)
        {
            return;
        }

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
            if (cinematica != null) cinematica.AvviaCinematicaMontaggio(micAttivato, titoloOlogramma, descOlogramma);
            else if (suonoPiazzamento != null) audioSource.PlayOneShot(suonoPiazzamento);

            inventario.RimuoviOggetto();
            gm.ApplicaEffettoMicrofono(nomeMic);

            if (gm.taskAttuale == GameManager.Reparto.Fonico)
            {
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